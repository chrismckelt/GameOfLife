using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        public delegate void NotifyMessage(string msg);

        public delegate void NotifyResult(IList<Cell> cells);

        public event NotifyMessage OnNotifyMessage;

        public event NotifyResult OnNotifyResult;

        private Stopwatch _stopwatch;
        

        public Simulator(int generations, IList<Cell> inputCells)
        {
            if (generations < 1) throw new ArgumentException("Rounds must be greater than zero");
            
            if (inputCells.Last().X < 1 || inputCells.Last().Y < 1) throw new ArgumentException("X and Y must be greater than zero");
            
            this.Rounds = generations;
            
            Cells = new Dictionary<int, IList<Cell>> {{0, inputCells}};
        }

        
        public void Run()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            for (int i = 0; i < Rounds; i++)
            {
                if (!AllDead)
                    SpawnRound(i+1);
            }
            _stopwatch.Stop();
            Completed = true;
        }

        public string TimeTaken
        {
            get { 
                {
      
                    return String.Format("{0:00} ms", _stopwatch.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>
        /// Any live cell with fewer than two live neighbours dies, as if caused by under-population.
        /// Any live cell with two or three live neighbours lives on to the next round.
        /// Any live cell with more than three live neighbours dies, as if by overcrowding.
        /// Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
        /// </summary>
        private void SpawnRound(int roundToCreate)
        {
            int previousRound = roundToCreate - 1;
            var spawnedCells = new List<Cell>();

            foreach (var cell in Cells[previousRound])
            {
                var neighbours = GetNeighbours(previousRound, cell);
                int alive = neighbours.Count(a=>a.Health == Health.Alive);
              //  Console.WriteLine("Alive Count:{0}", alive);

                if (cell.Health == Health.Alive)
                {
                    if (alive < 2)
                    {
                        //Any live cell with fewer than two live neighbours dies, as if caused by under-population.
                        spawnedCells.Add(new Cell(cell.X, cell.Y,Health.Dead));
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

            //Debug.Assert(Cells[previousRound].Count == spawnedCells.Count);
            Cells.Add(roundToCreate, spawnedCells);

            if (spawnedCells.All(a => a.Health != Health.Alive))
            {
                AllDead = true;

                if (OnNotifyMessage != null)
                    OnNotifyMessage("All Dead!");

            }

            if (OnNotifyResult != null)
                OnNotifyResult(spawnedCells);
        }

        private IEnumerable<Cell> GetNeighbours(int round, Cell cell)
        {
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
