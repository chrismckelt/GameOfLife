using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public static class ExtensionsAsync
    {
        public async static Task<Cell> TopLeftAsync(this IEnumerable<Cell> cells, Cell from)
        {
            return await Task.Run(()=>cells.FirstOrDefault(a => a.X == from.X - 1 && a.Y == from.Y - 1));
        }

        public async static Task<Cell> LeftAsync(this IEnumerable<Cell> cells, Cell from)
        {
             return await Task.Run(()=>cells.FirstOrDefault(a => a.X == from.X - 1 && a.Y == from.Y));
        }

        public async static Task<Cell> BottomLeftAsync(this IEnumerable<Cell> cells, Cell from)
        {
            return await Task.Run(() => cells.FirstOrDefault(a => a.X == from.X - 1 && a.Y == from.Y + 1));
        }

        public async static Task<Cell> TopRightAsync(this IEnumerable<Cell> cells, Cell from)
        {
            return await Task.Run(() => cells.FirstOrDefault(a => a.X == from.X + 1 && a.Y == from.Y - 1));
        }

        public async static Task<Cell> RightAsync(this IEnumerable<Cell> cells, Cell from)
        {
            return await Task.Run(() => cells.FirstOrDefault(a => a.X == from.X + 1 && a.Y == from.Y));
        }

        public async static Task<Cell> BottomRightAsync(this IEnumerable<Cell> cells, Cell from)
        {
            return await Task.Run(() => cells.FirstOrDefault(a => a.X == from.X + 1 && a.Y == from.Y + 1));
        }

        public async static Task<Cell> BottomAsync(this IEnumerable<Cell> cells, Cell from)
        {
            return await Task.Run(() => cells.FirstOrDefault(a => a.X == from.X && a.Y == from.Y - 1));
        }

        public async static Task<Cell> TopAsync(this IEnumerable<Cell> cells, Cell from)
        {
            return await Task.Run(() => cells.FirstOrDefault(a => a.X == from.X && a.Y == from.Y + 1));
        }
    }
}
