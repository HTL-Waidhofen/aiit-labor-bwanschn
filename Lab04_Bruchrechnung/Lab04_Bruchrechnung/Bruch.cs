using System;

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
        {
            if (nenner == 0)
                throw new Exception();
            this.nenner = nenner;
        }

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
            for (int i = little; i > 1; i--)
            {
                if ((zaehler % i == 0) &&
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
        public void sub(Bruch b)
        {
            int n = this.nenner * b.getNenner();
            int z = this.zaehler * b.getNenner() - b.getZaehler() * this.nenner;
            this.nenner = n;
            this.zaehler = z;
            Kuerzen();
        }
        public void mul(Bruch b)
        {
            int n = this.nenner * b.getNenner();
            int z = this.zaehler * b.getZaehler();
            this.nenner = n;
            this.zaehler = z;
            Kuerzen();
        }

        public void div(Bruch b)
        {
            int n = this.nenner * b.getZaehler();
            int z = this.zaehler * b.getNenner();

            this.nenner = n;
            this.zaehler = z;

            // Nenner positiv halten
            if (this.nenner < 0)
            {
                this.nenner = -this.nenner;
                this.zaehler = -this.zaehler;
            }

        }
    }
}
