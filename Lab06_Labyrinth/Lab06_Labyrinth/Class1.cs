using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lab06_Labyrinth
{
    internal class Figur
    {
        int hoehe = 10;
        int breite = 10;
        int x;
        int y;
        Ellipse geometrie;
        public Figur(int x, int y)
        {
            this.x = x;
            this.y = y;
            geometrie = new Ellipse();


               geometrie.Width = breite;
            geometrie.Height = hoehe;
            geometrie.Fill = Brushes.Red;
           
        }
        public void Bewegen(int dx, int dy)
        {
            x += dx;
            y += dy;
        }
    }
}
