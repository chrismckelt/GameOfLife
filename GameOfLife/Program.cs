using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameOfLife.Domain;
using GameOfLife.Seeders;
using GameOfLife.Simulators;

namespace GameOfLife
{
   /// <summary>
    /// http://en.wikipedia.org/wiki/Conway%27s_Game_of_Life
   /// </summary>
    class Program
   {
       private const string TITLE = "Conway's Game of Life";
       private const int ROUNDS = 30;
       private static bool _runContinousSimulationAtEnd = false;
       private static bool _verifySample1 = false;
       private static SimulatorBase _simulator;
       private static StringBuilder _log;
       private static string _logFile;
       private static decimal _sampleSize;
       private static string _logDir;
       private static List<Result> _results;
       private static bool _deleteLogFileOnStart;


       static void Main(string[] args)
       {
           string simulator = string.Empty;
           if (args != null && args.Any())
           {
               simulator = args[0];
               Console.WriteLine("Using: {0}",   args[0]);
           }

           Setup();
           ShowHeader();
           ShowHelp();
           string sample = FindInputSeed().GetSeed();

           var inputCells = ParseInput(sample);

           var items = inputCells as Cell[] ?? inputCells.ToArray();
           _sampleSize = items.Count();
           _results = new List<Result>();

           int roundsStart = 1;
           int roundsEnd = 1000;
           int increment = 100;

           if (!string.IsNullOrEmpty(simulator) && simulator == "all")
           {
               BenchmarkSimulator(typeof (IndexedSimulator).Name, items, roundsStart, roundsEnd, increment);
               VerifySample1();
               _results.Add(new Result(typeof(IndexedSimulator).Name, roundsStart, _simulator.TimeTake, _simulator.TimeTakeForCalculations));
               BenchmarkSimulator(typeof(ListSimulator).Name, items, roundsStart, roundsEnd, increment);
               VerifySample1();
               _results.Add(new Result(typeof(ListSimulator).Name, roundsStart, _simulator.TimeTake, _simulator.TimeTakeForCalculations));
               BenchmarkSimulator(typeof(HashSetSimulator).Name, items, roundsStart, roundsEnd, increment);
               VerifySample1();
               _results.Add(new Result(typeof(HashSetSimulator).Name, roundsStart, _simulator.TimeTake, _simulator.TimeTakeForCalculations));
               BenchmarkSimulator(typeof(LargeSimulator).Name, items, roundsStart, roundsEnd, increment);
               VerifySample1();
               _results.Add(new Result(typeof(LargeSimulator).Name, roundsStart, _simulator.TimeTake, _simulator.TimeTakeForCalculations));

               Console.ForegroundColor = ConsoleColor.DarkMagenta;
               WriteLog("Winner = " + _results.OrderBy(a => a.CalculationTime).First().SimulatorName);
               Console.ForegroundColor = ConsoleColor.Gray;
               foreach (var result in _results.OrderBy(a=>a.CalculationTime))
               {
                   WriteLog(result.ToString());
               }

           }
           else if (!string.IsNullOrWhiteSpace(simulator))
           {
               BenchmarkSimulator(simulator, items, ROUNDS, ROUNDS,ROUNDS);
               WriteLog(string.Format("Time taken: {0}", _simulator.TimeTakenMessage));
           }
           else
           {
               if (_sampleSize < (1024))// optimise for L1 cache size (sysinternals or http://chocolatey.org/packages/cpu-z)
               {
                   simulator = typeof (IndexedSimulator).Name;
               }
               else if (_sampleSize < 4096)
               {
                   simulator = typeof (ListSimulator).Name;
               }
               else
               {
                   simulator = typeof(LargeSimulator).Name;
               }
               BenchmarkSimulator(simulator, items,ROUNDS, ROUNDS, ROUNDS);
               WriteLog(string.Format("Time taken: {0}", _simulator.TimeTakenMessage));
           }


           WriteLogFile();
          
           if (_runContinousSimulationAtEnd)
           {
               Console.ForegroundColor = ConsoleColor.White;
               Console.WriteLine("Press any key to stop");
               do
               {
                   while (!Console.KeyAvailable)
                   {
                       for (int i = 0; i < _simulator.Cells.Count; i++)
                       {
                           PrintResult(_simulator.Cells[i]);
                           Thread.Sleep(300);
                       }
                   }
               } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
               
           }
           else
           {
               Console.WriteLine("");
               Console.ForegroundColor = ConsoleColor.Blue;
               Console.WriteLine("Would you like to view the log file? Press key: Y");
               Console.ForegroundColor = ConsoleColor.White;
               Console.WriteLine("");
               Console.WriteLine("");
               var key = Console.ReadKey();
               if (key.Key == ConsoleKey.Y)
               {
                   Console.ForegroundColor = ConsoleColor.DarkCyan;
                   Console.WriteLine();
                   Console.WriteLine(File.ReadAllText(_logFile));
               }
               Pause();
           }
       }

