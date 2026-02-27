using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab06_Labyrinth
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Wir speichern die Zeilen vom Labyrinth
        private string[] zeilen;
        // Anzahl Reihen und Spalten
        private int rows, cols;
        // Pixel-Größe pro Feld
        private int cellSize = 15;
        // Das Labyrinth: jede Zeile ist ein String mit Zeichen wie '#' '.' 'S' 'E'
        // Anzahl der Reihen (rows) und Spalten (cols) im Labyrinth
        // Pixelgröße eines Feldes auf dem Canvas (wie groß wird ein Zellen-Rechteck)

        // Wand-Klasse: ganz einfach, hat Position und ein Rechteck (Visual)
        private class Wand { public int X, Y; public Rectangle R; }
        // Liste aller Wände (nur logische Speicherung, aber wir fügen auch das Rechteck dem Canvas hinzu)
        private List<Wand> waende = new List<Wand>();

        // Spielfigur als Objekt (unsere Figur-Klasse)
        private Figur figur = null;
        public MainWindow()
        {
            InitializeComponent();
            // Versuche die Datei zu lesen, die das Labyrinth beschreibt.
            // Wenn sie nicht da ist, machen wir ein einfaches, kleines Labyrinth.
            if (File.Exists("maze_20x20.txt"))
            {
                // Datei gefunden, lade alle Zeilen rein
                zeilen = File.ReadAllLines("maze_20x20.txt");
            }
            else
            {
                // Keine Datei: Wir bauen ein 20x20 Labyrinth selber.
                // Ränder sind Wände (#) und innen ist Platz (.)
                int size = 20;
                zeilen = new string[size];
                for (int r = 0; r < size; r++)
                {
                    if (r == 0 || r == size - 1)
                        zeilen[r] = new string('#', size); // obere/untere wand
                    else
                        zeilen[r] = "#" + new string('.', size - 2) + "#"; // seitenwände, innen frei
                }
                // Setze ein S (Start) und ein E (Ende) an einfache Plätze
                char[] first = zeilen[1].ToCharArray(); first[1] = 'S'; zeilen[1] = new string(first);
                char[] last = zeilen[size - 2].ToCharArray(); last[size - 2] = 'E'; zeilen[size - 2] = new string(last);
            }
           
            

            // Bestimme Größe und zeichne das Labyrinth. Speichere Wände in waende-Liste
            rows = zeilen.Length;
            cols = zeilen.Max(l => l.TrimEnd('\r', '\n').Length);
            Spielfeld.Width = cols * cellSize; Spielfeld.Height = rows * cellSize; Spielfeld.Background = Brushes.Black;

            for (int y = 0; y < rows; y++)
            {
                string line = zeilen[y].TrimEnd('\r', '\n');
                for (int x = 0; x < cols; x++)
                {
                    char ch = x < line.Length ? line[x] : '#';
                    Rectangle cell = new Rectangle { Width = cellSize - 1, Height = cellSize - 1 };
                    switch (ch)
                    {
                        case '#': cell.Fill = Brushes.DarkSlateGray; break;
                        case 'S': cell.Fill = Brushes.Green; break;
                        case 'E': cell.Fill = Brushes.Red; break;
                        default: cell.Fill = Brushes.White; break;
                    }
                    Canvas.SetLeft(cell, x * cellSize); Canvas.SetTop(cell, y * cellSize); Spielfeld.Children.Add(cell);

                    if (ch == '#') // speichere Wand als Objekt
                    {
                        var w = new Wand() { X = x, Y = y, R = cell };
                        waende.Add(w);
                    }
                }
            }

            // Einfaches Start-Verhalten: Figur in der Mitte, aber nicht auf einer Wand
            int midX = cols / 2, midY = rows / 2;
            int sx = midX, sy = midY;
            if (midY < zeilen.Length)
            {
                string midLine = zeilen[midY].TrimEnd('\r', '\n');
                if (midX < midLine.Length && midLine[midX] == '#')
                {
                    // suche erstes freies Feld (sequentiell, einfach)
                    bool found = false;
                    for (int y = 0; y < rows && !found; y++)
                        for (int x = 0; x < cols && !found; x++)
                            if ((x < zeilen[y].Length ? zeilen[y][x] : '#') != '#') { sx = x; sy = y; found = true; }
                }
            }

            // Erzeuge Figur und füge deren Visual dem Canvas hinzu
            figur = new Figur(sx, sy, cellSize - 2, cellSize - 2);
            Spielfeld.Children.Add(figur.Visual);
            figur.SetPositionOnCanvas(Spielfeld, cellSize);

            // Fokussieren, damit KeyDown funktioniert
            this.Loaded += (s, e) => { Keyboard.Focus(this); };
            // KeyDown ist im Code-behind (hier), XAML-Ereignis optional
            this.KeyDown += MainWindow_KeyDown;
        }

        // Tasten bewegen die Figur (WASD). Wir prüfen Wände in der zeilen-Array
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            int nx = figur.X, ny = figur.Y;
            switch (e.Key)
            {
                case Key.W: ny--; break;
                case Key.S: ny++; break;
                case Key.A: nx--; break;
                case Key.D: nx++; break;
                default: return;
            }

            if (nx < 0 || nx >= cols || ny < 0 || ny >= rows) return;
            string line = zeilen[ny].TrimEnd('\r', '\n');
            if (nx >= line.Length) return;
            if (line[nx] == '#') return; // wenn wand, nicht bewegen

            // move figur und update visual
            figur.MoveTo(nx, ny);
            figur.SetPositionOnCanvas(Spielfeld, cellSize);
        }
    }
}
