using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GameOfLife.Domain;
using GameOfLife.Extensions;

namespace GameOfLife.Simulators
{
    public class Simulator : SimulatorBase
    {
        public Simulator(int generations, IEnumerable<Cell> inputCells) : base(generations, inputCells)
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
            var spawnedCells = new List<Cell>();

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
                        spawnedCells.Add(new Cell(cell.X, cell.Y, Health.Dead));
                    }
                    else if (alive == 2 || alive == 3)
                    {
                        //Any live cell with two or three live neighbours lives on to the next round.
                        spawnedCells.Add(new Cell(cell.X, cell.Y, cell.Health));
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

                SendMessage("Died in round " + roundToCreate);
            }

            if (NotifyOnceEachResultSetComplete)
                SendResult(spawnedCells);
        }

        protected IEnumerable<Cell> GetNeighbours(int round, Cell cell)
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
