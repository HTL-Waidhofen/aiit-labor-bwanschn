using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab04_Bruchrechnung
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Bitte Bruchrechnung eingeben ");
            string line = Console.ReadLine();



            Bruchrechnung b = Bruchrechnung.Parse(line);
            

            Console.WriteLine(b.GetResult());
            Console.ReadKey();

        }
    }
}
