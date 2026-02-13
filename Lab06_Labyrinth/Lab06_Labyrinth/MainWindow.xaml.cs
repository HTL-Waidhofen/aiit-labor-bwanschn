using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
            // try to read a maze file; if not found create a default 20x20 maze
            string[] zeilen;
            if (File.Exists("maze_20x20.txt"))
            {
                zeilen = File.ReadAllLines("maze_20x20.txt");
            }
            else
            {
                // default: 20x20 with border walls and open inside, start S and end E
                int size = 20;
                zeilen = new string[size];
                for (int r = 0; r < size; r++)
                {
                    if (r == 0 || r == size - 1)
                        zeilen[r] = new string('#', size);
                    else
                        zeilen[r] = "#" + new string('.', size - 2) + "#";
                }
                // place start and end
                char[] first = zeilen[1].ToCharArray(); first[1] = 'S'; zeilen[1] = new string(first);
                char[] last = zeilen[size - 2].ToCharArray(); last[size - 2] = 'E'; zeilen[size - 2] = new string(last);
            }

            // determine grid size
            int rows = zeilen.Length;
            int cols = zeilen.Max(l => l.TrimEnd('\r', '\n').Length);

            const int cellSize = 15; // pixels per cell
            Spielfeld.Width = cols * cellSize;
            Spielfeld.Height = rows * cellSize;
            this.Spielfeld.Background = Brushes.Black;

            for (int y = 0; y < rows; y++)
            {
                string line = zeilen[y].TrimEnd('\r', '\n');
                for (int x = 0; x < cols; x++)
                {
                    char ch = x < line.Length ? line[x] : '#';

                    Rectangle cell = new Rectangle();
                    cell.Width = cellSize - 1; // leave a 1px gap for visual separation
                    cell.Height = cellSize - 1;

                    switch (ch)
                    {
                        case '#':
                            cell.Fill = Brushes.DarkSlateGray; // wall
                            break;
                        case 'S':
                            cell.Fill = Brushes.Green; // start
                            break;
                        case 'E':
                            cell.Fill = Brushes.Red; // end
                            break;
                        default:
                            cell.Fill = Brushes.White; // path
                            break;
                    }

                    Canvas.SetLeft(cell, x * cellSize);
                    Canvas.SetTop(cell, y * cellSize);
                    Spielfeld.Children.Add(cell);
                }
            }
        }
    }
}
