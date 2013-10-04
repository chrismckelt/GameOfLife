using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLife.Domain;

namespace GameOfLife.Simulators
{
    public class IndexedSimulator : SimulatorBase
    {
        private bool[,] _previousGeneration;
        private bool[,] _nextGeneration;

        private int _xSize = 0;
        private int _ySize = 0;

        public IndexedSimulator(int generations, IEnumerable<Cell> inputCells) : base(generations, inputCells)
        {
            _previousGeneration = new bool[inputCells.Count(a => a.X == 1), inputCells.Max(a => a.X)];
            _xSize = _previousGeneration.GetLength(1);
            _ySize = _previousGeneration.GetLength(0);

            for (int a = 0; a < _xSize; a++)
            {
                for (int b = 0; b < _ySize; b++)
                {
                    var cell = inputCells.Single(x => x.X == a+1 && x.Y == b+1); // cell list is not zero based
                    _previousGeneration[b, a] = cell.Health == Health.Alive ? true : false;
                }
            }

            Cells = new Dictionary<int, IEnumerable<Cell>>();
            Cells.Add(0, inputCells);
        }

        public override void Run()
        {
            _totalTime = new Stopwatch();
            _totalTime.Start();
            SendMessage("Total Rounds: " + Rounds);

            for (int i = 0; i < Rounds; i++)
            {
                if (AllDead)
                    break;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                SpawnRound(i + 1);
                CollectCells(i+1);
                _previousGeneration = _nextGeneration;
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

        protected override void SpawnRound(int roundToCreate)
        {
            _nextGeneration = new bool[_ySize,_xSize];
            for (int a = 0; a < _xSize; a++)
            {
                for (int b = 0; b < _ySize; b++)
                {
                    var alive = GetNeighbours(a,b);

                    if (_previousGeneration[b,a]) 
                    {
                        //alive
                        if (alive < 2)
                        {
                            //Any live cell with fewer than two live neighbours dies, as if caused by under-population.
                            _nextGeneration[b,a] = false;
                        }
                        else if (alive == 2 || alive == 3)
                        {
                            //Any live cell with two or three live neighbours lives on to the next round.
                            _nextGeneration[b,a] = true;
                        }
                        else if (alive > 3)
                        {
                            //Any live cell with more than three live neighbours dies, as if by overcrowding.
                            _nextGeneration[b,a] = false;
                        }
                    }
                    else
                    {
                        //dead
                        if (alive == 3)
                        {
                            _nextGeneration[b,a] = true;
                        }
                        else
                        {
                            _nextGeneration[b,a] = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// x = width
        /// y = height
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int GetNeighbours(int x, int y)
        {
            int alive = 0;

            // top left
            if (x-1>=0 && y-1>=0 &&_previousGeneration[y-1, x-1])
                alive ++;

            // top
            if (y - 1 >= 0 && _previousGeneration[y -1,x])
                alive++;

            //top right
            if (x+1 <_xSize && y - 1 >= 0 && _previousGeneration[y - 1, x+1])
                alive++;

            // left
              if (x-1>=0 &&_previousGeneration[y, x-1])
                alive ++;

            // right
              if (x+1<_xSize &&_previousGeneration[y,x+1])
                alive ++;

            //bottom left
             if (x-1>=0 && y+1<_ySize &&_previousGeneration[y+1,x-1])
                alive ++;

            //bottom
             if (y+1 < _ySize && _previousGeneration[y + 1,x])
                 alive++;

            //bottom right
             if (x + 1 < _xSize && y + 1 < _ySize && _previousGeneration[y + 1, x + 1])
                alive ++;
          
            return alive;
        }

        private void CollectCells(int round)
        {
            var cells = new Cell[_xSize * _ySize];
            int counter = 0;
            for (int a = 0; a < _ySize; a++)
            {
                for (int b = 0; b < _xSize; b++)
                {
                    cells[counter] = new Cell(b+1,a+1,_nextGeneration[a,b] ? Health.Alive : Health.Dead); // bump up from zero index
                    counter++;
                }
            }

            Cells.Add(round, cells);
        }
    }
}
