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
        private readonly IEnumerable<Chromosome> chromos;
        
        private readonly Chromosome solution;
        private readonly int generation;

        public int Count()
        {
            return chromos.Count();
        }

        public int Generation()
        {
            return generation;
        }

        private Population(IEnumerable<Chromosome> chromos, Random r)
        {
            this.chromos = chromos.ToArray();
            this.random = r;
        }

        private Population(IEnumerable<Chromosome> chromos, Random r, Chromosome solution, int gen)
        {
            this.chromos = chromos.ToArray();
            this.solution = solution;
            this.generation = gen;
            this.random = r;
        }

        public static Population NewPopulation(int size)
        {
            Random randomness = new Random();
           
            return new Population(
                Enumerable.Range(0, size)
                    .Select(id => Chromosome.New(randomness)), randomness);
        }

        public Chromosome Solution()
        {
            return solution;
        }

        public Chromosome BestAnswer()
        {
            return chromos.Aggregate((m, f) => m.Fitness >= f.Fitness ? m : f);
        }

        public Chromosome SelectChromosome(float goal)
        {
            var candidates = chromos.Select(x => x.CalculateFitness(goal));

            float totalFitness = candidates.Sum(x => x.Fitness);
            float slice = (float)(totalFitness * random.NextDouble());

            float fitnessSoFar = 0.0F;
            foreach (var item in candidates)
            {
                fitnessSoFar += item.Fitness;
                if (fitnessSoFar > slice)
                {
                    return item;
                }                
            }

            return chromos.Last();
        }

        public Population TestPopulation(float goal)
        {
            return new Population(
                Enumerable.Range(0, chromos.Count())
                    .Select(i =>
                    {
                        return chromos.ElementAt(i).CalculateFitness(goal);
                    }), random);
        }        

        public IEnumerable<Chromosome> All() { return chromos; }

        public Population FindSolution(float goal, double crossOverRate, double mutationRate)
        {
            int generations = 0;
            int maxgens = 50;

            Population currentGeneration = this;
            while (generations < maxgens)
            {
                generations++;

                var nextGeneration = 
                Enumerable.Range(1, Count())
                .Select(_ =>
                {
                    var male = currentGeneration.SelectChromosome(goal);
                    var female = currentGeneration.SelectChromosome(goal);
                    return 
                        male
                        .MateWith(female, crossOverRate, mutationRate)
                        .CalculateFitness(goal);
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
