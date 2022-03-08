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

namespace miapr3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int PointsCount = 50000;
        private double _pc1;
        private double _pc2;
        private readonly Random _random = new Random();

        private List<Line> redline = new List<Line>();
        private Line greenline;
        private List<Line> blueline = new List<Line>();

        public MainWindow()
        {
            InitializeComponent();
            SliderPC1.ValueChanged += SliderPC1_OnValueChanged;
            SliderPC2.ValueChanged += SliderPC2_OnValueChanged;
            Canva.Children.Clear();
        }

        private void SliderPC1_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderPC2.Value = 1 - SliderPC1.Value;
            ReDrawChart();
        }

        private void SliderPC2_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderPC1.Value = 1 - SliderPC2.Value;
        }

        private void ReDrawChart()
        {
            _pc1 = SliderPC1.Value;
            _pc2 = SliderPC2.Value;
            Calc(Canva.Children);
        }

        private void Calc(UIElementCollection lineList)
        {
            var points1 = new int[PointsCount];
            var points2 = new int[PointsCount];
            double mx1 = 0;
            double mx2 = 0;
            var length = (int)Canva.ActualWidth;
            for (var i = 0; i < PointsCount; i++)
            {
                //Margins
                points1[i] = _random.Next(200, length);
                points2[i] = _random.Next(0, length - 200);
                mx1 += points1[i];
                mx2 += points2[i];
            }

            mx1 /= PointsCount;
            mx2 /= PointsCount;

            double sigma1 = 0;
            double sigma2 = 0;
            for (var i = 0; i < PointsCount; i++)
            {
                sigma1 += Math.Pow(points1[i] - mx1, 2);
                sigma2 += Math.Pow(points2[i] - mx2, 2);
            }

            sigma1 = Math.Sqrt(sigma1 / PointsCount);
            sigma2 = Math.Sqrt(sigma2 / PointsCount);

            var result1 = new double[length];
            var result2 = new double[length];
            result1[0] = (Math.Exp(-0.5 * Math.Pow((-100 - mx1) / sigma1, 2)) /
                (sigma1 * Math.Sqrt(2 * Math.PI)) * _pc1);
            result2[0] =
                (Math.Exp(-0.5 * Math.Pow((-100 - mx2) / sigma2, 2)) /
                    (sigma2 * Math.Sqrt(2 * Math.PI)) * _pc2);

            var d = 0;
            for (int i = redline.Count; i < length; i++)
            {
                //Add red line part
                var rline = new Line()
                {
                    Stroke = Brushes.Blue
                };
                redline.Add(rline);
                var bline = new Line()
                {
                    Stroke = Brushes.Red
                };
                blueline.Add(bline);
                lineList.Add(bline);
                lineList.Add(rline);
            }

            for (var i = 0; i < length; i++)
            {
                result1[i] =
                    (Math.Exp(-0.5 * Math.Pow((i - 100 - mx1) / sigma1, 2)) /
                        (sigma1 * Math.Sqrt(2 * Math.PI)) * _pc1);

                result2[i] =
                    (Math.Exp(-0.5 * Math.Pow((i - 100 - mx2) / sigma2, 2)) /
                        (sigma2 * Math.Sqrt(2 * Math.PI)) * _pc2);

                if (Math.Abs(result1[i] * 500 - result2[i] * 500) < 0.005)
                {
                    d = i;
                }

                if (i == 0)
                    continue;
                //Add red line part
                redline[i].X1 = i - 1;
                redline[i].Y1 = Canva.ActualHeight - (result1[i - 1] * Canva.ActualHeight * 500);
                redline[i].X2 = i;
                redline[i].Y2 = Canva.ActualHeight - (result1[i] * Canva.ActualHeight * 500);
                //Add blue line part
                blueline[i].X1 = i - 1;
                blueline[i].Y1 = Canva.ActualHeight - (result2[i - 1] * Canva.ActualHeight * 500);
                blueline[i].X2 = i;
                blueline[i].Y2 = Canva.ActualHeight - (result2[i] * Canva.ActualHeight * 500);

            }

            var error1 = result2.Take((int)d).Sum();
            var error2 = _pc1 > _pc2 ? result2.Skip((int)d).Sum() : result1.Skip((int)d).Sum();

            if (greenline == null)
            {
                greenline = new Line()
                {
                    X1 = d,
                    Y1 = 0,
                    X2 = d,
                    Y2 = Canva.ActualHeight,
                    Stroke = Brushes.Green
                };
                lineList.Add(greenline);
            }
            else
            {
                greenline.X1 = d;
                greenline.Y1 = 0;
                greenline.X2 = d;
                greenline.Y2 = Canva.ActualHeight;
            }


            TextBoxFalseAlarm.Text = error1.ToString("F20");
            TextBoxMiss.Text = error2.ToString("F20");
            TextBoxAmountOfRisk.Text = (error1 + error2).ToString("F20");
        }
    }
}