       private static void BenchmarkSimulator(string simulator, Cell[] items, int roundsStart, int roundsEnd, int increment)
       {
           do
           {
               if (simulator == typeof (IndexedSimulator).Name)
               {
                   _simulator = new IndexedSimulator(roundsStart, items);
               }
               else if (simulator == typeof (ListSimulator).Name)
               {
                   _simulator = new ListSimulator(roundsStart, items);
               }
               else if (simulator == typeof (HashSetSimulator).Name)
               {
                   _simulator = new HashSetSimulator(roundsStart, items);
               }
               else
               {
                   _simulator = new LargeSimulator(roundsStart, items);
               }

               _simulator.OnNotifyMessage += WriteLog;
               _simulator.OnNotifyResult += PrintResult;
               _simulator.NotifyOnceEachResultSetComplete = !_runContinousSimulationAtEnd;
               WriteLog(simulator);
               Console.ForegroundColor = ConsoleColor.Cyan;
               Console.WriteLine("Generating results...");
               Console.ForegroundColor = ConsoleColor.Gray;
               _simulator.Run();

               roundsStart += increment;

           } while (roundsStart < roundsEnd);
       }

       private static decimal GetCellSize()
       {
           unsafe
           {
               return sizeof(Cell);
           }
       }

      
       private static ISeedLife FindInputSeed()
       {
           var input = Console.ReadLine();
           if (string.IsNullOrWhiteSpace(input)) ShowHelp();
           ISeedLife seed;
           Console.ForegroundColor = ConsoleColor.Cyan;
           switch (input.ToLowerInvariant())
           {
               case "1":
                   _verifySample1 = true;
                   seed = new Sample1Seed();
                   WriteLog("Sample 1 ", true);
                   WriteLog("");
                   WriteLog(seed.GetSeed());
                   return seed;
               case "2":
                   seed = new Sample2Seed();
                   WriteLog("Sample 2 ", true);
                   WriteLog("");
                   WriteLog(seed.GetSeed());
                   return seed;
               case "g":
                   _runContinousSimulationAtEnd = true;
                   seed = new GliderGunSeed();
                   WriteLog("Glider Gun");
                   WriteLog("");
                   WriteLog(seed.GetSeed());
                   return seed;
               case "r":
                   _runContinousSimulationAtEnd = true;
                   Console.SetWindowSize(Console.LargestWindowWidth-5, Console.LargestWindowHeight-5);
                   seed = new RandomSeed(Console.LargestWindowHeight - 5, Console.LargestWindowWidth - 5);
                   WriteLog("Random Sample");
                   WriteLog(seed.GetSeed());
                   return seed;
               case "p":
                   _runContinousSimulationAtEnd = true;
                   Console.SetWindowSize(Console.LargestWindowWidth - 5, Console.LargestWindowHeight - 5);
                   seed = new PrimeSeed(Console.LargestWindowHeight - 5, Console.LargestWindowWidth - 5);
                   WriteLog("Prime Sample");
                   WriteLog(seed.GetSeed());
                   return seed;
               default:
                   try
                   {
                       var fileSeeder = new FileSeeder(input);
                       return fileSeeder;
                   }
                   catch (Exception ex)
                   {
                       ShowError("File input invalid", ex);
                   }
                   break;
           }

           Console.ForegroundColor = ConsoleColor.White;
           throw new InvalidDataException("No sample");
       }

