using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameOfLife.Domain;
using GameOfLife.Extensions;

namespace GameOfLife.Simulators
{
    public abstract class LargeSimulator : SimulatorBase
    {
        protected abstract IProducerConsumerCollection<Cell> GetCellEnumerableDataHolder();

        protected abstract IProducerConsumerCollection<Task> GetTaskEnumerableDataHolder();

        private IProducerConsumerCollection<Task> _tasks;

        protected LargeSimulator(int generations, ICollection<Cell> inputCells)
            : base(generations, inputCells)
        {
            Cells = new ConcurrentDictionary<int, IEnumerable<Cell>>(); 
            Cells.Add(0, inputCells);
        }

        public override void Run()
        {
            _tasks = GetTaskEnumerableDataHolder();
            base.Run();
        }
        
        /// <summary>
        ///     Any live cell with fewer than two live neighbours dies, as if caused by under-population.
        ///     Any live cell with two or three live neighbours lives on to the next round.
        ///     Any live cell with more than three live neighbours dies, as if by overcrowding.
        ///     Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
        /// </summary>
        protected override void SpawnRound(int roundToCreate)
        {
            int previousRound = roundToCreate - 1;
            var spawnedCells = GetCellEnumerableDataHolder();
            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 10 };
            ///http://stackoverflow.com/questions/5009181/parallel-foreach-vs-task-factory-startnew
            var task = Task.Run(() =>
                {
                    Parallel.ForEach(Cells[previousRound], options, cell =>
                    {
                        var local = cell;
                        var result = BirthCell(previousRound, local);
                        bool added = spawnedCells.TryAdd(result);
                        if (!added) throw new ThreadStateException("Cell not added");
                    });
                    }
                );
           
            _tasks.TryAdd(task);
            Task.WaitAll(_tasks.ToArray());
            //Debug.Assert(Cells[previousRound].Count == spawnedCells.Count);
            Cells.Add(roundToCreate, spawnedCells);

            if (spawnedCells.All(a => a.Health != Health.Alive))
            {
                AllDead = true;
                SendMessage("Died in round " + roundToCreate);
            }

            if (NotifyOnceEachResultSetComplete)
                SendResult(spawnedCells.ToList());
        }

        private Cell BirthCell(int previousRound, Cell local)
        {
            var neighbours = GetNeighbours(previousRound, local);
            int alive = neighbours.Count(a => a.Health == Health.Alive);
            //  Console.WriteLine("Alive Count:{0}", alive);

            if (local.Health == Health.Alive)
            {
                //SendMessage(string.Format("Alive X:{0} Y:{1}", local.X, local.Y));

                if (alive < 2)
                {
                    //Any live cell with fewer than two live neighbours dies, as if caused by under-population.
                    return new Cell(local.X, local.Y, Health.Dead);
                }
                else if (alive == 2 || alive == 3)
                {
                    //Any live cell with two or three live neighbours lives on to the next round.
                    return new Cell(local.X, local.Y, local.Health);
                }
                else if (alive > 3)
                {
                    //Any live cell with more than three live neighbours dies, as if by overcrowding.
                     return new Cell(local.X, local.Y, Health.Dead);
                }
            }
            else
            {
               // SendMessage(string.Format("Dead X:{0} Y:{1}", local.X, local.Y));
                if (alive == 3)
                {
                    return new Cell(local.X, local.Y, Health.Alive);
                }
                else
                {
                    return new Cell(local.X, local.Y, Health.Dead);
                }
            }
            throw new ApplicationException("Nothing configured for this cell type?");
        }

        private IEnumerable<Cell> GetNeighbours(int round, Cell cell)
        {
            var topLeft = Cells[round].TopLeft(cell);
            var top = Cells[round].Top(cell);
            var topRight = Cells[round].TopRight(cell);
            var left = Cells[round].Left(cell);
            var right = Cells[round].Right(cell);
            var bottomLeft= Cells[round].BottomLeft(cell);
            var bottom = Cells[round].Bottom(cell);
            var bottomRight = Cells[round].BottomRight(cell);

            //var tasks = new List<Task>(){topLeft, top, topRight, left, right,bottomLeft, bottom, bottomRight};
            var list = GetCellEnumerableDataHolder();
            list.TryAdd(topLeft);
            list.TryAdd(top);
            list.TryAdd(topRight);
            list.TryAdd(left);
            list.TryAdd(right);
            list.TryAdd(bottomLeft);
            list.TryAdd(bottom);
            list.TryAdd(bottomRight);

          //  Task.WaitAll(tasks.ToArray()); //http://stackoverflow.com/questions/13432017/await-task-whenall-inside-task-not-awaiting
            return list;
        }

    }
}
