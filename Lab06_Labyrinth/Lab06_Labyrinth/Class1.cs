using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lab06_Labyrinth
{
    internal class Figur
    {
        // Ein ganz einfaches Objekt für die Spielfigur
        // Speichert x/y und hat eine sichtbare Ellipse (Visual)
        public int X { get; private set; }
        public int Y { get; private set; }
        // Das sichtbare Element der Figur: entweder ein Bild oder eine Ellipse
        UIElement visual;

        // Erzeuge Figur an Position x/y. Breite/Höhe sind optional.
        public Figur(int x, int y, int breite = 10, int hoehe = 10)
        {
            X = x; Y = y;
            // Versuche ein Bild zu laden (player.png im Programmordner).
            // Wenn die Datei nicht vorhanden ist, benutzen wir eine einfache Ellipse.
            string imgFile = "player.png";
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imgFile);
            if (File.Exists(fullPath))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();

                    var img = new Image();
                    img.Source = bmp;
                    img.Width = breite;
                    img.Height = hoehe;
                    visual = img;
                }
                catch
                {
                    // falls laden fehlschlägt, fallback zur Ellipse
                    var e = new Ellipse();
                    e.Width = breite; e.Height = hoehe; e.Fill = Brushes.Blue;
                    visual = e;
                }
            }
            else
            {
                // Kein Bild gefunden: einfache Ellipse als Sichtbarkeit
                var e = new Ellipse();
                e.Width = breite; e.Height = hoehe; e.Fill = Brushes.Blue;
                visual = e;
            }
        }

        // Das UI-Element, das wir dem Canvas hinzufügen
        public UIElement Visual => visual;

        // Setze die Position der sichtbaren Figur auf dem Canvas
        public void SetPositionOnCanvas(Canvas canvas, int cellSize)
        {
            Canvas.SetLeft(visual, X * cellSize + 1);
            Canvas.SetTop(visual, Y * cellSize + 0);
        }

        // Bewege die Figur auf neue Koordinaten
        public void MoveTo(int x, int y)
        {
            X = x; Y = y;
        }
    }
}
