using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Seeders
{
    /// <summary>
    /// http://www.conwaylife.com/wiki/index.php?title=Gosper_glider_gun
    /// https://www.codehosting.net/blog/files/gameoflife.txt
    /// </summary>
    public class GliderGunSeed : ISeedLife
    {
        public string GetSeed()
        {
            return 
@"000000000000000000000000100000000000
000000000000000000000010100000000000
000000000000110000001100000000000011
000000000001000100001100000000000011
110000000010000010001100000000000000
110000000010001011000010100000000000
000000000010000010000000100000000000
000000000001000100000000000000000000
000000000000110000000000000000000000";
        }
    }
}
