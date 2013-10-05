using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GameOfLife.Domain;
using GameOfLife.Extensions;

namespace GameOfLife.Simulators
{
    public class ListSimulator : SmallDataSimulator
    {
        public ListSimulator(int generations, ICollection<Cell> inputCells) : base(generations, inputCells)
        {
        }

        protected override ICollection<Cell> GetCellEnumerableDataHolder()
        {
            return new List<Cell>();
        }
    }
}
