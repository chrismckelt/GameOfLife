using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLife.Domain;

namespace GameOfLife.Simulators
{
    public class ConcurrentStackSimulator : LargeSimulator
    {
        public ConcurrentStackSimulator(int generations, ICollection<Cell> inputCells) : base(generations, inputCells)
        {
        }

        protected override IProducerConsumerCollection<Cell> GetCellEnumerableDataHolder()
        {
            return new ConcurrentStack<Cell>();
        }

        protected override IProducerConsumerCollection<Task> GetTaskEnumerableDataHolder()
        {
            return new ConcurrentStack<Task>();
        }
    }
}
