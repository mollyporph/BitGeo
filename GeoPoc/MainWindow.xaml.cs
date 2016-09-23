using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
            int padH = 50;
            int padV = 50;
            var geoLookup = new Dictionary<int,GeoSquareData>();
            GeoSquare root = new GeoSquare
            {
                data = new GeoSquareData
                {
                    Id = 1,
                    level = 0,
                    center = new Vector(250, 250),
                    Height = 500,
                    Width = 500
                }
            };
            geoLookup.Add(root.data.Id,root.data);
            BuildTree(root, geoLookup,6);
            int i = 1;
            foreach( var node in geoLookup)
            {
                var rec = new Rectangle
                {
                    Height = node.Value.Height,
                    Width = node.Value.Width

                };

                var varyingColorBrush = new SolidColorBrush(Color.FromArgb(255, 255,0,0));
                rec.StrokeThickness = 0.7;
                rec.Stroke = varyingColorBrush;
                rec.Fill = new SolidColorBrush(Colors.Transparent);

                Canvas.SetTop(rec, node.Value.Y+padV);
                Canvas.SetLeft(rec, node.Value.X+padH);
                mapCanvas.Children.Add(rec);
                var numtext = new TextBlock();
                numtext.Text = node.Value.Id.ToString();
                numtext.Foreground = new SolidColorBrush(Colors.Green);
                Canvas.SetTop(numtext, node.Value.center.Y+padV -7);
                Canvas.SetLeft(numtext, node.Value.center.X + padH -6);
                mapCanvas.Children.Add(numtext);
                i++;
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
                    Id = parent.data.Id << 1 | 0,

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
        public double Hypotenuse => Math.Sqrt(Math.Pow(center.X, 2) + Math.Pow(center.Y, 2));
    }
    public class GeoSquare
    {
        public GeoSquareData data { get; set; }
        public GeoSquare lower { get; set; }
        public GeoSquare higher { get; set; }
    }
}
