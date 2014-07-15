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
        private readonly float target;

        public readonly Func<string, string> Decode = s => "";

        public int Count()
        {
            return chromos.Count();
        }

        public int Generation()
        {
            return generation;
        }

        private Population(float target, IEnumerable<Chromosome> chromos, Random random, Func<string, string> decoder)
        {
            this.target = target;
            this.chromos = chromos.ToList();
            this.random = random;
            this.Decode = decoder;            
        }

        private Population(float target, IEnumerable<Chromosome> chromos, Random random, Chromosome solution, int generation)
        {
            this.target = target;
            this.chromos = chromos.ToList();
            this.solution = solution;
            this.generation = generation;
            this.random = random;
        }

        public static Population NewPopulation(float target, int size, Random randomness)
        {
            var decoder = new GeneticCode(randomness);
            return new Population(target,
                Enumerable.Range(0, size)
                    .Select(id => 
                        Chromosome.New(target,
                            decoder)), 
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

        public Tuple<Chromosome,Chromosome> SelectPairs()
        {
            IList<Chromosome> candidates = chromos.ToList();
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
                    candidates.Remove(item);
                    return item;
                }

                var any = candidates.Last();
                candidates.Remove(any);
                return any;
            }).First();      
        }

        public Population FindSolution(double crossOverRate, double mutationRate, int maxgenerations)
        {
            int genCount = 0;
            Population currentGeneration = this;
            while( genCount < maxgenerations)               
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
                        offSpring => Math.Abs(offSpring.Fitness - float.MaxValue) < float.Epsilon);

                currentGeneration =
                    new Population(target, nextGeneration, random, possibleSolution, genCount);

                if (possibleSolution != null)
                {
                    //got the loot lets get out
                    return currentGeneration;
                }
            }

            return currentGeneration;
        }
    }
}
