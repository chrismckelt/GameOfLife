using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
   /// <summary>
    /// http://en.wikipedia.org/wiki/Conway%27s_Game_of_Life
   /// </summary>
    class Program
   {
       private static int _generations = 300;
       private static string _sampleInput = @"
10100
00100
10100
";

       private static string _sampleInput2 = @"
1010000100101010111100111
0110010101000111001010101
0101111000101010001101010
";


       static void Main(string[] args)
       {
           Setup();
           ShowHeader();
           ShowHelp();

           string sample = string.Empty;
           sample = GetInputSample(sample);
           
           Console.ForegroundColor = ConsoleColor.White;

           IList<Cell> inputCells = ParseInput(sample);
           var simulator = new Simulator(_generations,inputCells);
           simulator.OnNotifyMessage += Console.WriteLine;
           simulator.OnNotifyResult += PrintResult;
           simulator.Run();

           Console.ForegroundColor = ConsoleColor.DarkGreen;
           Console.WriteLine("Time taken: {0}", simulator.TimeTaken);

           Pause();
       }

       private static string GetInputSample(string sample)
       {
           var input = Console.ReadLine();

           switch (input.ToLowerInvariant())
           {
               case "1":
                   sample = _sampleInput;
                   break;
               case "r":
                   sample = GenerateRandomString(75, 75);
                   break;
               default:
                   try
                   {
                       if (!File.Exists(input))
                       {
                           ShowHelp();
                       }

                       sample = File.ReadAllText(input);
                   }
                   catch (Exception ex)
                   {
                       ShowError("File input invalid", ex);
                   }
                   break;
           }
           return sample;
       }

       private static void PrintResult(IList<Cell> result)
       {

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

           Console.Write(sb.ToString());
       }

       private static IList<Cell> ParseInput(string input)
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
           Console.ForegroundColor = ConsoleColor.White;
           Console.BackgroundColor = ConsoleColor.Black;
           Console.WindowLeft = Console.WindowTop = 0;
           Console.WindowWidth = 80;
           Console.WindowHeight = 80;
       }

       private static void ShowHeader()
       {          
           Console.WriteLine("Conway's Game of Life");
           Console.WriteLine("------------------------");
           Console.WriteLine("Sample by Chris McKelt");
         
       }

       private static void ShowHelp()
       {
           Console.ForegroundColor = ConsoleColor.DarkGray;
           Console.WriteLine("Enter:");
           Console.WriteLine("-- '1' for test sample");
           Console.WriteLine("-- paste a file path for text file input");
           Console.WriteLine("-- 'R' for a random sample");
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
   }
}
