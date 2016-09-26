using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace GeoPoc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random r = new Random();
SetCollection<GeoSquareData> geoRepo = new SetCollection<GeoSquareData>();
        SetCollection<Dot> dots = new SetCollection<Dot>();
        long avgIndex = 0;
        GeoSquareData root = new GeoSquareData
        {
            Id = 1,
            level = 0,
            center = new Vector(380, 350),
            Height = 700,
            Width = 760
        };
        public MainWindow()
        {
            InitializeComponent();
            geoRepo.CollectionChanged += GeoRepo_CollectionChanged;
            dots.CollectionChanged += Dots_CollectionChanged;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            for(int i = 0;i<1000;i++)
            {
                dots.Add(new Dot(r.NextDouble() * 760.0, r.NextDouble() * 700));
            }
        }
        private void Dots_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var dot = dots.Last();
            var watch = Stopwatch.StartNew();
            BuildTreeFromDot(root, geoRepo, dot, 12);
            watch.Stop();
            avgIndex = watch.ElapsedTicks;
            PaintDots(dot);
            BuildDataLabel(dot);
        }

        private void GeoRepo_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PaintTree(geoRepo.Last());

        }
        private void mapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(mapCanvas);
            Dot dot = new Dot(p.X, p.Y);
            dots.Add(dot);


        }
        public void BuildDataLabel(Dot dot)
        {
            var sb = new StringBuilder();
            sb.Append($"Number of dots: {dots.Count}\n");
            sb.Append($"Data for point: {dot.Id}\n\n\n");
            var stopWatches = new List<Stopwatch>();

            for(int i = 0; i<= 12; i++)
            {
                var watch = Stopwatch.StartNew();
                var geoid = dot.geoId >> i;
                var dotCount = dots.Where(x => x.geoId >> i  == geoid).Count()-1;
                watch.Stop();
                stopWatches.Add(watch);
                var square = geoRepo.FirstOrDefault(x => x.Id == geoid);

                sb.Append($"Neighbours in avg {Math.Floor(square.Hypotenuse/2)} distance aways: {dotCount}\n");

            }
            var avg = stopWatches.Average(x => x.ElapsedTicks);
            var ms =  ((double)avg / (double)Stopwatch.Frequency) * 1000;
            var msIndex = ((double)avgIndex / (double)Stopwatch.Frequency) * 1000;
            sb.Append($"\n\nDot index in {Math.Floor(msIndex)} ms ({Math.Floor((double)avgIndex)} ticks)\n\n");
            sb.Append($"Neighbours fetched in avg {Math.Floor(ms)} ms ({Math.Floor(avg)} ticks)");
            Data.Text = sb.ToString();
            
        }
        public void PaintDots(Dot dot)
        {
                var dotCircle = new Ellipse
                {
                    Width = 3.7,
                    Height = 3.7
                };
                dotCircle.Fill = new SolidColorBrush(Colors.Green);
                Canvas.SetLeft(dotCircle, dot.position.X);
                Canvas.SetTop(dotCircle, dot.position.Y);
                mapCanvas.Children.Add(dotCircle);
        }
        public void PaintTree(GeoSquareData node)
        {
                var rec = new Rectangle
                {
                    Height = node.Height,
                    Width = node.Width

                };
                var varyingColorBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                rec.StrokeThickness = 0.7;
                rec.Stroke = varyingColorBrush;
                rec.Fill = new SolidColorBrush(Colors.Transparent);
                Canvas.SetTop(rec, node.Y);
                Canvas.SetLeft(rec, node.X);
                mapCanvas.Children.Add(rec);
        }
        public void BuildTreeFromDot(GeoSquareData currentNode, SetCollection<GeoSquareData> geoRepo, Dot dot, int maxLevel = 5)
        {
            geoRepo.Add(currentNode);
            if (currentNode.level == maxLevel)
                return;

            if(currentNode.level % 2 == 0)
            {
                if(dot.position.Y >= currentNode.center.Y)
                {
                    var Higherleaf = new GeoSquareData
                    {
                        level = currentNode.level + 1,
                        Id = currentNode.Id << 1 | 1,
                        center = new Vector(currentNode.center.X, currentNode.center.Y + (currentNode.Height / 4)),
                        Height = currentNode.Height / 2,
                        Width = currentNode.Width
                    };
                    dot.geoId = Higherleaf.Id;
                    BuildTreeFromDot(Higherleaf, geoRepo, dot, maxLevel);
                }
                else
                {
                    var Lowerleaf = new GeoSquareData
                    {
                        level = currentNode.level + 1,
                        Id = currentNode.Id << 1 | 0,
                        center = new Vector(currentNode.center.X, currentNode.center.Y - (currentNode.Height / 4)),
                        Height = currentNode.Height / 2,
                        Width = currentNode.Width
                    };
                    dot.geoId = Lowerleaf.Id;
                    BuildTreeFromDot(Lowerleaf, geoRepo, dot, maxLevel);
                }
            }
            else
            {
                if (dot.position.X >= currentNode.center.X)
                {
                    var Higherleaf = new GeoSquareData
                    {
                        level = currentNode.level + 1,
                        Id = currentNode.Id << 1 | 1,
                        center = new Vector(currentNode.center.X + (currentNode.Width / 4), currentNode.center.Y),
                        Height = currentNode.Height,
                        Width = currentNode.Width / 2
                    };
                    dot.geoId = Higherleaf.Id;
                    BuildTreeFromDot(Higherleaf, geoRepo, dot, maxLevel);
                }
                else
                {
                    var Lowerleaf = new GeoSquareData
                    {
                        level = currentNode.level + 1,
                        Id = currentNode.Id << 1 | 0,
                        center = new Vector(currentNode.center.X - (currentNode.Width / 4), currentNode.center.Y),
                        Height = currentNode.Height,
                        Width = currentNode.Width / 2
                    };
                    dot.geoId = Lowerleaf.Id;
                    BuildTreeFromDot(Lowerleaf, geoRepo, dot, maxLevel);
                }
            }

        }
        public void BuildTree(GeoSquare parent, Dictionary<int, GeoSquareData> geolookup, int maxLevel = 5)
        {
            if (parent.data.level == maxLevel)
                return;
            int level = parent.data.level + 1;
            parent.lower = new GeoSquare
            {
                data = new GeoSquareData
                {
                    level = level,
                    Id = parent.data.Id << 1 | 0

                }
            };
            parent.higher = new GeoSquare
            {
                data = new GeoSquareData
                {
                    level = level,
                    Id = parent.data.Id << 1 | 1,
                }
            };
            if (level % 2 == 0) // split horizontally
            {

                var pCenter = parent.data.center;
                parent.lower.data.center = new Vector(pCenter.X, pCenter.Y - (parent.data.Height /4));
                parent.higher.data.center = new Vector(pCenter.X, pCenter.Y +(parent.data.Height /4));
                parent.lower.data.Height = parent.data.Height / 2;
                parent.higher.data.Height = parent.data.Height / 2;
                parent.lower.data.Width = parent.data.Width;
                parent.higher.data.Width = parent.data.Width;
            }
            else // split vertically
            {
                var pCenter = parent.data.center;
                parent.lower.data.center = new Vector(pCenter.X -(parent.data.Width /4), pCenter.Y);
                parent.higher.data.center = new Vector(pCenter.X + (parent.data.Width /4), pCenter.Y);
                parent.lower.data.Width = parent.data.Width / 2;
                parent.higher.data.Width = parent.data.Width / 2;
                parent.lower.data.Height = parent.data.Height;
                parent.higher.data.Height = parent.data.Height;
            }
            geolookup.Add(parent.lower.data.Id, parent.lower.data);
            geolookup.Add(parent.higher.data.Id, parent.higher.data);
            BuildTree(parent.lower, geolookup,maxLevel);
            BuildTree(parent.higher, geolookup, maxLevel);
        }

    }

    public class GeoSquareData
    {
        public double X => center.X - Width / 2.0;
        public double Y => center.Y - Height / 2.0;
        public double Width { get; set; }
        public double Height { get; set; }
        public bool isRoot => level == 0;
        public int level { get; set; }
        public Vector center { get; set; }
        public int Id { get; set; }
        public double Hypotenuse => Math.Sqrt(Math.Pow(Height, 2) + Math.Pow(Width, 2));
    }
    public class GeoSquare
    {
        public GeoSquareData data { get; set; }
        public GeoSquare lower { get; set; }
        public GeoSquare higher { get; set; }
    }
    public class Dot
    {
        public Dot(double X, double Y) { this.position = new Vector(X, Y); }
        public Guid Id => Guid.NewGuid();
        public Vector position { get; set; }
        public int geoId { get; set; }

    }
    public class SetCollection<T> : ObservableCollection<T>
    {
        protected override void InsertItem(int index, T item)
        {
            if (Contains(item)) return;

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            int i = IndexOf(item);
            if (i >= 0 && i != index) throw new ArgumentException(nameof(item));

            base.SetItem(index, item);
        }
    }
}


