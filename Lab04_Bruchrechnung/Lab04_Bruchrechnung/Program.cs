using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab04_Bruchrechnung
{
    class Bruch
    {
        private int zaehler;
        private int nenner;

        public Bruch(int zeahler, int nenner)
        {
            this.zaehler = zeahler;
            this.nenner = nenner;
        }
        public int getZaehler()
        {
            return zaehler;
        }

        public override string ToString()
        {
            return $"{zaehler} / {nenner}";
        }
        public int getNenner()
        {
            return nenner;
        }

        public void SetNenner(int nenner)
        {   if (nenner == 0)
                throw new Exception();
            this.nenner = nenner; }

        public void SetZaehler(int zaehler)
        {
            this.zaehler = zaehler;
        }
        public static Bruch Parse(string str)
        {
            string[] teile = str.Split('/');
            int zaehler = int.Parse(teile[0]);
            int nenner = int.Parse(teile[1]);
            return new Bruch(zaehler, nenner);
        }
        public void Kuerzen()
        {
            int little = Math.Min(zaehler, nenner);
            for (int i = little; i >1; i--)
            {
                if((zaehler % i== 0)&&
                    (nenner % i == 0))
                    {

                    zaehler /= i;
                    nenner /= i;
                }
            }
            
        }

        public void Add(Bruch b)
        {
            int n = this.nenner * b.getNenner();
            int z = this.zaehler * b.getNenner() + b.getZaehler() * this.nenner;
            this.nenner = n;
            this.zaehler = z;
            Kuerzen();
        }
    }
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

            Console.WriteLine(b1);
            Console.ReadKey();

        }
    }
}
