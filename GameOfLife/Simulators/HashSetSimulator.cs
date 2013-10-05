using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using GameOfLife.Domain;
using GameOfLife.Extensions;

namespace GameOfLife.Simulators
{
    public class HashSetSimulator : SmallDataSimulator
    {
        public HashSetSimulator(int generations, ICollection<Cell> inputCells) : base(generations, inputCells)
        {
        }


        protected override ICollection<Cell> GetCellEnumerableDataHolder()
        {
            return new HashSet<Cell>();
        }
    }
}
