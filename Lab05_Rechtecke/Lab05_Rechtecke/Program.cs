using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab05_Rechtecke
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            var rects = new List<RectangleModel>();

            int count = ReadInt("Wie viele Rechtecke sollen gezeichnet werden?", min: 1);

            for (int i = 0; i < count; i++)
            {
                Console.WriteLine();
                Console.WriteLine($"Rechteck #{i + 1}:");
                int width = ReadInt("Breite (>=2):", min: 2);
                int height = ReadInt("Höhe (>=2):", min: 2);
                int x = ReadInt("X-Position (linke obere Ecke, >=0):", min: 0);
                int y = ReadInt("Y-Position (linke obere Ecke, >=0):", min: 0);

                rects.Add(new RectangleModel(x, y, width, height));
            }

            DrawAll(rects);
            Console.WriteLine();
            Console.WriteLine("Steuerung: W/A/S/D oder Pfeiltasten = bewegen, Esc = beenden.");

            if (rects.Count == 0)
            {
                Console.WriteLine("Keine Rechtecke vorhanden. Beliebige Taste zum Beenden.");
                Console.ReadKey(true);
                return;
            }

            // Interaktiver Modus: letztes Rechteck bewegen
            var selected = rects.Last();

            while (true)
            {
                var key = Console.ReadKey(true);
                int dx = 0, dy = 0;
                bool attempted = false;

                switch (key.Key)
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        dy = -1;
                        attempted = true;
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        dy = 1;
                        attempted = true;
                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        dx = -1;
                        attempted = true;
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        dx = 1;
                        attempted = true;
                        break;
                    case ConsoleKey.Escape:
                        Console.CursorVisible = true;
                        return;
                }

                if (attempted)
                {
                    // Kandidatenposition berechnen und an Fenstergröße anpassen
                    int candX = selected.X + dx;
                    int candY = selected.Y + dy;

                    candX = Math.Max(0, Math.Min(candX, Math.Max(0, Console.WindowWidth - selected.Width)));
                    candY = Math.Max(0, Math.Min(candY, Math.Max(0, Console.WindowHeight - selected.Height)));

                    // Prüfen, ob die Kandidatenposition mit anderen Rechtecken überlappt
                    bool collides = rects.Any(r => !ReferenceEquals(r, selected) && selected.OverlapsAt(candX, candY, r));

                    if (!collides)
                    {
                        selected.X = candX;
                        selected.Y = candY;
                        selected.ClampToConsole();
                        DrawAll(rects);
                        Console.WriteLine();
                        Console.WriteLine("Steuerung: W/A/S/D oder Pfeiltasten = bewegen, Esc = beenden.");
                    }
                    else
                    {
                        // Signal: Bewegung blockiert
                        try { Console.Beep(); } catch { /* some consoles may not support Beep */ }
                    }
                }
            }
        }

        private static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(prompt + " ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int value))
                {
                    if (value < min)
                    {
                        Console.WriteLine($"Wert muss mindestens {min} sein.");
                        continue;
                    }
                    if (value > max)
                    {
                        Console.WriteLine($"Wert darf höchstens {max} sein.");
                        continue;
                    }
                    return value;
                }
                Console.WriteLine("Ungültige Zahl, bitte erneut eingeben.");
            }
        }

        private static void DrawAll(IEnumerable<RectangleModel> rects)
        {
            Console.Clear();
            foreach (var r in rects)
            {
                r.Draw();
            }
        }
    }

    internal class RectangleModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; }
        public int Height { get; }

        public RectangleModel(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            ClampToConsole();
        }

        public void Draw()
        {
            try
            {
                // Ecken
                SetSafeCursor(X, Y); Console.Write('+');
                SetSafeCursor(X + Width - 1, Y); Console.Write('+');
                SetSafeCursor(X, Y + Height - 1); Console.Write('+');
                SetSafeCursor(X + Width - 1, Y + Height - 1); Console.Write('+');

                // obere und untere Kante
                for (int dx = 1; dx < Width - 1; dx++)
                {
                    SetSafeCursor(X + dx, Y); Console.Write('-');
                    SetSafeCursor(X + dx, Y + Height - 1); Console.Write('-');
                }

                // linke und rechte Kante
                for (int dy = 1; dy < Height - 1; dy++)
                {
                    SetSafeCursor(X, Y + dy); Console.Write('|');
                    SetSafeCursor(X + Width - 1, Y + dy); Console.Write('|');
                }
            }
            catch
            {
                // Bei Fenstergrößen-Änderungen kann SetCursorPosition fehlschlagen. Sicher bleiben.
            }
        }

        public void ClampToConsole()
        {
            int maxX = Math.Max(0, Console.WindowWidth - Width);
            int maxY = Math.Max(0, Console.WindowHeight - Height);

            if (X < 0) X = 0;
            if (Y < 0) Y = 0;
            if (X > maxX) X = maxX;
            if (Y > maxY) Y = maxY;
        }

        /// <summary>
        /// Prüft, ob dieses Rechteck an Position (newX,newY) mit dem anderen Rechteck überlappt.
        /// Berührung an Kanten gilt nicht als Überlappung (also Kanten dürfen sich berühren).
        /// </summary>
        public bool OverlapsAt(int newX, int newY, RectangleModel other)
        {
            return newX < other.X + other.Width
                   && newX + Width > other.X
                   && newY < other.Y + other.Height
                   && newY + Height > other.Y;
        }

        private void SetSafeCursor(int x, int y)
        {
            if (x < 0 || y < 0) return;
            if (x >= Console.WindowWidth || y >= Console.WindowHeight) return;
            Console.SetCursorPosition(x, y);
        }
    }
}
