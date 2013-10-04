using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Seeders
{
    public class Sample2Seed : ISeedLife
    {
        public string GetSeed()
        {
            return @"
0000000000
0000000000
0000000000
0000000000
0000000000
0000010000
0000010000
0000010000
0000000000
0000000000
";
        }
    }
}
