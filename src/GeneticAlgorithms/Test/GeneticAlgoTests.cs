using System.Diagnostics;
using System.Threading;
using GeneticAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{

    public class GeneticAlgoTests
    {
        [Fact]
        public void Asking_for_a_population_of_N_creates_N_chromosomes()
        {
            var N = 100;
            var sut = Population.NewPopulation(N);
            Assert.Equal(N, sut.All().Count());
        }

        [Fact]
        public void Can_handle_addition()
        {
            var sut = Chromosome.New(new Random());
            sut.Decode = bits => "7+4";
            Assert.Equal(7 + 4, sut.Answer());            
        }

        [Fact]
        public void Can_handle_subtraction()
        {            
            var sut = Chromosome.New(new Random());
            sut.Decode = bits => "7-4";
            Assert.Equal(7 - 4, sut.Answer());
        }

        [Fact]
        public void Can_handle_multiplication()
        {            
            var sut = Chromosome.New(new Random());
            sut.Decode = bits => "7*4";
            Assert.Equal(7 * 4, sut.Answer());
        }

        [Fact]
        public void Can_handle_division()
        {            
            var sut = Chromosome.New(new Random());
            sut.Decode = bits => "7/4";
            Assert.Equal(7 / 4, sut.Answer());
        }

        [Fact]
        public void Can_handle_multiple_operations()
        {           
            var sut = Chromosome.New(new Random());
            sut.Decode = bits => "7/7*4+4-7";
            Assert.Equal(7 / 7 * 4 + 4 - 7, sut.Answer());
        }

        [Fact]
        public void Decoding_string_should_ignore_prefixed_operators()
        {
            Func<string, string> g = s => Chromosome.GenoTypes.Single(x => x.Value.Equals(s)).Key;
            Func<string> fakeCodeGen = () => string.Format("{0}{1}{2}{3}", g("/"), g("7"), g("+"), g("4"));

            var sut = Chromosome.New(fakeCodeGen);           

            Assert.Equal("7+4", sut.DecodedString);
        }

        [Fact]
        public void Decoding_string_should_ignore_repeated_operators()
        {
            Func<string, string> g = s => Chromosome.GenoTypes.Single(x => x.Value.Equals(s)).Key;
            Func<string> fakeCodeGen = () => string.Format("{0}{1}{2}{3}",  g("7"), g("+"), g("+"), g("4"));

            var sut = Chromosome.New(fakeCodeGen);

            Assert.Equal("7+4", sut.DecodedString);
        }

        [Fact]
        public void Decoding_string_should_ignore_repeated_operands()
        {
            Func<string, string> g = s => Chromosome.GenoTypes.Single(x => x.Value.Equals(s)).Key;
            Func<string> fakeCodeGen = () => string.Format("{0}{1}{2}{3}", g("7"), g("+"), g("4"), g("4"));

            var sut = Chromosome.New(fakeCodeGen);

            Assert.Equal("7+4", sut.DecodedString);
        }


        [Fact]
        public void CrossOver_should_swap_at_the_position_passed_in()
        {
            var sut = Chromosome.New(() => "11111111");
            var other = Chromosome.New(() => "00000000");

            var result = sut.Crossover(other, .5);

            Assert.Equal(result.Bits, "11110000");
        }

        [Fact]
        public void Mutate_should_mutate_all_when_rate_is_1()
        {
            var sut = Chromosome.New(() => "11111111");           

            var result = sut.Mutate(1);

            Assert.Equal(result.Bits, "00000000");
        }

        [Fact]
        public void Mutate_should_mutate_none_all_when_rate_is_0()
        {
            var sut = Chromosome.New(() => "11111111");

            var result = sut.Mutate(0);

            Assert.Equal(result.Bits, "11111111");
        }

        [Fact]
        public void Mutate_should_mutate_some_when_rate_is_50_percent()
        {
            var sut = Chromosome.New(() => "11111111");

            var result = sut.Mutate(.5);

            Assert.NotEqual(sut.Bits, result.Bits);
        }

        [Fact]
        public void Find_the_first_chromoSome_that_solves_the_puzzle()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            float goal = 22F;
            var population = Population.NewPopulation(300);


            var finalGeneration = population.FindSolution(goal, .7, .05);
            var solution = finalGeneration.Solution();

            watch.Stop();

            if (solution != null)
            {
                Console.WriteLine("Ms to solution: {0}", watch.ElapsedMilliseconds);
                Console.WriteLine("Number of generations: {0}", finalGeneration.Generation());
                Console.WriteLine("Fitness: {0}, Solution: {1}, Answer: {2}", solution.Fitness, solution.DecodedString,
                    solution.Answer());
                Console.WriteLine("Bits: {0}", solution.Bits);

                Assert.Equal(solution.Answer(), goal);
            }

            foreach (var c in finalGeneration.All())
            {
                Console.WriteLine("Current generations solutions");
                Console.WriteLine("Fitness: {0}, Solution: {1}, Answer: {2}", c.Fitness, c.DecodedString, c.Answer());
            }

            Assert.NotNull(solution);
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
