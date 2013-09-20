using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public static class Extensions
    {
        public static Cell TopLeft(this IList<Cell> cells, Cell from)
        {
            return cells.SingleOrDefault(a => a.X == from.X - 1 && a.Y == from.Y-1);
        }

        public static Cell Left(this IList<Cell> cells, Cell from)
        {
            return cells.SingleOrDefault(a => a.X == from.X - 1 && a.Y == from.Y);
        }

        public static Cell BottomLeft(this IList<Cell> cells, Cell from)
        {
            return cells.SingleOrDefault(a => a.X == from.X - 1 && a.Y == from.Y+1);
        }

        public static Cell TopRight(this IList<Cell> cells, Cell from)
        {
            return cells.SingleOrDefault(a => a.X == from.X + 1 && a.Y == from.Y - 1);
        }

        public static Cell Right(this IList<Cell> cells, Cell from)
        {
            return cells.SingleOrDefault(a => a.X == from.X + 1 && a.Y == from.Y);
        }

        public static Cell BottomRight(this IList<Cell> cells, Cell from)
        {
            return cells.SingleOrDefault(a => a.X == from.X + 1 && a.Y == from.Y + 1);
        }

        public static Cell Bottom(this IList<Cell> cells, Cell from)
        {
            return cells.SingleOrDefault(a => a.X == from.X && a.Y == from.Y - 1);
        }

        public static Cell Top(this IList<Cell> cells, Cell from)
        {
            return cells.SingleOrDefault(a => a.X == from.X && a.Y == from.Y + 1);
        }
    }
}
