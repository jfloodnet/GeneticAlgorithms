    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class Population
    {
        private readonly Random random;
        private readonly IList<Chromosome> chromos;
        
        private readonly Chromosome solution;
        private readonly int generation;

        public readonly Func<string, string> Decode = s => "";

        public int Count()
        {
            return chromos.Count();
        }

        public int Generation()
        {
            return generation;
        }

        private Population(IEnumerable<Chromosome> chromos, Random r, Func<string, string> decoder)
        {
            this.chromos = chromos.ToList();
            this.random = r;
            this.Decode = decoder;
        }

        private Population(IEnumerable<Chromosome> chromos, Random r, Chromosome solution, int gen)
        {
            this.chromos = chromos.ToList();
            this.solution = solution;
            this.generation = gen;
            this.random = r;
        }

        public static Population NewPopulation(int size)
        {
            Random randomness = new Random();
            var decoder = new GeneticCode(randomness);
            return new Population(
                Enumerable.Range(0, size)
                    .Select(id => 
                        Chromosome.New(
                            new GeneticCode(randomness))), 
                        randomness, 
                        bits => decoder.Decode(bits));
        }

        public IEnumerable<Chromosome> All() { return chromos; }

        public Chromosome Solution()
        {
            return solution;
        }

        public Chromosome BestAnswer()
        {
            return chromos.Aggregate((m, f) => m.Fitness >= f.Fitness ? m : f);
        }

        public Tuple<Chromosome,Chromosome> SelectPairs(float goal)
        {
            IList<Chromosome> candidates = chromos.Select(x => x.CalculateFitness(goal)).ToList();
            return new Tuple<Chromosome, Chromosome>(Select(candidates), Select(candidates));
        }

        private Chromosome Select(IList<Chromosome> candidates)
        {
            float totalFitness = candidates.Sum(x => x.Fitness);
            float slice = (float)(totalFitness * random.NextDouble());

            float fitnessSoFar = 0.0F;
            return Enumerable.Range(0, candidates.Count()).Select(i =>
            {
                var item = candidates[i];
                fitnessSoFar += candidates[i].Fitness;
                if (fitnessSoFar >= slice)
                {
                    return item;
                }

                var any = candidates.Last();
                return any;
            }).First();      
        }

        public Population FindSolution(float goal, double crossOverRate, double mutationRate, int maxgenerations)
        {
            int generations = 0;
            int maxgens = maxgenerations;

            Population currentGeneration = this;

            while (generations < maxgens)
            {
                generations++;

                var nextGeneration =
                Enumerable.Range(1, currentGeneration.Count() / 2)
                .SelectMany(_ =>
                {
                    var pair = currentGeneration.SelectPairs(goal);
                    var male = pair.Item1;
                    var female = pair.Item2;

                    male = male.Crossover(ref female, crossOverRate);
                    male = male.Mutate(mutationRate).CalculateFitness(goal);
                    female = female.Mutate(mutationRate).CalculateFitness(goal);

                    return new[] { male, female };
                }).ToList();

                var possibleSolution =
                    nextGeneration.FirstOrDefault(
                        offSpring => Math.Abs(offSpring.Fitness - float.MaxValue) < float.Epsilon);

                currentGeneration =
                    new Population(nextGeneration, random, possibleSolution, generations);
            }
            return currentGeneration;
        }
    }
}
