﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLife
{
   /// <summary>
    /// http://en.wikipedia.org/wiki/Conway%27s_Game_of_Life
   /// </summary>
    class Program
   {
       private const int ROUNDS = 10;
       private const string LOG_DIRECTORY = @"D:\Development\GameOfLife\Log\";
       private const string SAMPLE_INPUT = @"
10100
00100
10100
";

       private const string SAMPLE_INPUT2 = @"
1010000100101010111100111
0110010101000111001010101
0101111000101010001101010
1001010110101110101011010
1100101010110010110010110
";

       private const string TITLE = "Conway's Game of Life";
       private static bool _runRandomSample = false;
       private static bool _verifySample1 = false;
       private static SimulatorBase _simulator;
       private static StringBuilder _log;
       private static string _logFile;
      

       static void Main(string[] args)
       {
           Setup();
           ShowHeader();
           ShowHelp();
           string sample = GetInputSample();
           var inputCells = ParseInput(sample);

           var items = inputCells as Cell[] ?? inputCells.ToArray();
           var objectCount = items.Sum(a => a.X * a.Y);
           if (objectCount >= (2048 * 2048))  // find out L1 cache size (sysinternals)
           {
               _simulator = new LargeSimulator(ROUNDS, items);
               Console.WriteLine("Using the large data simulator");
           }
           else
           {
               _simulator = new Simulator(ROUNDS, items);
               Console.WriteLine("Using the normal simulator");
           }
          
           _simulator.OnNotifyMessage += WriteLog;
           _simulator.OnNotifyResult += PrintResult;
           _simulator.NotifyOnceEachResultSetComplete = !_runRandomSample;
           Console.WriteLine("Generating results...");
           //Console.WriteLine(sample);
           _simulator.Run();

           Console.ForegroundColor = ConsoleColor.DarkGreen;
           WriteLog(string.Format("Time taken: {0}", _simulator.TimeTaken));

           VerifySample1();
           WriteLogFile();
           Pause();

           if (_runRandomSample)
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
                           Thread.Sleep(500);
                       }
                   }
               } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
               
           }
       }


       private static string GetInputSample()
       {
           var input = Console.ReadLine();

           switch (input.ToLowerInvariant())
           {
               case "1":
                   _verifySample1 = true;
                   return SAMPLE_INPUT;
               case "2":
                   return SAMPLE_INPUT2;
               case "r":
                   _runRandomSample = true;
                   var rnd = new Random();
                   int height = rnd.Next(5, 30);
                   int width = rnd.Next(5, 30);
                   if (height > Console.WindowHeight && width > Console.WindowWidth)
                        Console.SetWindowSize(width,height);
                   return GenerateRandomString(80,80);
                   //return GenerateRandomString(height, width);
               default:
                   try
                   {
                       if (!File.Exists(input))
                       {
                           ShowHelp();
                       }

                       return File.ReadAllText(input);
                   }
                   catch (Exception ex)
                   {
                       ShowError("File input invalid", ex);
                   }
                   break;
           }
           throw new ArgumentNullException("No sample");
       }

       private static void PrintResult(IEnumerable<Cell> result)
       {
           if (_runRandomSample)
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
                           foreach (var chr in line.ToCharArray())
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
           if (!Directory.Exists(LOG_DIRECTORY))
               ShowError("Log directory does not exist " + LOG_DIRECTORY, new DirectoryNotFoundException(LOG_DIRECTORY));
           
       }

       private static void ShowHeader()
       {
           Console.WriteLine(TITLE);
           Console.WriteLine("------------------------");
           Console.WriteLine("Sample by Chris McKelt");
       }

       private static void ShowHelp()
       {
           Console.ForegroundColor = ConsoleColor.DarkGray;
           Console.WriteLine("Enter:");
           Console.WriteLine("-- '1' for test sample 1");
           Console.WriteLine("-- '2' for test sample 2");
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
           Console.WriteLine("");
           Console.WriteLine("\nPress any key to exit.");
           Console.ReadLine();
       }

       private static string GenerateRandomString(int height, int width)
       {
           var sb = new StringBuilder();
           var random = new Random();

           for (int a = 0; a < height; a++)
           {
               for (int b = 0; b < width; b++)
               {
                   sb.Append(random.Next(1,3) % 2 == 0 ? "1" : "0");
               }
               sb.AppendLine();
           }

           return sb.ToString();
       }

       private static void WriteLog(string msg)
       {
           Console.WriteLine(msg);

           _log.AppendLine(msg);
       }

       private static void WriteLogFile()
       {
           try
           {
               _logFile = Path.Combine(LOG_DIRECTORY, string.Format("gameoflife-{0:yyyy-MM-dd_hh-mm-ss-tt}.log", DateTime.Now));
              File.WriteAllText(_logFile, _log.ToString());
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
               Console.WriteLine("Sample 1 test passed");
           }
       }
   }
}