       private static void PrintResult(IEnumerable<Cell> result)
       {
           if (_runContinousSimulationAtEnd)
               Console.Clear();

           var sb = new StringBuilder();
           var gr = from r in result group r by r.Y;

           foreach (var row in gr)
           {
               foreach (var cell in row)
               {
                   sb.Append((int) cell.Health);
               }
               sb.AppendLine();
           }

           //Console.Beep(37,100);
           Console.Write(sb.ToString());
           
       }

       private static IEnumerable<Cell> ParseInput(string input)
       {
           var cells = new List<Cell>();
           int lineCount = 1;
           int widthCount = 0;
           bool isFirstRun = true;
           try
           {
               using (var reader = new StringReader(input))
               {
                   string line = string.Empty;
                   do
                   {
                       line = reader.ReadLine();
                       if (!string.IsNullOrWhiteSpace(line))
                       {

                           if (isFirstRun) // safe guard to match input line lengths match
                           {
                               widthCount = line.Length;
                               isFirstRun = false;
                           }
                           else
                           {
                               if (line.Length>widthCount) 
                                   throw new ArgumentException("Input incorrect. Characters on all lines should have the same count: check line: " + lineCount+1);
                           }

                           int charCount = 1;
                           foreach (var chr in line.ToCharArray().AsParallel())
                           {
                               cells.Add(new Cell(charCount, lineCount, IsAlive(chr)));
                               charCount++;
                           }

                           lineCount++;
                       }

                   } while (line != null);
               }
           }
           catch (Exception ex)
           {
               Console.ForegroundColor = ConsoleColor.DarkRed;
               ShowError("Failed to parse input", ex);
           }
           return cells;
       }

       private static Health IsAlive(char chr)
       {
           if (chr == Convert.ToChar("1")) return Health.Alive;
           if (chr == Convert.ToChar("0")) return Health.Dead;

           throw new InvalidDataException(chr + " is not a valid alive or dead character");
       }

       private static void Setup()
       {
           Console.Title = TITLE;
           Console.ForegroundColor = ConsoleColor.White;
           Console.BackgroundColor = ConsoleColor.Black;
           Console.WindowLeft = Console.WindowTop = 0;
           Console.WindowWidth = 100;
           Console.WindowHeight = 80;

           _log = new StringBuilder();
           _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

           if (!Directory.Exists(_logDir))
           {
               try
               {
                   Directory.CreateDirectory(_logDir);
               }
               catch (Exception fu)
               {
                   ShowError("Log directory does not exist " + _logDir, fu);
                   Pause();
               }
           }
           else
           {
               if (_deleteLogFileOnStart)
               {
                   foreach (var ff in Directory.GetFiles(_logDir))
                   {
                       File.Delete(ff);
                   }
               }
           }
       }

       private static void ShowHeader()
       {
           Console.WriteLine(TITLE);
           Console.WriteLine("------------------------");
           Console.WriteLine("Sample by Chris McKelt");
       }

       private static void ShowHelp()
       {
           Console.ForegroundColor = ConsoleColor.Gray;
           Console.WriteLine("Enter:");
           Console.WriteLine("-- '1' for test sample 1");
           Console.WriteLine("-- '2' for test sample 2");
           Console.WriteLine("-- 'g' for glider gun");
           Console.WriteLine("-- 'r' for a random sample");
           Console.WriteLine("-- or paste a file path for text file input");
           Console.ForegroundColor = ConsoleColor.White;
       }

       private static void ShowError(string msg, Exception e)
       {
           Console.ForegroundColor = ConsoleColor.DarkRed;
         
           Console.WriteLine("------------------------");
           Console.WriteLine("Error");
           Console.WriteLine(msg);
           Console.WriteLine("------------------------");
           Console.WriteLine(e);
           Console.ReadLine();
           Environment.Exit(1);
       }

       private static void Pause()
       {
           Console.ForegroundColor = ConsoleColor.White;
           Console.WriteLine("");
           Console.WriteLine("\nPress any key to exit.");
           Console.ReadLine();
       }

     
       private static void WriteLog(string msg)
       {
           Console.WriteLine(msg);

           _log.AppendLine(msg);
       }

