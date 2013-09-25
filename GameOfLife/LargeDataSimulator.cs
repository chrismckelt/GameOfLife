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
        private IList<Task> _tasks;

        public LargeSimulator(int generations, IEnumerable<Cell> inputCells)
            : base(generations, inputCells)
        {
            Cells = new ConcurrentDictionary<int, IEnumerable<Cell>>(); 
            Cells.Add(0, inputCells);
            _tasks = new List<Task>();
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
                    string msg = string.Format("Created: {0} in {1}", (i + 1), ts);
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

            foreach (var cell in Cells[previousRound])
            {
                var local = cell;
                var task = Task.Run(() =>
                    {
                        var result = BirthCell(previousRound, local);
                        _tasks.Add(result);
                        spawnedCells.Add(result.Result);
                    });

                _tasks.Add(task);
            }

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

        private async Task<Cell> BirthCell(int previousRound, Cell local)
        {
            var neighbours = await GetNeighbours(previousRound, local);
            int alive = neighbours.Count(a => a.Health == Health.Alive);
            //  Console.WriteLine("Alive Count:{0}", alive);

            if (local.Health == Health.Alive)
            {
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

        private async Task<List<Cell>> GetNeighbours(int round, Cell cell)
        {
            var topLeft = Cells[round].TopLeftAsync(cell);
            var top = Cells[round].TopAsync(cell);
            var topRight = Cells[round].TopRightAsync(cell);
            var left = Cells[round].LeftAsync(cell);
            var right = Cells[round].RightAsync(cell);
            var bottomLeft= Cells[round].BottomLeftAsync(cell);
            var bottom = Cells[round].BottomAsync(cell);
            var bottomRight = Cells[round].BottomRightAsync(cell);

            var tasks = new List<Task>(){topLeft, top, topRight, left, right,bottomLeft, bottom, bottomRight};

            var list = new List<Cell>()
                {
                    await topLeft,
                    await top,
                    await topRight,
                    await left,
                    await right,
                    await bottomLeft,
                    await bottom,
                    await bottomRight,
                };

            Task.WaitAll(tasks.ToArray()); //http://stackoverflow.com/questions/13432017/await-task-whenall-inside-task-not-awaiting
            return list;
        }
    }
}
