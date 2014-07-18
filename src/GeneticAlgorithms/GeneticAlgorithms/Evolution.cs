using System;
using System.Linq;

namespace GeneticAlgorithms
{
    public class Evolution
    {
        private readonly Random random = new Random();

        private readonly float target;
        private readonly Population seed;

        public Evolution(float target, Population seed)
        {
            this.target = target;
            this.seed = seed;
        }

        public Solution FindSolution(double crossOverRate, double mutationRate, int maxgenerations)
        {
            int genCount = 0;
            Population currentGeneration = seed;
            while (genCount < maxgenerations)
            {
                genCount++;
                var nextGeneration =
                    ParallelEnumerable.Range(1, currentGeneration.Count())
                        .Select(__ =>
                        {
                            var pair = currentGeneration.SelectPairs();
                            var male = pair.Item1;
                            var female = pair.Item2;
                            return
                                male.Crossover(female, crossOverRate)
                                    .Mutate(mutationRate);
                        }).ToList();

                var possibleSolution =
                    nextGeneration.FirstOrDefault(
                        offSpring => offSpring.EqualsAnswer(target));

                currentGeneration =
                    new Population(target, nextGeneration, random, possibleSolution, genCount);

                if (possibleSolution != null)
                {
                    //got the loot lets get out
                    return new Solution(true, target, possibleSolution.Answer, possibleSolution.Fitness, possibleSolution.Bits, genCount, currentGeneration);
                }
            }

            return new Solution(false, target, 0.0f, 0.0f, "", genCount, currentGeneration);
        }

        public class Solution
        {
            public readonly bool SolutionFound;
            public readonly float Target;
            public readonly float Answer;
            public readonly float Fitness;
            public readonly string Bits;
            public readonly int GenCount;
            public readonly Population Population;

            public Solution(bool solutionFound, float target,  float answer, float fitness, string bits,int genCount, Population population)
            {
                SolutionFound = solutionFound;
                Target = target;
                Answer = answer;
                Fitness = fitness;
                Bits = bits;
                GenCount = genCount;
                Population = population;
            }
        }
    }
}