       private static void WriteLog(string msg, bool ignoreConsoleOutput)
       {
           if (!ignoreConsoleOutput)
            Console.WriteLine(msg);

           _log.AppendLine(msg);
       }

       private static void WriteLogFile()
       {
           try
           {
               string fileName = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}.log", "GameOfLife_", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, "_", DateTime.Now.ToString("hh_mm"), "_sample_size_", _sampleSize, "_debug");
             
               _logFile = Path.Combine(_logDir, fileName);
               File.AppendAllText(_logFile, _log.ToString() + Environment.NewLine);     
           }
           catch (Exception ex)
           {
               ShowError("Log file creation failed", ex);
           }

           try
           {
               if (_results != null && _results.Any())
               {
                   string fileName = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}.log", "GameOfLife_", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, "_", DateTime.Now.ToString("hh_mm"), "_sample_size_", _sampleSize, "_results");

                   string resultsFile = Path.Combine(_logDir, fileName);
                   using (var fs = new FileStream(resultsFile, FileMode.OpenOrCreate))
                   using (var fw = new StreamWriter(fs))
                   {
                       foreach (var result in _results)
                       {
                           fw.WriteLine(result.ToString());
                       }
                       fw.Close();
                       fs.Close();
                   }
                 
               }
           }
           catch (Exception ex)
           {
               ShowError("Log file creation failed", ex);
           }
       }

       private static void VerifySample1()
       {
           if (_verifySample1)
           {
               const string expected1 = @"
01000
00110
01000
";

               const string expected2 = @"
00100
01100
00100
";

               const string expected3 = @"
01100
01110
01100
";
               var expectedCells = new Dictionary<int, IEnumerable<Cell>>
                   {
                       {1, ParseInput(expected1)},
                       {2, ParseInput(expected2)},
                       {3, ParseInput(expected3)}
                   };

               foreach (var expectedCellList in expectedCells.Where(a => a.Key > 0).GroupBy(b => b.Key)) // skip zero original generation
               {
                   foreach (var kvp in expectedCellList)
                   {
                       foreach (var expectedCell in kvp.Value)
                       {
                           var actualCell =
                               _simulator.Cells[kvp.Key].SingleOrDefault(a => a.X == expectedCell.X && a.Y == expectedCell.Y);

                           if (!actualCell.Equals(expectedCell))
                           {
                               Console.ForegroundColor = ConsoleColor.DarkRed;
                               Console.WriteLine("Failed");
                               Console.WriteLine("Excepted   {0}  X: {1}  Y: {1} Health:{2}", expectedCell.X, expectedCell.Y, expectedCell.Health);
                               Console.WriteLine("Actual     {0}  X: {1}  Y: {1} Health:{2}", actualCell.X, actualCell.Y, actualCell.Health);
                               Console.WriteLine();
                           }
                       }
                   }
               }
               Console.ForegroundColor = ConsoleColor.DarkYellow;
               Console.WriteLine("Sample 1 test passed");
               Console.ForegroundColor = ConsoleColor.White;
           }
       }

       internal struct Result
       {
           private readonly string _simulatorName;
           private readonly int _rounds;
           private readonly long _totalTime;
           private readonly long _calculationTime;

           public Result(string simulatorName, int rounds, long totalTime, long calculationTime)
           {
               _simulatorName = simulatorName;
               _rounds = rounds;
               _totalTime = totalTime;
               _calculationTime = calculationTime;
           }

           public long TotalTime
           {
               get { return _totalTime; }
           }

           public string SimulatorName
           {
               get { return _simulatorName; }
           }

           public int Rounds
           {
               get { return _rounds; }
           }

           public long CalculationTime
           {
               get { return _calculationTime; }
           }

           public override string ToString()
           {
               return string.Format("Simulator: {0,10} \t  Rounds: {1,3} Total time: {2,10} Calculation time: {3,10}", SimulatorName,
                                    Rounds, TotalTime, CalculationTime);
           }
       }


       public static int GetSizeOfObject(object obj)
       {

           unsafe
           {
               return sizeof(Cell);
           }
       }
   }
}
