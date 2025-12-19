using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab02_Widerstand
{
    class Resistor// Bauplan = Klasse
    {   //Attribute, Member Variablen
        private string label; //Bezeichnung, R1 etc.
        private double value; // Wert R
        private double toleranz; //1 Prozent etc
        private double maxPower; // 10 f. 10 Watt
                                 //Methoden
                                 // Konstruktor
        public Resistor(string label, double value, double toleranz, double maxPower)
        {
            this.value = value;

            this.label = label;
            this.maxPower = maxPower;
            this.toleranz = toleranz ;
        }
        public double GetValue()
        { return value; }
        public double CalculateCurrent(double voltage)
        {
            double i = voltage / value;
            return i; }
        public double CalculateVoltage(double current) 
        { double u = this.value * current;
            return current;
        }
        public Resistor InSerieMit(Resistor R2)
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            //Obj sind Instanzen d. Klasse
            Resistor R1 = new Resistor("R1",100,5,10);// obj r1 vom  typ resistor
            Resistor R2 = new Resistor("R2",200,1,20);// obj r2...
            
            double current = R1.CalculateCurrent(5);
            Resistor Rges = R1.InSerieMit(R2);
        }
    }
}
