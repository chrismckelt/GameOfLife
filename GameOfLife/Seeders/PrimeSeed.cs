using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Seeders
{
    public class PrimeSeed : ISeedLife
    {
        private readonly int _height;
        private readonly int _width;

        public PrimeSeed(int height, int width)
        {
            _height = height;
            _width = width;
        }

        public string GetSeed()
        {
            var sb = new StringBuilder();

            for (int a = 0; a < _height; a++)
            {
                for (int b = 0; b < _width; b++)
                {
                    if (PrimeSeed.IsPrime(a))
                    {
                        sb.Append("1");
                        continue;
                    }

                    if (PrimeSeed.IsPrime(b))
                    {
                        sb.Append("1");
                        continue;
                    }

                    sb.Append("0");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static bool IsPrime(int number)
        {
            double boundary = Math.Floor(Math.Sqrt(number));

            if (number == 1) return false;
            if (number == 2) return true;

            for (int i = 2; i <= boundary; ++i)
            {
                if (number % i == 0) return false;
            }

            return true;
        }
    }
}
