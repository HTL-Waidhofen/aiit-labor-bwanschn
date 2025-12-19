using System;
using System.Globalization;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("Widerstand (Ohm, kOhm, MOhm): ");
            string inputR = Console.ReadLine()?.Trim();

            if (inputR == null)
                continue;

            if (inputR.Equals("q", StringComparison.OrdinalIgnoreCase) ||
                inputR.Equals("ende", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Auf Wiedersehen!");
                break;
            }

            if (!TryParseValueWithUnit(inputR, out double resistance))
            {
                Console.WriteLine("Falsche Eingabe!");
                continue;
            }

            Console.Write("Spannung (V, kV, mV): ");
            string inputU = Console.ReadLine()?.Trim();

            if (!TryParseValueWithUnit(inputU, out double voltage))
            {
                Console.WriteLine("Falsche Eingabe!");
                continue;
            }

            if (resistance == 0)
            {
                Console.WriteLine("Falsche Eingabe!");
                continue;
            }

            double current = voltage / resistance;
            Console.WriteLine($"Die Stromstärke beträgt {current:0.00} Ampere.");
        }
    }

    static bool TryParseValueWithUnit(string input, out double valueInBaseUnit)
    {
        valueInBaseUnit = 0;

        input = input.Replace(" ", "").ToLower();

        double factor;
        if (input.EndsWith("mohm"))
            factor = 1_000_000;
        else if (input.EndsWith("kohm"))
            factor = 1_000;
        else if (input.EndsWith("ohm"))
            factor = 1;
        else if (input.EndsWith("kv"))
            factor = 1_000;
        else if (input.EndsWith("mv"))
            factor = 0.001;
        else if (input.EndsWith("v"))
            factor = 1;
        else
            return false;

        string numberPart = "";
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsDigit(input[i]) || input[i] == '.' || input[i] == ',')
                numberPart += input[i];
            else
                break;
        }

        numberPart = numberPart.Replace(',', '.');

        if (!double.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            return false;

        valueInBaseUnit = number * factor;
        return true;
    }
}
