using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GameOfLife.Domain;

namespace GameOfLife.Simulators
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

        protected IList<Stopwatch> _calculationTimers = new List<Stopwatch>();

        public long TimeTake {
            get { return _totalTime.ElapsedMilliseconds; }
        }

        public long TimeTakeForCalculations
        {
            get { return _calculationTimers.Sum(a => a.ElapsedMilliseconds); }
        }
    

        public string TimeTakenMessage
        {
            get
            {
                {
                    return String.Format("{0:00} ms total {1:00} ms calculations  ", _totalTime.ElapsedMilliseconds, TimeTakeForCalculations);
                }
            }
        }

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

        public virtual void Run()
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
                stopwatch.Stop();
                _calculationTimers.Add(stopwatch);
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

        protected abstract void SpawnRound(int roundToCreate);
    }
}
