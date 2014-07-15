using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class Chromosome
    {
        
        static readonly int GENO_SIZE = 4;
        static readonly int CHROMO_SIZE = 20 * GENO_SIZE;

        readonly Random rand = new Random();
        readonly Func<string, string> decode;
        
        public readonly string Bits;
        public readonly float Target;
        public readonly float Fitness;
        public readonly float Answer;

        private Chromosome(float target, string bits, Func<string, string> decoder)
        {
            this.Bits = bits;            
            this.decode = decoder;
            this.Target = target;

            Answer = CalculateAnswer(decoder(bits));
            Fitness = CalculateFitness(target, this.Answer);
        }

        public static Chromosome New(float target, GeneticCode code)
        {
            return New(target, () => code.Generate(CHROMO_SIZE), bits => code.Decode(bits));
        }

        public static Chromosome New(float target, Func<string> generateCode, Func<string,string> decoder)
        {
            return new Chromosome(target, generateCode(), decoder);
        }
                
        public Chromosome Crossover(Chromosome mate, double rate)
        {
            if (rate > 1) throw new Exception("Rate cant exceed 1");
            if (rand.NextDouble() >= rate) return this;

            int position = (int)(rate * Bits.Length);
            string firstHalf = Bits.Substring(0, position);
            string lastHalf = mate.Bits.Substring(position);

            return new Chromosome(Target, lastHalf + firstHalf, decode);
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

            return new Chromosome(Target, new string(newGenes.ToArray()), decode);
        }

        private static float CalculateFitness(float goal, float answer)
        {            
            if (Math.Abs(answer - goal) < float.Epsilon)
                return float.MaxValue;
            return 1 / Math.Abs(goal - answer);
        }

        private static float CalculateAnswer(string calculation)
        {
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

            return result; 
        
        }
    }
}
