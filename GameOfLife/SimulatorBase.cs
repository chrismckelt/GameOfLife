using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public abstract class SimulatorBase
    {
        protected SimulatorBase(int generations, IEnumerable<Cell> inputCells)
        {
            if (generations < 1) throw new ArgumentException("Rounds must be greater than zero");

            if (inputCells.Last().X < 1 || inputCells.Last().Y < 1)
                throw new ArgumentException("X and Y must be greater than zero");

            Rounds = generations;
        }

        public IDictionary<int, IEnumerable<Cell>> Cells;

        public int Rounds { get; protected set; }

        public bool AllDead { get; protected set; }

        public bool Completed { get; protected set; }

        public bool NotifyOnceEachResultSetComplete { get; set; }

        public delegate void NotifyMessage(string msg);

        public delegate void NotifyResult(IEnumerable<Cell> cells);

        public event NotifyMessage OnNotifyMessage;

        public event NotifyResult OnNotifyResult;

        protected Stopwatch _totalTime;

        public string TimeTaken
        {
            get
            {
                {
                    return String.Format("{0:00} ms", _totalTime.ElapsedMilliseconds);
                }
            }
        }

        public abstract void Run();

        protected void SendMessage(string msg)
        {
            if (OnNotifyMessage != null)
                OnNotifyMessage(msg);
        }

        protected void SendResult(IEnumerable<Cell> cells)
        {
            if (OnNotifyResult != null)
                OnNotifyResult(cells);
        }
    }
}
