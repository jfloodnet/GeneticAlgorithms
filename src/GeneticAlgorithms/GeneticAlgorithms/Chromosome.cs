using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class Chromosome
    {
        const int genoSize = 4;
        const int chromoSize = 75 * genoSize;

        public readonly string Bits;
        public readonly float Fitness;

        public string DecodedString
        {
            get
            {
                return Decode(Bits);
            }
        }

        private float answer;
        public float Result
        {
            get
            {
                return answer;
            }
        }

        public Func<string, string> Decode = bits => DecodeImpl(bits);
        
        public static readonly Dictionary<string, string> GenoTypes = InitGenoTypes();

        private Chromosome(string bits, float fitness, float answer)
        {
            Bits = bits;
            Fitness = fitness;
            this.answer = answer;
        }



        public static Chromosome New(Random random)
        {
            return New(() => GenerateCodeImpl(random));
        }

        public static Chromosome New(Func<string> generateCode)
        {
            return new Chromosome(generateCode(), 0, 0);
        }

        public Chromosome CalculateFitness(float goal)
        {
            var answer = this.Answer();
            if (Math.Abs(answer - goal) < float.Epsilon)
                return new Chromosome(Bits, float.MaxValue, answer);
            return new Chromosome(Bits, 1 / Math.Abs(goal - answer), answer);
        }


        public float Answer()
        {
            var calculation = Decode(Bits);

            float result = 0;
            char lastOperator = '+';
            foreach (var c in calculation)
            {
                int operand = 0;
                if (int.TryParse(c.ToString(), out operand))
                {
                    if (lastOperator == '+')
                        result = result + operand;
                    else if (lastOperator == '*')
                        result = result * operand;
                    if (lastOperator == '-')
                        result = result - operand;
                    else if (lastOperator == '/')
                        result = result / operand;
                }
                else
                {
                    lastOperator = c;
                }
            }

            return answer = result; 
        }

        public Chromosome MateWith(Chromosome female, double crossoverRate, double mutationRate)
        {            
            var baby = Crossover(female, crossoverRate);
            return baby.Mutate(mutationRate);
        }

        public Chromosome Crossover(Chromosome mate, double rate)
        {
            if (rate > 1) throw new Exception("Rate cant exceed 1");

            int position = (int)(rate * Bits.Length);
            return new Chromosome(
                Bits.Substring(0, position) + mate.Bits.Substring(position),
                0, 0);
        }

        public Chromosome Mutate(double mutationRate)
        {
            Random rand = new Random();
            var newGenes = Bits.Select(x =>
            {
                if (rand.NextDouble() <= mutationRate)
                    return x == '0' ? '1' : '0';
                else
                    return x;

            });
            return new Chromosome(new string(newGenes.ToArray()), 0, 0);
        }

        private static string GenerateCodeImpl(Random random)
        {
            return new string(
                Enumerable.Range(0, chromoSize)
                    .Select(_ => random.NextDouble() > .5 ? '1' : '0')
                    .ToArray());            
        }

        private static string DecodeImpl(string bits)
        {
            return DecodeImpl("", bits, true, GenoTypes);
        }

        private static string DecodeImpl(string calculation, string remainder, bool lastSawOperator, Dictionary<string, string> genoTypes)
        {
            if (remainder.Length <= 0) return calculation;

            var operators = new[] { "+", "*", "/", "-" };
            var geno = remainder.Substring(0, 4);
            var tail = remainder.Substring(4);
            string val = null;
            if (genoTypes.TryGetValue(geno, out val) && 
                lastSawOperator != operators.Contains(val))          
                return DecodeImpl(
                    string.Concat(calculation, val), 
                    tail, !lastSawOperator, genoTypes);                
            
            //skip if we dont recognise the geno;
            return DecodeImpl(calculation, tail, lastSawOperator, genoTypes);
        }

        private static Dictionary<string, string> InitGenoTypes()
        {
            var genoTypes = new Dictionary<string, string>();
            genoTypes.Add("0001", "1");
            genoTypes.Add("0010", "2");
            genoTypes.Add("0011", "3");
            genoTypes.Add("0100", "4");
            genoTypes.Add("0101", "5");
            genoTypes.Add("0110", "6");
            genoTypes.Add("0111", "7");
            genoTypes.Add("1000", "8");
            genoTypes.Add("1001", "9");
            genoTypes.Add("1010", "+");
            genoTypes.Add("1011", "-");
            genoTypes.Add("1100", "*");
            genoTypes.Add("1101", "/");
            return genoTypes;
        }   
    }
}
