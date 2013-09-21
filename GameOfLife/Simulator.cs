using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLife
{
    public enum Health
    {
        Dead = 0,
        Alive = 1
    }

    public class Simulator
    {
        public IDictionary<int, IList<Cell>> Cells;
        public int Rounds { get; private set; }

        public bool AllDead { get; private set; }

        public bool Completed { get; private set; }

        public bool NotifyOnceEachResultSetComplete { get; set; }
        // toggle this to output all results at the end - or notify upon completion or each result set

        public delegate void NotifyMessage(string msg);

        public delegate void NotifyResult(IList<Cell> cells);

        public event NotifyMessage OnNotifyMessage;

        public event NotifyResult OnNotifyResult;

        private Stopwatch _totalTime;


        public Simulator(int generations, IList<Cell> inputCells)
        {
            if (generations < 1) throw new ArgumentException("Rounds must be greater than zero");

            if (inputCells.Last().X < 1 || inputCells.Last().Y < 1)
                throw new ArgumentException("X and Y must be greater than zero");

            Rounds = generations;

            Cells = new Dictionary<int, IList<Cell>>(); // TODO make this thread safe when concurreny is implemented
            Cells.Add(0, inputCells);
        }


        public void Run()
        {
            _totalTime = new Stopwatch();
            _totalTime.Start();
            if (OnNotifyMessage != null)
                OnNotifyMessage("Total Rounds: " + Rounds);
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
                    if (OnNotifyMessage != null)
                    {
                        string ts = string.Format("{0:00} ms", stopwatch.ElapsedMilliseconds);
                        string msg = string.Format("Created: {0} in {1}", (i + 1), ts);
                        OnNotifyMessage(msg);
                    }
                }
            }
            _totalTime.Stop();
            Completed = true;

            if (!NotifyOnceEachResultSetComplete)
            {
                foreach (var item in Cells.Values)
                {
                    if (OnNotifyResult != null && !NotifyOnceEachResultSetComplete)
                        OnNotifyResult(item);
                }
            }
        }

        public string TimeTaken
        {
            get
            {
                {
                    return String.Format("{0:00} ms", _totalTime.ElapsedMilliseconds );
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
            var spawnedCells = new List<Cell>();

            foreach (Cell cell in Cells[previousRound]) // TODO parallelise this
            {
                IEnumerable<Cell> neighbours = GetNeighbours(previousRound, cell); //TODO async await
                int alive = neighbours.Count(a => a.Health == Health.Alive);
                //  Console.WriteLine("Alive Count:{0}", alive);

                if (cell.Health == Health.Alive)
                {
                    if (alive < 2)
                    {
                        //Any live cell with fewer than two live neighbours dies, as if caused by under-population.
                        spawnedCells.Add(new Cell(cell.X, cell.Y, Health.Dead));
                    }
                    else if (alive == 2 || alive == 3)
                    {
                        //Any live cell with two or three live neighbours lives on to the next round.
                        spawnedCells.Add(cell);
                    }
                    else if (alive > 3)
                    {
                        //Any live cell with more than three live neighbours dies, as if by overcrowding.
                        spawnedCells.Add(new Cell(cell.X, cell.Y, Health.Dead));
                    }
                }
                else
                {
                    spawnedCells.Add(alive == 3
                                         ? new Cell(cell.X, cell.Y, Health.Alive)
                                         : new Cell(cell.X, cell.Y, Health.Dead));
                }
            }
            ;

            //Debug.Assert(Cells[previousRound].Count == spawnedCells.Count);
            Cells.Add(roundToCreate, spawnedCells);

            if (spawnedCells.All(a => a.Health != Health.Alive))
            {
                AllDead = true;

                if (OnNotifyMessage != null)
                    OnNotifyMessage("Died in round " + roundToCreate);
            }

            if (OnNotifyResult != null && NotifyOnceEachResultSetComplete)
                OnNotifyResult(spawnedCells);
        }

        private IEnumerable<Cell> GetNeighbours(int round, Cell cell)
        {
            //TODO async await these calls (RX extensions?)
            var list = new List<Cell>
                {
                    Cells[round].TopLeft(cell),
                    Cells[round].Top(cell),
                    Cells[round].TopRight(cell),
                    Cells[round].Left(cell),
                    Cells[round].Right(cell),
                    Cells[round].BottomLeft(cell),
                    Cells[round].Bottom(cell),
                    Cells[round].BottomRight(cell)
                };

            return list;
        }
    }
}
