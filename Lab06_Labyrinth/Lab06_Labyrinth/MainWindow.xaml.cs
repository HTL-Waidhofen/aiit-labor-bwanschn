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
        // Datei: MainWindow.xaml.cs
        // Diese Datei erzeugt das Fenster, lädt das Labyrinth aus einer Datei
        // (oder erzeugt ein Standard-Labyrinth) und zeigt es auf einem Canvas.
        // Hauptaufgaben:
        // - Labyrinth zeichnen (Wände, Start, Ziel)
        // - Spielfigur erstellen und bewegen (W/A/S/D)
        // - Sammelobjekte (Schnitzelsemmeln) platzieren und einsammeln
        // - Zwei Gegner platzieren (enemy1/enemy2) und Kollision prüfen
        // - Viewport (Ausschnitt) zentrieren, damit die Figur immer sichtbar bleibt

        // Wir speichern die Zeilen vom Labyrinth (Text aus maze_20x20.txt)
        private string[] zeilen;
        // Anzahl Reihen (rows) und Spalten (cols) im Gitter
        private int rows, cols;
        // Größe einer Zelle in Pixeln (Basisgröße, vor Skalierung/Zoom)
        private int cellSize = 15;
        // Das Labyrinth: jede Zeile ist ein String mit Zeichen wie '#' '.' 'S' 'E'
        // Anzahl der Reihen (rows) und Spalten (cols) im Labyrinth
        // Pixelgröße eines Feldes auf dem Canvas (wie groß wird ein Zellen-Rechteck)

        // Wand-Klasse: ganz einfach, hat Position und ein Rechteck (Visual)
        private class Wand { public int X, Y; public Rectangle R; }
        // Schnitzelsemmel-Klasse: ein Gegenstand den man einsammeln kann
        private class Goodie { public int X, Y; public UIElement Visual; }
        // Gegner-Klasse: zwei Gegner, können Bild oder Form haben
        private class Enemy { public int X, Y; public UIElement Visual; public string Id; }
        // Liste aller Wände (nur logische Speicherung, aber wir fügen auch das Rechteck dem Canvas hinzu)
        private List<Wand> waende = new List<Wand>();
        // Liste mit Schnitzelsemmeln (Goodies) - logische Sammlung und zum Entfernen
        private List<Goodie> goodies = new List<Goodie>();
        // Wie viele Goodies ursprünglich platziert wurden
        private int initialGoodies = 0;
        // Zufallsgenerator für zufällige Spawn-Positionen
        private Random rnd = new Random();
        // Wie viele Schnitzelsemmel wurden bisher eingesammelt
        private int collectedCount = 0;
        // Referenz auf das TextBlock oben rechts, das die Anzahl zeigt
        private TextBlock goodieCounterText;
        // Gegner-Liste (zwei Gegner werden erstellt)
        private List<Enemy> enemies = new List<Enemy>();
        // Startposition der Spielfigur merken (zum Zurücksetzen nach Tod)
        private int startX = 0, startY = 0;

        // Spielfigur als Objekt (unsere Figur-Klasse)
        private Figur figur = null;
        public MainWindow()
        {
            InitializeComponent();
            // Initialwerte setzen
            initialGoodies = 0; // Anzahl Goodies zu Beginn

            // Zoom einstellen: Canvas wird skaliert, sodass Figuren größer angezeigt werden
            // Ändere den Wert für stärkeren/schwächeren Zoom (z.B. 2.0 = 200%)
            double zoom = 1.8;
            Spielfeld.LayoutTransform = new ScaleTransform(zoom, zoom);
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
            // Setze die Canvas-Größe so, dass das Labyrinth komplett drauf passt.
            // Der ScrollViewer (Viewport) zeigt davon fast den ganzen Bildschirm.
            Spielfeld.Width = cols * cellSize;
            Spielfeld.Height = rows * cellSize;
            Spielfeld.Background = Brushes.Black;

            // Jede Zelle wird als Rechteck auf das Canvas gemalt
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

                    // *** Beginn Wand-Erstellung ***
                    // Wenn das Zeichen '#' ist, dann ist das hier eine Mauer.
                    // Wir machen zwei Dinge:
                    // 1) Wir malen das Rechteck (oben passiert schon die Farbwahl),
                    // 2) Wir speichern eine logische Wand-Information in der Liste 'waende'.
                    //
                    // Warum speichern wir eine Wand? Damit das Programm weiß, welche
                    // Felder nicht begehbar sind. Beim Bewegen prüfen wir später die
                    // Zeichen in 'zeilen' (oder die Wand-Liste) und verhindern so,
                    // dass die Spielfigur in eine Mauer läuft.
                    //
                    // Schritt für Schritt:
                    // - 'ch == '#'' bedeutet: dieses Feld ist eine Mauer.
                    // - Wir bauen ein Wand-Objekt mit den Gitter-Koordinaten X/Y
                    //   (also Spalte x und Reihe y) und geben ihm das Rechteck R,
                    //   das wir gerade gezeichnet haben. R ist nur für die Ansicht.
                    // - Wir fügen das Wand-Objekt zur Liste 'waende' hinzu. Diese
                    //   Liste ist unsere logische Sammlung aller Mauern.
                    //
                    // Wenn wir später die Figur bewegen, fragen wir: "Ist da eine
                    // Wand?" Wenn ja, dann lassen wir die Figur nicht dorthin gehen.
                    if (ch == '#')
                    {
                        var w = new Wand() { X = x, Y = y, R = cell };
                        waende.Add(w);
                    }
                    // *** Ende Wand-Erstellung ***
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
            // ------------------ GENAUE ERKLÄRUNG: Schnitzelsemmeln erzeugen ------------------
            // Dieser Block erstellt die Sammel-Objekte ("Schnitzelsemmel").
            // Schritt-für-Schritt Erklärung (ganz genau, damit du weißt was passiert):
            // 1) Wir durchsuchen jede Zeile im Array 'zeilen' (das ist der Text vom Maze).
            //    Wenn an einer Position das Zeichen 'G' steht, gilt das als Vorgabe:
            //    Dort soll eine Schnitzelsemmel erscheinen.
            // 2) Wir prüfen, dass das Feld kein '#' (Mauer) ist. Schnitzelsemmeln
            //    dürfen nie in einer Wand erscheinen. Daher nur wenn Feld != '#'.
            // 3) Für jede gefundene Position rufen wir `CreateGoodieAt(x,y)` auf.
            //    Diese Hilfsfunktion macht folgendes:
            //      a) Sie versucht, eine Bilddatei 'goodie.png' im Programm-Ordner
            //         zu laden. Wenn du ein eigenes Bild benutzen willst, lege
            //         die Datei 'goodie.png' in das Ausgabeverzeichnis (z.B. bin\\Debug).
            //      b) Wenn das Bild existiert und geladen werden kann, wird ein
            //         `Image`-Element erstellt und als Visual verwendet.
            //      c) Wenn kein Bild vorhanden ist oder Laden fehlschlägt,
            //         benutzt die Funktion ein gelbes Rechteck als Fallback.
            //      d) Das Visual wird an die richtige pixel-Position gesetzt
            //         (Canvas.SetLeft/SetTop mit cellSize Multiplikator).
            //      e) Die Funktion gibt ein Goodie-Objekt zurück mit X/Y und Visual.
            // 4) Wir fügen das Visual dem Canvas hinzu und speichern das Goodie in
            //    der Liste `goodies` (logische Sammlung aller Schnitzelsemmeln).
            // 5) Wenn in der Maze-Datei überhaupt kein 'G' drinsteht, dann legen
            //    wir eine Anzahl (default 5) Schnitzelsemmeln zufällig auf freien
            //    Feldern (Felder, die kein '#') an. Dabei:
            //      - Wir sammeln alle freien Positionen in eine Liste `freePositions`.
            //      - Wir wählen zufällig Positionen aus dieser Liste (ohne Zurücklegen)
            //        und erzeugen dort die Goodies.
            // 6) `initialGoodies` merkt sich, wie viele zu Beginn existierten.
            //    `collectedCount` zählt wie viele du bereits eingesammelt hast.
            // 7) Beim Bewegen (in KeyDown) prüfen wir nach jeder Bewegung, ob
            //    an der neuen Figur-Position ein Goodie liegt. Wenn ja:
            //      - Entfernen wir das Visual vom Canvas
            //      - Entfernen das Goodie aus der Liste
            //      - Erhöhen `collectedCount` und rufen `UpdateGoodieCounter()` auf
            // 8) Wenn alle Goodies eingesammelt wurden (goodies.Count == 0),
            //    ruft das Programm `RespawnGoodiesRandomly()` auf, welches neue
            //    Schnitzelsemmeln an zufälligen, freien Positionen erzeugt.
            //
            // Hinweise für eigene Bilder:
            // - Lege die Datei 'goodie.png' in dein Ausgabeverzeichnis (z.B.
            //   bin\\Debug\\ oder bin\\Release\\) damit das Programm sie
            //   automatisch findet. Alternativ kann der Code so erweitert werden,
            //   dass ein Dateiauswahldialog verwendet wird.
            // - Dateiformat: PNG funktioniert gut, andere Bildtypen sind auch OK.
            // - Die Größe des Bildes wird auf cellSize-2 skaliert, damit es in
            //   eine Zelle passt.
            // --------------------------------------------------------------------------------
            // Schnitzelsemmeln erzeugen: suche nach 'G' in der Datei
            bool foundAnyG = false;
            for (int y = 0; y < rows; y++)
            {
                string line = zeilen[y].TrimEnd('\r', '\n');
                for (int x = 0; x < cols; x++)
                {
                    char ch = x < line.Length ? line[x] : '#';
                    if (ch == 'G' && ch != '#')
                    {
                        var g = CreateGoodieAt(x, y);
                        goodies.Add(g);
                        Spielfeld.Children.Add(g.Visual);
                        foundAnyG = true;
                    }
                }
            }

            if (!foundAnyG)
            {
                // Wenn keine 'G' in der Datei: platziere mehrere Schnitzelsemmeln an
                // zufälligen freien Positionen. Anzahl default = 5.
                int toPlace = 5;
                var freePositions = new List<Tuple<int, int>>();
                for (int y = 0; y < rows; y++)
                {
                    string line = zeilen[y].TrimEnd('\r', '\n');
                    for (int x = 0; x < cols; x++)
                    {
                        char c = x < line.Length ? line[x] : '#';
                        if (c != '#') freePositions.Add(Tuple.Create(x, y));
                    }
                }
                // zufällig verteilen
                for (int i = 0; i < toPlace && freePositions.Count > 0; i++)
                {
                    int idx = rnd.Next(freePositions.Count);
                    var pos = freePositions[idx];
                    freePositions.RemoveAt(idx);
                    var g = CreateGoodieAt(pos.Item1, pos.Item2);
                    goodies.Add(g);
                    Spielfeld.Children.Add(g.Visual);
                }
            }
            initialGoodies = goodies.Count;
            collectedCount = 0;

            // Merke Startposition der Figur (wird zum Zurücksetzen nach Tod benutzt)
            startX = figur.X; startY = figur.Y;

            // Gegner erzeugen: suche nach '1' und '2' in der Datei, sonst zufällig
            bool found1 = false, found2 = false;
            for (int y = 0; y < rows; y++)
            {
                string line = zeilen[y].TrimEnd('\r', '\n');
                for (int x = 0; x < cols; x++)
                {
                    if (x >= line.Length) continue;
                    if (line[x] == '1') { var e = CreateEnemyAt(x, y, "enemy1.png", "E1"); enemies.Add(e); Spielfeld.Children.Add(e.Visual); found1 = true; }
                    if (line[x] == '2') { var e = CreateEnemyAt(x, y, "enemy2.png", "E2"); enemies.Add(e); Spielfeld.Children.Add(e.Visual); found2 = true; }
                }
            }
            if (!found1) // spawn random free pos
            {
                var free = GetFreePositions();
                if (free.Count > 0)
                {
                    var p = free[rnd.Next(free.Count)];
                    var e = CreateEnemyAt(p.Item1, p.Item2, "enemy1.png", "E1"); enemies.Add(e); Spielfeld.Children.Add(e.Visual);
                }
            }
            if (!found2)
            {
                var free = GetFreePositions();
                if (free.Count > 0)
                {
                    var p = free[rnd.Next(free.Count)];
                    var e = CreateEnemyAt(p.Item1, p.Item2, "enemy2.png", "E2"); enemies.Add(e); Spielfeld.Children.Add(e.Visual);
                }
            }

            // Fokussieren, damit KeyDown funktioniert
            this.Loaded += (s, e) => { Keyboard.Focus(this); };
            // KeyDown ist im Code-behind (hier), XAML-Ereignis optional
            this.KeyDown += MainWindow_KeyDown;

            // Goodie counter TextBlock (aus XAML) referenzieren
            goodieCounterText = this.FindName("GoodieCounter") as TextBlock;
            UpdateGoodieCounter();
        }

        /*
         Ganz genaue Erklärung der Variablen und was beim Drücken einer Taste passiert:

         - "figur.X" / "figur.Y": Das ist die aktuelle Stelle der Spielfigur im
            Gitter. X = Spalte, Y = Reihe.

         - "nx" / "ny": Das sind die NEUEN Koordinaten, die wir zuerst nur
            berechnen. Beispiel: Wenn die Figur rechts gehen soll, setzen wir
            nx = figur.X + 1 und ny = figur.Y. Wir ändern also zuerst nx/ny,
            ohne die Figur sofort zu verschieben.

         - Warum zuerst nx/ny? Damit wir prüfen können, ob die Bewegung erlaubt
            ist (z.B. nicht außerhalb des Feldes oder in eine Mauer).

         - Prüfungen nach der Berechnung von nx/ny:
            1) Ist nx/ny innerhalb des Labyrinths? (Nicht kleiner als 0 und
               nicht größer/gleich cols bzw. rows.)
            2) Ist an zeilen[ny][nx] eine Mauer ('#')? Wenn ja, dann darf die
               Figur nicht dorthin. Dann machen wir nichts.

         - Wenn beide Prüfungen bestanden sind, dann wird die Figur wirklich
           nach nx/ny bewegt: figur.MoveTo(nx, ny) und die Anzeige (Visual)
           wird auf dem Canvas aktualisiert.

         Kurz: nx/ny sind nur temporäre Wunsch-Koordinaten. Nach Prüfung
         übernehmen wir sie in die echte Position der Figur.
        */
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

            // Nach Bewegung: zentriere das Viewport auf die Figur
            CenterViewportOnPlayer();

            // Prüfe ob auf der neuen Position ein Goodie liegt
            var collected = goodies.FirstOrDefault(g => g.X == nx && g.Y == ny);
            if (collected != null)
            {
                // Entferne das sichtbare Element vom Canvas und aus der Liste
                Spielfeld.Children.Remove(collected.Visual);
                goodies.Remove(collected);
                // Erhöhe eingesammelte Anzahl
                collectedCount++;
                // Update Zähler
                UpdateGoodieCounter();

                // Wenn alle eingesammelt sind: neu spawnen an anderen Orten
                if (goodies.Count == 0)
                {
                    RespawnGoodiesRandomly();
                }
            }

            // Prüfe Kollision mit Gegnern
            var hit = enemies.FirstOrDefault(ev => ev.X == nx && ev.Y == ny);
            if (hit != null)
            {
                // Spieler wurde gefressen
                // Zeige spezielle Nachricht je nachdem welcher Gegner es war
                if (hit.Id == "E2") MessageBox.Show("Du einhorn wüühhaa", "Gefressen");
                else if (hit.Id == "E1") MessageBox.Show("du gehst mir richtig auf den sack", "Gefressen");
                // reset player to start (zurück zur Startposition)
                figur.MoveTo(startX, startY);
                figur.SetPositionOnCanvas(Spielfeld, cellSize);
                CenterViewportOnPlayer();
            }

            // Nachdem der Spieler gezogen hat, bewegen sich die Gegner auch.
            // Dadurch entsteht ein einfaches Gegnerverhalten. Danach prüfen
            // wir erneut, ob ein Gegner auf den Spieler gelaufen ist.
            MoveEnemiesAfterPlayer();
        }

        // Prüft ob Feld (x,y) begehbar ist (nicht außerhalb und kein '#')
        private bool IsWalkable(int x, int y)
        {
            if (x < 0 || x >= cols || y < 0 || y >= rows) return false;
            string line = zeilen[y].TrimEnd('\r', '\n');
            if (x >= line.Length) return false;
            return line[x] != '#';
        }

        // Bewegt die Gegner nach dem Spielerzug. E1 bewegt sich zufällig,
        // E2 verfolgt den Spieler (einfacher Greedy-Schritt). Gegner dürfen
        // nicht durch Wände laufen und vermeiden (so gut es geht) andere Gegner.
        private void MoveEnemiesAfterPlayer()
        {
            var occupied = new HashSet<(int, int)>();
            // Markiere aktuelle Gegner-Positionen
            foreach (var en in enemies) occupied.Add((en.X, en.Y));

            foreach (var en in enemies)
            {
                int ex = en.X, ey = en.Y;
                int nx = ex, ny = ey;

                if (en.Id == "E1")
                {
                    // E1: zufälliger Schritt
                    var dirs = new List<(int dx, int dy)>{(0,-1),(0,1),(-1,0),(1,0)};
                    // shuffle simple
                    for (int i = dirs.Count-1; i > 0; i--) { var j = rnd.Next(i+1); var t = dirs[i]; dirs[i] = dirs[j]; dirs[j] = t; }
                    foreach (var d in dirs)
                    {
                        int tx = ex + d.dx, ty = ey + d.dy;
                        if (!IsWalkable(tx, ty)) continue;
                        if (occupied.Contains((tx, ty)) && !(tx == figur.X && ty == figur.Y)) continue;
                        nx = tx; ny = ty; break;
                    }
                }
                else
                {
                    // E2: einfache Verfolgung zum Spieler (Greedy)
                    int dx = figur.X - ex; int dy = figur.Y - ey;
                    (int nxc, int nyc) = (ex, ey);
                    if (Math.Abs(dx) > Math.Abs(dy))
                    {
                        int stepX = dx > 0 ? 1 : -1;
                        if (IsWalkable(ex + stepX, ey) && !occupied.Contains((ex + stepX, ey))) { nxc = ex + stepX; nyc = ey; }
                        else if (IsWalkable(ex, ey + (dy > 0 ? 1 : -1)) && !occupied.Contains((ex, ey + (dy > 0 ? 1 : -1)))) { nxc = ex; nyc = ey + (dy > 0 ? 1 : -1); }
                    }
                    else
                    {
                        int stepY = dy > 0 ? 1 : -1;
                        if (IsWalkable(ex, ey + stepY) && !occupied.Contains((ex, ey + stepY))) { nxc = ex; nyc = ey + stepY; }
                        else if (IsWalkable(ex + (dx > 0 ? 1 : -1), ey) && !occupied.Contains((ex + (dx > 0 ? 1 : -1), ey))) { nxc = ex + (dx > 0 ? 1 : -1); nyc = ey; }
                    }
                    nx = nxc; ny = nyc;
                }

                // Update occupied positions
                occupied.Remove((ex, ey));
                occupied.Add((nx, ny));

                // Setze neue Koordinaten und verschiebe Visual
                en.X = nx; en.Y = ny;
                Canvas.SetLeft(en.Visual, nx * cellSize + 1);
                Canvas.SetTop(en.Visual, ny * cellSize + 0);
            }

            // Nach der Bewegung prüfen wir, ob ein Gegner nun auf dem Spieler steht
            var hit = enemies.FirstOrDefault(ev => ev.X == figur.X && ev.Y == figur.Y);
            if (hit != null)
            {
                if (hit.Id == "E2") MessageBox.Show("Du einhorn wüühhaa", "Gefressen");
                else if (hit.Id == "E1") MessageBox.Show("du gehst mir richtig auf den sack", "Gefressen");
                // reset player
                figur.MoveTo(startX, startY);
                figur.SetPositionOnCanvas(Spielfeld, cellSize);
                CenterViewportOnPlayer();
            }
        }

        // Gibt alle freien (nicht-wand) Positionen zurück
        private List<Tuple<int,int>> GetFreePositions()
        {
            var res = new List<Tuple<int,int>>();
            for (int y = 0; y < rows; y++)
            {
                string line = zeilen[y].TrimEnd('\r','\n');
                for (int x = 0; x < cols; x++)
                {
                    char c = x < line.Length ? line[x] : '#';
                    if (c != '#') res.Add(Tuple.Create(x,y));
                }
            }
            return res;
        }

        // Erzeugt einen Gegner an Position x,y. Lädt 'fileName' wenn vorhanden.
        private Enemy CreateEnemyAt(int x, int y, string fileName, string id)
        {
            UIElement vis;
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(fullPath))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit(); bmp.UriSource = new Uri(fullPath, UriKind.Absolute); bmp.CacheOption = BitmapCacheOption.OnLoad; bmp.EndInit();
                    var img = new Image(); img.Source = bmp; img.Width = cellSize - 2; img.Height = cellSize - 2; vis = img;
                }
                catch { var r = new Ellipse(){ Width=cellSize-2, Height=cellSize-2, Fill=Brushes.Purple}; vis = r; }
            }
            else { var r = new Ellipse(){ Width=cellSize-2, Height=cellSize-2, Fill=Brushes.Purple}; vis = r; }
            Canvas.SetLeft(vis, x * cellSize + 1); Canvas.SetTop(vis, y * cellSize + 0);
            return new Enemy(){ X = x, Y = y, Visual = vis, Id = id };
        }

        // Wenn alle Schnitzelsemmeln eingesammelt sind, spawne neue an zufälligen Orten
        private void RespawnGoodiesRandomly()
        {
            // Entferne alte Visuals (falls noch Reste)
            foreach (var g in goodies) if (g.Visual != null) Spielfeld.Children.Remove(g.Visual);
            goodies.Clear();
            // Platziere neue (5) an zufälligen freien Positionen
            int toPlace = 5;
            var freePositions = new List<Tuple<int, int>>();
            for (int y = 0; y < rows; y++)
            {
                string line = zeilen[y].TrimEnd('\r', '\n');
                for (int x = 0; x < cols; x++)
                {
                    char c = x < line.Length ? line[x] : '#';
                    if (c != '#') freePositions.Add(Tuple.Create(x, y));
                }
            }
            for (int i = 0; i < toPlace && freePositions.Count > 0; i++)
            {
                int idx = rnd.Next(freePositions.Count);
                var pos = freePositions[idx];
                freePositions.RemoveAt(idx);
                var g = CreateGoodieAt(pos.Item1, pos.Item2);
                goodies.Add(g);
                Spielfeld.Children.Add(g.Visual);
            }
            // Reset initial count and update UI
            initialGoodies = goodies.Count;
            UpdateGoodieCounter();

            // Wenn neue goodies erstellt wurden, evtl. mitte auf Spieler
            CenterViewportOnPlayer();
        }

        // Zentriert den ScrollViewer so, dass die Figur in der Mitte des Viewports bleibt
        private void CenterViewportOnPlayer()
        {
            try
            {
                // Name des ScrollViewers: Viewport (in XAML)
                var sv = this.FindName("Viewport") as ScrollViewer;
                if (sv == null || figur == null) return;

                // Berechne pixel-position der Figur (vor Transform)
                double px = figur.X * cellSize;
                double py = figur.Y * cellSize;

                // Wenn LayoutTransform (Scale) aktiv ist, berücksichtige Skalierung
                double scaleX = 1.0, scaleY = 1.0;
                if (Spielfeld.LayoutTransform is ScaleTransform st)
                {
                    scaleX = st.ScaleX; scaleY = st.ScaleY;
                }

                // Zielpunkt (in gescalten Pixeln) = center des Viewports
                double targetX = px * scaleX - (sv.ViewportWidth / 2) + (cellSize * scaleX / 2);
                double targetY = py * scaleY - (sv.ViewportHeight / 2) + (cellSize * scaleY / 2);

                if (targetX < 0) targetX = 0;
                if (targetY < 0) targetY = 0;

                sv.ScrollToHorizontalOffset(targetX);
                sv.ScrollToVerticalOffset(targetY);
            }
            catch
            {
                // ignore errors silently for simplicity
            }
        }

        // ********** G O O D I E - H I L F S M E T H O D E N **********
        // Erzeugt ein Goodie an Gitterposition x,y und gibt das Objekt zurück.
        // Wenn eine Datei 'goodie.png' im Programmordner liegt, wird dieses Bild
        // verwendet. Sonst wird ein gelbes Rechteck angezeigt.
        // Hinweis: Lege die Datei 'goodie.png' in das Ausgabeverzeichnis (z.B.
        // bin\Debug) damit sie beim Start gefunden wird.
        private Goodie CreateGoodieAt(int x, int y)
        {
            UIElement vis;
            string imgFile = "goodie.png";
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imgFile);
            if (File.Exists(fullPath))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit(); bmp.UriSource = new Uri(fullPath, UriKind.Absolute); bmp.CacheOption = BitmapCacheOption.OnLoad; bmp.EndInit();
                    var img = new Image(); img.Source = bmp; img.Width = cellSize - 2; img.Height = cellSize - 2; vis = img;
                }
                catch
                {
                    var r = new Rectangle(); r.Width = cellSize - 2; r.Height = cellSize - 2; r.Fill = Brushes.Yellow; vis = r;
                }
            }
            else
            {
                var r = new Rectangle(); r.Width = cellSize - 2; r.Height = cellSize - 2; r.Fill = Brushes.Yellow; vis = r;
                vis = r;
            }

            Canvas.SetLeft(vis, x * cellSize + 1); Canvas.SetTop(vis, y * cellSize + 0);
            return new Goodie { X = x, Y = y, Visual = vis };
        }

        // Aktualisiert den Zähler oben rechts. Zeigt an, wie viele Goodies der
        // Spieler bereits eingesammelt hat (gesammelt = initial - aktuell).
        private void UpdateGoodieCounter()
        {
            if (goodieCounterText == null) goodieCounterText = this.FindName("GoodieCounter") as TextBlock;
            if (goodieCounterText != null)
            {
                int collected = initialGoodies - goodies.Count;
                if (collected < 0) collected = 0;
                goodieCounterText.Text = $"Schnitzelsemmel: {collected}";
            }
        }
    }
}
