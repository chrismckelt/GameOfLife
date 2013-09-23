using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    /// <summary>
    /// FirstOrDefault is almost twice as fast as SingleOrDefault
    /// </summary>
    public static class Extensions
    {
        public static Cell TopLeft(this IEnumerable<Cell> cells, Cell from)
        {
            return cells.FirstOrDefault(a => a.X == from.X - 1 && a.Y == from.Y-1);
        }

        public static Cell Left(this IEnumerable<Cell> cells, Cell from)
        {
            return cells.FirstOrDefault(a => a.X == from.X - 1 && a.Y == from.Y);
        }

        public static Cell BottomLeft(this IEnumerable<Cell> cells, Cell from)
        {
            return cells.FirstOrDefault(a => a.X == from.X - 1 && a.Y == from.Y+1);
        }

        public static Cell TopRight(this IEnumerable<Cell> cells, Cell from)
        {
            return cells.FirstOrDefault(a => a.X == from.X + 1 && a.Y == from.Y - 1);
        }

        public static Cell Right(this IEnumerable<Cell> cells, Cell from)
        {
            return cells.FirstOrDefault(a => a.X == from.X + 1 && a.Y == from.Y);
        }

        public static Cell BottomRight(this IEnumerable<Cell> cells, Cell from)
        {
            return cells.FirstOrDefault(a => a.X == from.X + 1 && a.Y == from.Y + 1);
        }

        public static Cell Bottom(this IEnumerable<Cell> cells, Cell from)
        {
            return cells.FirstOrDefault(a => a.X == from.X && a.Y == from.Y - 1);
        }

        public static Cell Top(this IEnumerable<Cell> cells, Cell from)
        {
            return cells.FirstOrDefault(a => a.X == from.X && a.Y == from.Y + 1);
        }
    }
}
