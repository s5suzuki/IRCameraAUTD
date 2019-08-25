using LibirimagerSharp;
using Microsoft.Win32;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PI450Viewer
{
    public partial class MainWindow : Window
    {
        private static readonly int ImageWidth = (int)(double)App.Current.Resources["Width"];
        private static readonly int ImageHeight = (int)(double)App.Current.Resources["Height"];

        private IRDirectInterface _irDirectInterface;
        private readonly bool _grabImages = true;
        private double _margin;

        private ThermalPlotViewModel _data;
        private int _mesureDataNum = 0;
        private bool _mesureing = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeThermoCamera();
            InitializeParameters();

            ClearMargin(PlotX, OxyPlot.Axes.AxisPosition.Right);
            ClearMargin(PlotY, OxyPlot.Axes.AxisPosition.Left);

            foreach (string palleteOption in Enum.GetNames(typeof(OptrisColoringPalette)))
            {
                MenuItem item = new MenuItem() { Header = palleteOption, IsCheckable = true };
                if (palleteOption == "Iron")
                {
                    item.IsChecked = true;
                }

                PalleteOption.Items.Add(item);
            }
            foreach (string scalingOption in Enum.GetNames(typeof(OptrisPaletteScalingMethod)))
            {
                MenuItem item = new MenuItem() { Header = scalingOption, IsCheckable = true };
                if (scalingOption == "MinMax")
                {
                    item.IsChecked = true;
                }

                if (scalingOption == "Manual")
                {
                    item.IsEnabled = false;
                }

                ScalingOption.Items.Add(item);
            }
        }

        private static void ClearMargin(Plot plot, OxyPlot.Axes.AxisPosition pos)
        {
            plot.PlotMargins = new Thickness(0, 0, 0, 20);
            plot.Padding = new Thickness(0, 0, 0, 20);
            plot.TitleFontSize = 0;
            plot.SubtitleFontSize = 0;
            plot.TitlePadding = 0;
            plot.BorderThickness = new Thickness(0, 0, 0, 0);
            plot.PlotAreaBorderThickness = new Thickness(0, 0, 0, 0);

            plot.Axes.Clear();
            plot.UpdateLayout();

            plot.Axes.Add(new LinearAxis() { IsAxisVisible = true, Position = pos, MinimumRange = 0.0, AbsoluteMinimum = 0, Minimum = 0.0 });
            plot.Axes.Add(new LinearAxis() { IsAxisVisible = false, Position = OxyPlot.Axes.AxisPosition.Bottom });
        }

        private void InitializeParameters()
        {
            _data = new ThermalPlotViewModel();
            DataContext = _data;

            _margin = (double)App.Current.Resources["DragMargin"];
            _data.ViewX = (int)(double)App.Current.Resources["InitX"];
            _data.ViewY = (int)(double)App.Current.Resources["InitY"];
        }

        public void InitializeThermoCamera()
        {
            try
            {
                _irDirectInterface = IRDirectInterface.Instance;
                _irDirectInterface.Connect("generic.xml");
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
                Close();
            }

            Task.Run(() =>
            {
                ImageGrabberMethode();
            });

        }

        private void ImageGrabberMethode()
        {
            while (_grabImages)
            {
                try
                {
                    ThermalPaletteImage images = _irDirectInterface.ThermalPaletteImage;

                    if (_mesureing)
                    {
                        Task task = Task.Run(() =>
                        {
                            SaveData(images.ThermalImage);
                        });
                        _tasks.Add(task);
                        _mesureDataNum++;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ThermalImage.Source = images.PaletteImage;
                        _data.ThermalData = images.ThermalImage;
                        PlotX.InvalidatePlot(true);
                        PlotY.InvalidatePlot(true);
                    });

                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message, "ERROR");
                }
            }
        }

        private readonly object _lock = new object();
        private readonly List<Task> _tasks = new List<Task>();

        private void SaveData(ushort[,] data)
        {

            long now = DateTime.Now.Ticks;
            lock (_lock)
            {
                while (File.Exists("temp\\" + now + ".bin"))
                {
                    now++;
                }
            }

            using (FileStream fs = new FileStream("temp\\" + now + ".bin", FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, data);
            }
        }

        private static void FormatToCSV(StreamWriter sw, string file)
        {
            using (FileStream fis = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter bf = new BinaryFormatter();
                ushort[,] data = bf.Deserialize(fis) as ushort[,];

                sw.Write(file.Split('.', '\\')[1]);

                for (int i = 0; i < data.GetLength(0); i++)
                {
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        sw.Write("," + ThermalPlotViewModel.ConvertToTemp(data[i, j]));
                    }

                    sw.Write(", ");
                }

                sw.WriteLine();
            }
        }

        private void Thumb_DragDelta_Horizontal(object sender, DragDeltaEventArgs e)
        {
            Thumb thumb = e.Source as Thumb;
            int y = (int)Clamp(Canvas.GetTop(thumb) + e.VerticalChange + _margin, 0, ImageHeight - 1);
            Canvas.SetTop(thumb, y - _margin);

            _data.ViewY = y;
            PlotX.InvalidatePlot(true);
        }

        private void Thumb_DragDelta_Vertical(object sender, DragDeltaEventArgs e)
        {
            Thumb thumb = e.Source as Thumb;
            int x = (int)Clamp(Canvas.GetLeft(thumb) + e.HorizontalChange + _margin, 0, ImageWidth - 1);

            Canvas.SetLeft(thumb, x - _margin);

            _data.ViewX = x;
            PlotY.InvalidatePlot(true);
        }

        private static double Clamp(double value, double min, double max)
        {
            return value < min ? min : max < value ? max : value;
        }

        private void ThermalImage_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ThemalAtCursor.Visibility = Visibility.Visible;
        }

        private void ThermalImage_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ThemalAtCursor.Visibility = Visibility.Hidden;
        }

        private void ThermalImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point p = e.GetPosition(ThermalImage);
            Canvas.SetLeft(ThemalAtCursor, p.X + 5);
            Canvas.SetTop(ThemalAtCursor, p.Y - 10);

            int x = (int)Clamp(p.X, 0, ImageWidth - 1);
            int y = (int)Clamp(p.Y, 0, ImageHeight - 1);

            ThemalAtCursor.Text = ThermalPlotViewModel.ConvertToTemp(_data.ThermalData[y, x]).ToString("00.0", CultureInfo.InvariantCulture) + "℃";
        }

        private void PalleteOption_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem check)
            {
                if (check.IsChecked)
                {
                    foreach (MenuItem item in PalleteOption.Items)
                    {
                        if (item != check)
                        {
                            item.IsChecked = false;
                        }
                    }

                    OptrisPaletteScalingMethod scaling = OptrisPaletteScalingMethod.None;
                    foreach (MenuItem item in ScalingOption.Items)
                    {
                        if (item.IsChecked)
                        {
                            scaling = (OptrisPaletteScalingMethod)Enum.Parse(typeof(OptrisPaletteScalingMethod), item.Header.ToString());
                        }
                    }

                    if (Enum.TryParse(check.Header.ToString(), out OptrisColoringPalette value))
                    {
                        _irDirectInterface.SetPaletteFormat(value, scaling);
                    }
                }
                else
                {
                    check.IsChecked = true;
                }
            }
        }

        private void ScalingOption_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem check)
            {
                if (check.IsChecked)
                {
                    OptrisColoringPalette pallete = OptrisColoringPalette.None;
                    foreach (MenuItem item in PalleteOption.Items)
                    {
                        if (item.IsChecked)
                        {
                            pallete = (OptrisColoringPalette)Enum.Parse(typeof(OptrisColoringPalette), item.Header.ToString());
                        }
                    }

                    foreach (MenuItem item in ScalingOption.Items)
                    {
                        if (item != check)
                        {
                            item.IsChecked = false;
                        }
                    }

                    if (Enum.TryParse(check.Header.ToString(), out OptrisPaletteScalingMethod value))
                    {
                        _irDirectInterface.SetPaletteFormat(pallete, value);
                    }
                }
                else
                {
                    check.IsChecked = true;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = "data.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "CSVファイル(*.csv)|*.csv",
                FilterIndex = 1,
                Title = "保存先のファイルを選択してください",
                RestoreDirectory = true,
                OverwritePrompt = true,
                CheckPathExists = true
            };

            if (sfd.ShowDialog() == true)
            {
                _data.Path = sfd.FileName;
            }
        }

        private void Button_Click_Finish(object sender, RoutedEventArgs e)
        {
            _mesureing = false;
            DEBUG.Text = "Stop Mesure.";

            if (!Directory.Exists("temp"))
            {
                return;
            }

            Task.Run(async () =>
            {
                await Task.WhenAll(_tasks);
                _tasks.Clear();

                using (StreamWriter sw = new StreamWriter(_data.Path))
                {
                    int c = 0;
                    foreach (string file in Directory.EnumerateFiles("temp"))
                    {
                        FormatToCSV(sw, file);
                        Dispatcher.Invoke(() => DEBUG.Text = $"Write to CSV {++c}/{_mesureDataNum}");
                    }
                }
                //if (Directory.Exists("temp")) Directory.Delete("temp", true);
            });
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_data.Path))
            {
                if (!Directory.Exists("temp"))
                {
                    Directory.CreateDirectory("temp");
                }

                _mesureing = true;
                DEBUG.Text = "Start Mesure.";
            }
            else
            {
                DEBUG.Text = "Select file save path.";
            }
        }
    }
}
