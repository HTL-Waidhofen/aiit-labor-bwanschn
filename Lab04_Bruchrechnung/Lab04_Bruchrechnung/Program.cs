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

            Console.WriteLine("Eingebe");
            string line1 = Console.ReadLine();
            Console.WriteLine("Eingebe");
            string line2 = Console.ReadLine();

            Bruch b1 = Bruch.Parse(line1);
            Bruch b2 = Bruch.Parse(line2);
          

            b1.Add(b2);

            b1.sub(b2);

            b1.div(b2) ;
            b1.mul(b2);

            Console.WriteLine(b1);
            Console.ReadKey();

        }
    }
}
