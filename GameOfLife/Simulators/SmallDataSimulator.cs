using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLife.Domain;
using GameOfLife.Extensions;

namespace GameOfLife.Simulators
{
    public abstract class SmallDataSimulator : SimulatorBase
    {
        protected abstract ICollection<Cell> GetCellEnumerableDataHolder();

        private ICollection<Cell> _spawnedCells;

        protected SmallDataSimulator(int generations, ICollection<Cell> inputCells)
            : base(generations, inputCells)
        {
            Cells = new Dictionary<int, IEnumerable<Cell>>(); 
            Cells.Add(0, inputCells);
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
            _spawnedCells = GetCellEnumerableDataHolder();

            foreach (Cell cell in Cells[previousRound]) 
            {
                IEnumerable<Cell> neighbours = GetNeighbours(previousRound, cell); 
                int alive = neighbours.Count(a => a.Health == Health.Alive);
                //  Console.WriteLine("Alive Count:{0}", alive);

                if (cell.Health == Health.Alive)
                {
                    if (alive < 2)
                    {
                        //Any live cell with fewer than two live neighbours dies, as if caused by under-population.
                        _spawnedCells.Add(new Cell(cell.X, cell.Y, Health.Dead));
                    }
                    else if (alive == 2 || alive == 3)
                    {
                        //Any live cell with two or three live neighbours lives on to the next round.
                        _spawnedCells.Add(new Cell(cell.X, cell.Y, cell.Health));
                    }
                    else if (alive > 3)
                    {
                        //Any live cell with more than three live neighbours dies, as if by overcrowding.
                        _spawnedCells.Add(new Cell(cell.X, cell.Y, Health.Dead));
                    }
                }
                else
                {
                    _spawnedCells.Add(alive == 3
                                         ? new Cell(cell.X, cell.Y, Health.Alive)
                                         : new Cell(cell.X, cell.Y, Health.Dead));
                }
            }
      
            //Debug.Assert(Cells[previousRound].Count == spawnedCells.Count);
            Cells.Add(roundToCreate, _spawnedCells);

            if (_spawnedCells.All(a => a.Health != Health.Alive))
            {
                AllDead = true;

                SendMessage("Died in round " + roundToCreate);
            }

            if (NotifyOnceEachResultSetComplete)
                SendResult(_spawnedCells);
        }

        protected ICollection<Cell> GetNeighbours(int round, Cell cell)
        {
            var list = GetCellEnumerableDataHolder();
            list.Add(Cells[round].TopLeft(cell));
            list.Add(Cells[round].Top(cell));
            list.Add(Cells[round].TopRight(cell));
            list.Add(Cells[round].Left(cell));
            list.Add(Cells[round].Right(cell));
            list.Add(Cells[round].BottomLeft(cell));
            list.Add(Cells[round].Bottom(cell));
            list.Add(Cells[round].BottomRight(cell));

            return list;
        }

        

    }
}
