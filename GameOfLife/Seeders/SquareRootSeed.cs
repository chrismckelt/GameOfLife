using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Seeders
{
    public class SquareRootSeed : ISeedLife
    {
        private readonly int _squared;

        public SquareRootSeed(int squared)
        {
            _squared = squared;
        }

        public string GetSeed()
        {
            var sb = new StringBuilder();
            var random = new Random();
            double sr = Math.Sqrt(_squared);
            for (int a = 0; a < sr; a++)
            {
                for (int b = 0; b < sr; b++)
                {
                    sb.Append(random.Next(1, 3) % 2 == 0 ? "1" : "0");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
