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
        private Figur figur = null;
        // fields für einfaches bewegliches X
        private string[] zeilen;
        private int rows;
        private int cols;
        private int cellSize = 15;
        private TextBlock centerXBlock;
        private int placeX;
        private int placeY;
        public MainWindow()
        {
            InitializeComponent();
            // try to read a maze file; if not found create a default 20x20 maze
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
            this.rows = zeilen.Length;
            this.cols = zeilen.Max(l => l.TrimEnd('\r', '\n').Length);

            // pixel pro zelle
            Spielfeld.Width = this.cols * this.cellSize;
            Spielfeld.Height = this.rows * this.cellSize;
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

            // noob: jetzt setz ich ein X in die mitte, aber nur auf dem weg (nicht auf einer wand)
            int midX = this.cols / 2;
            int midY = this.rows / 2;

            this.placeX = midX;
            this.placeY = midY;

            // check ob mitte wand ist, wenn ja such nächstes nicht-wand feld (manhattan dist)
            char midChar = '#';
            if (midY < zeilen.Length)
            {
                string midLine = zeilen[midY].TrimEnd('\r', '\n');
                if (midX < midLine.Length) midChar = midLine[midX];
            }

            if (midChar == '#')
            {
                int bestDist = int.MaxValue;
                for (int y = 0; y < rows; y++)
                {
                    string line = zeilen[y].TrimEnd('\r', '\n');
                    for (int x = 0; x < cols; x++)
                    {
                        char c = x < line.Length ? line[x] : '#';
                        if (c != '#')
                        {
                            int d = Math.Abs(x - midX) + Math.Abs(y - midY);
                            if (d < bestDist)
                            {
                                bestDist = d;
                                this.placeX = x;
                                this.placeY = y;
                            }
                        }
                    }
                }
            }
            centerXBlock = new TextBlock();
            centerXBlock.Text = "X";
            centerXBlock.FontWeight = FontWeights.Bold;
            centerXBlock.Foreground = Brushes.Blue;
            centerXBlock.FontSize = Math.Max(10, this.cellSize - 2);
            Spielfeld.Children.Add(centerXBlock);
            Canvas.SetLeft(centerXBlock, this.placeX * this.cellSize + 1);
            Canvas.SetTop(centerXBlock, this.placeY * this.cellSize + 0);

            // fokus setzen damit tasten ankommen
            this.Loaded += (s, e) => { Keyboard.Focus(this); };
            this.KeyDown += MainWindow_KeyDown;
        }

        // WASD steuerung: W=oben, A=links, S=unten, D=rechts (noob style)
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            int nx = this.placeX;
            int ny = this.placeY;

            switch (e.Key)
            {
                case Key.W: ny = this.placeY - 1; break; // hoch
                case Key.S: ny = this.placeY + 1; break; // runter
                case Key.A: nx = this.placeX - 1; break; // links
                case Key.D: nx = this.placeX + 1; break; // rechts
                default: return; // andere tasten ignorieren
            }

            // check bounds
            if (ny < 0 || ny >= this.rows || nx < 0 || nx >= this.cols) return;

            string line = this.zeilen[ny].TrimEnd('\r', '\n');
            if (nx >= line.Length) return; // außerhalb string -> wand
            if (line[nx] == '#') return; // wand

            // move
            this.placeX = nx;
            this.placeY = ny;
            Canvas.SetLeft(this.centerXBlock, this.placeX * this.cellSize + 1);
            Canvas.SetTop(this.centerXBlock, this.placeY * this.cellSize + 0);
        }
    }
}
