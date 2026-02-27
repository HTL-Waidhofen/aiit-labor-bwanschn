using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lab06_Labyrinth
{
    internal class Figur
    {
        // Ein ganz einfaches Objekt für die Spielfigur
        // Speichert x/y und hat eine sichtbare Ellipse (Visual)
        public int X { get; private set; }
        public int Y { get; private set; }
        Ellipse geometrie;

        // Erzeuge Figur an Position x/y. Breite/Höhe sind optional.
        public Figur(int x, int y, int breite = 10, int hoehe = 10)
        {
            X = x; Y = y;
            geometrie = new Ellipse();
            geometrie.Width = breite;
            geometrie.Height = hoehe;
            geometrie.Fill = Brushes.Blue; // einfache Farbe
        }

        // Das UI-Element, das wir dem Canvas hinzufügen
        public UIElement Visual => geometrie;

        // Setze die Position der sichtbaren Figur auf dem Canvas
        public void SetPositionOnCanvas(Canvas canvas, int cellSize)
        {
            Canvas.SetLeft(geometrie, X * cellSize + 1);
            Canvas.SetTop(geometrie, Y * cellSize + 0);
        }

        // Bewege die Figur auf neue Koordinaten
        public void MoveTo(int x, int y)
        {
            X = x; Y = y;
        }
    }
}
