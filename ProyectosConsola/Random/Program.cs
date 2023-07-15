using System;
using System.Collections.Generic;
using System.IO;

namespace Randoom
{
    class Program
    {
        static void Main(string[] args)
        {
            int maxNumbers = 10000000;
            string outputFileName = "Random.txt";

            var random = new Random();
            var numbers = new HashSet<int>();

            while (numbers.Count < maxNumbers)
            {
                int randomNumber = random.Next();
                numbers.Add(randomNumber);
            }

            using (StreamWriter writer = new StreamWriter(outputFileName))
            {
                foreach (int number in numbers)
                {
                    writer.WriteLine(number);
                }
            }

            Console.WriteLine($"Archivo generado: {outputFileName}");
        }
    }
}
