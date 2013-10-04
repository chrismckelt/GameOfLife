using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Seeders
{
    public class Sample1Seed : ISeedLife
    {
        public string GetSeed()
        {
            return @"
10100
00100
10100
";
        }
    }
}
