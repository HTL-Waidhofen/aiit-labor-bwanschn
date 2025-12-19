using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Resistor      // Klasse = Bauplan
{
    // Atribute, Member Variablen
    private string label; // Bezeichung fpr R1,R2;...
    private double value; //Widerstandswert
    private double toleranz; // zb 1 für 1%, 5 für 5%
    private double maxPower; // zb 10 für 10 Watt

    //Methoden
    // Konstruktor
    public Resistor(string label, double value, double toleranz, double maxPower)
    {
        this.label = label;
        this.value = value;
        this.toleranz = toleranz;
        this.maxPower = maxPower;

    }
    //Zugriffmethoden Getter and Setter
    public double GetValue()
    {
        return value;
    }

    public double CalculateCurrent(double voltage)
    {
        double Current = voltage / this.value;
        return Current;
    }
    public double CalculateVoltage(double current)
    {
        double voltage = this.value * current;
        return voltage;
    }
    public Resistor inSeriemit(Resistor r2)
    {
        Resistor Rges = new Resistor("Rges", this.value + r2.value, Math.Min(this.toleranz, r2.toleranz), Math.Min(this.maxPower, r2.maxPower));
        return Rges;
    }
    public Resistor inParallelemit(Resistor r2)
    {
        double RgesValue = 1 / (1 / this.value + 1 / r2.value);
        Resistor Rges = new Resistor("Rges", RgesValue, Math.Min(this.toleranz, r2.toleranz), Math.Min(this.maxPower, r2.maxPower));
        return Rges;
    }


}
internal class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Ersten Widerstand eingeben (oder 'ende'): ");
            string input1 = Console.ReadLine();

            if (input1.ToLower() == "ende")
                break;

            Console.WriteLine("Zweiten Widerstand eingeben: ");
            string input2 = Console.ReadLine();

            Console.WriteLine("Schaltung (seriell / parallel): ");
            string schaltung = Console.ReadLine().ToLower();

            double r1Value = double.Parse(input1);
            double r2Value = double.Parse(input2);

            Resistor r1 = new Resistor("R1", r1Value, 5, 10);
            Resistor r2 = new Resistor("R2", r2Value, 5, 10);

            Resistor rGes;

            if (schaltung == "seriell")
            {
                rGes = r1.inSeriemit(r2);
            }
            else if (schaltung == "parallel")
            {
                rGes = r1.inParallelemit(r2);
            }
            else
            {
                Console.WriteLine("Unbekannte Schaltungsart!");
                continue;
            }

            Console.WriteLine($"Gesamtwiderstand: {rGes.GetValue()} Ohm");
            Console.WriteLine("------------------------------------");
        }

        Console.WriteLine("Programm beendet.");
        Console.ReadKey();
    }
}