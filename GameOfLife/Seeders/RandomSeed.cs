using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Seeders
{
    public class RandomSeed : ISeedLife
    {
        private readonly int _height;
        private readonly int _width;

        public RandomSeed(int height, int width)
        {
            _height = height;
            _width = width;
        }

        public string GetSeed()
        {
            var sb = new StringBuilder();
            var random = new Random();

            for (int a = 0; a < _height; a++)
            {
                for (int b = 0; b < _width; b++)
                {
                    sb.Append(random.Next(1, 3) % 2 == 0 ? "1" : "0");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

    }
}
