using System;

namespace GameOfLife.Domain
{
    public struct Cell : IEquatable<Cell>
    {
        public Cell(int x, int y, Health health)
        {
            _x = x;
            _y = y;
            _health = health;
        }

        private readonly int _x;
        private readonly int _y;
        private readonly Health _health;

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public Health Health
        {
            get { return _health; }
        }

        public bool Equals(Cell other)
        {
            return (X == other.X && Y == other.Y && _health == other.Health);
        }
    }
}
