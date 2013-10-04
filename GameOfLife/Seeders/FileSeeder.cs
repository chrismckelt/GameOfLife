using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Seeders
{
    public class FileSeeder : ISeedLife
    {
        private readonly string _filePath;

        public FileSeeder(string filePath)
        {
            _filePath = filePath;
            if (!File.Exists(filePath))
            {
               throw new FileNotFoundException(filePath);
            }
        }

        public string GetSeed()
        {
            return File.ReadAllText(_filePath);
        }
    }
}
