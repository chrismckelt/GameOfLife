using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class LargeSimulator : SimulatorBase
    {
        private ConcurrentBag<Task> _tasks;

        public LargeSimulator(int generations, IEnumerable<Cell> inputCells)
            : base(generations, inputCells)
        {
            Cells = new ConcurrentDictionary<int, IEnumerable<Cell>>(); 
            Cells.Add(0, inputCells);
            _tasks = new ConcurrentBag<Task>();
        }

        public override void Run()
        {
            _totalTime = new Stopwatch();
            _totalTime.Start();

            for (int i = 0; i < Rounds; i++)
            {
                if (AllDead)
                    break;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                SpawnRound(i + 1);
                stopwatch.Stop();

                if (!NotifyOnceEachResultSetComplete)
                {
                    string ts = string.Format("{0:00} ms", stopwatch.ElapsedMilliseconds);
                    string msg = string.Format("Created: {0} in {1} {2}", (i + 1), ts, DateTime.Now.ToLongTimeString());
                    SendMessage(msg);
                }
            }
            _totalTime.Stop();
            Completed = true;

            if (!NotifyOnceEachResultSetComplete)
            {
                foreach (var item in Cells.Values)
                {
                    if (!NotifyOnceEachResultSetComplete)
                        SendResult(item);
                }
            }
        }

        /// <summary>
        ///     Any live cell with fewer than two live neighbours dies, as if caused by under-population.
        ///     Any live cell with two or three live neighbours lives on to the next round.
        ///     Any live cell with more than three live neighbours dies, as if by overcrowding.
        ///     Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
        /// </summary>
        private void SpawnRound(int roundToCreate)
        {
            int previousRound = roundToCreate - 1;
            var spawnedCells = new ConcurrentBag<Cell>();

            Parallel.ForEach(Cells[previousRound], cell =>
            {
                var local = cell;
                var task = Task.Run(() =>
                {
                    var result = BirthCell(previousRound, local);
                    // _tasks.Add(result);
                    spawnedCells.Add(result);
                });

                _tasks.Add(task);
            });

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

            var list = new List<Cell>()
                {
                     topLeft,
                     top,
                     topRight,
                     left,
                     right,
                     bottomLeft,
                     bottom,
                     bottomRight,
                };

          //  Task.WaitAll(tasks.ToArray()); //http://stackoverflow.com/questions/13432017/await-task-whenall-inside-task-not-awaiting
            return list;
        }
    }
}
