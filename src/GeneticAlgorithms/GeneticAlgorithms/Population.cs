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

        public int Count()
        {
            return chromos.Count();
        }
        private Population(IEnumerable<Chromosome> chromos, Random r)
        {
            this.chromos = chromos.ToArray();
            this.random = r;
        }
        public static Population NewPopulation(int size)
        {
            Random randomness = new Random();
           
            return new Population(
                Enumerable.Range(0, size)
                    .Select(id => Chromosome.New(randomness)), randomness);
        }

        public Population Add(IEnumerable<Chromosome> newchromos)
        {
            return new Population(chromos.Concat(newchromos), random);
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
                if (fitnessSoFar > totalFitness)
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

        public static Chromosome BreedNewGenerationUntilSolutionFound(Population p,
            float goal, 
            double crossOverRate,
            double mutationRate)
        {
            int generations = 0;
            int maxgens = 400;
            while (generations < maxgens)
            {
                generations++;
                Chromosome solution = null;

                p = p.Add(Enumerable.Range(0, p.Count())
                .Select(_ =>
                    {
                        var male = p.SelectChromosome(goal);
                        var female = p.SelectChromosome(goal);
                        var offSpring = 
                            male
                            .MateWith(female, .7, .001)
                            .CalculateFitness(goal);

                        if (offSpring.Fitness == 1)                        
                            solution = offSpring;
                        
                        return offSpring;
                    }));
                if (solution != null)
                    return solution;                
            }

            throw new Exception("solution not found in 400 gens");

            
        }
    }
}
