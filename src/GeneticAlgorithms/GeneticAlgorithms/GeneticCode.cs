using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class GeneticCode : IGeneticCode
    {
        readonly Random random;

        public GeneticCode(Random random)
        {
            this.random = random;
        }
        public string Generate(int length)
        {
            return new string(
             Enumerable.Range(0, length)
                 .Select(_ => random.NextDouble() > .5 ? '1' : '0')
                 .ToArray());
        }

        public string Decode(string bits)
        {
            return DecodeImpl("", bits, true, GenoTypes());
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

        private static Dictionary<string, string> GenoTypes()
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
