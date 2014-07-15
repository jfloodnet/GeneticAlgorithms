using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class Chromosome
    {
        Random rand = new Random();
        static readonly int GENO_SIZE = 4;
        static readonly int CHROMO_SIZE = 75 * GENO_SIZE;

        readonly Func<string, string> decode;
        private float answer;
        
        public readonly string Bits;
        public readonly float Fitness;
       
        public float Result
        {
            get
            {
                return answer;
            }
        }

        private Chromosome(string bits, float fitness, float answer, Func<string, string> decoder)
        {
            Bits = bits;
            Fitness = fitness;
            this.answer = answer;
            this.decode = decoder;
        }

        public static Chromosome New(GeneticCode code)
        {
            return New(() => code.Generate(CHROMO_SIZE), bits => code.Decode(bits));
        }

        public static Chromosome New(Func<string> generateCode, Func<string,string> decoder)
        {
            return new Chromosome(generateCode(), 0, 0, decoder);
        }

        public Chromosome CalculateFitness(float goal)
        {
            var answer = this.Answer();
            if (Math.Abs(answer - goal) < float.Epsilon)
                return new Chromosome(Bits, float.MaxValue, answer, decode);
            return new Chromosome(Bits, 1 / Math.Abs(goal - answer), answer, decode);
        }

        public float Answer()
        {
            var calculation = decode(Bits);

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

        public Chromosome Crossover(ref Chromosome mate, double rate)
        {
            if (rate > 1) throw new Exception("Rate cant exceed 1");
            if (rand.NextDouble() >= rate) return this;

            int position = (int)(rate * Bits.Length);
            string firstHalf = Bits.Substring(0, position);
            string lastHalf = mate.Bits.Substring(position);

            mate = new Chromosome(firstHalf + lastHalf, 0, 0, mate.decode);
            return new Chromosome(lastHalf + firstHalf, 0, 0, decode);
        }

        public Chromosome Mutate(double mutationRate)
        {
            var newGenes = Bits.Select(x =>
            {
                if (rand.NextDouble() <= mutationRate)
                    return x == '0' ? '1' : '0';
                else
                    return x;

            });
            return new Chromosome(
                new string(newGenes.ToArray()), 0, 0, decode);
        }
    }
}
