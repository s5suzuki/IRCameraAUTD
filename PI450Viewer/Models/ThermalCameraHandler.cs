using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PI450Viewer.Domain;
using PI450Viewer.Helpers;
using PI450Viewer.Models.Camera;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace PI450Viewer.Models
{
    public class ThermalCameraHandler : ReactivePropertyBase
    {
        private readonly ICamera _camera;

        private static readonly int ImageWidth = (int)(double)Application.Current.Resources["ThermalImageWidth"];
        private static readonly int ImageHeight = (int)(double)Application.Current.Resources["ThermalImageHeight"];

        public ReactiveProperty<Bitmap> PaletteImage { get; set; }
        public ReactiveProperty<PlotModel> PlotXModel { get; }
        public ReactiveProperty<PlotModel> PlotYModel { get; }
        private readonly DataPoint[] _plotX;
        private readonly DataPoint[] _plotY;

        private Task? _thermalHandler;
        private bool _grabImage;

        public ushort[,] ThermalData { get; set; }

        public int ViewY { get; set; }
        public int ViewX { get; set; }

        public ReactiveProperty<double> MaxTempX { get; set; }
        public ReactiveProperty<double> MinTempX { get; set; }
        public ReactiveProperty<double> AverageTempX { get; set; }

        public ReactiveProperty<double> MaxTempY { get; set; }
        public ReactiveProperty<double> MinTempY { get; set; }
        public ReactiveProperty<double> AverageTempY { get; set; }

        public ReactiveProperty<double> MaxTempTotal { get; set; }
        public ReactiveProperty<double> MinTempTotal { get; set; }
        public ReactiveProperty<double> AverageTempTotal { get; set; }

        public ThermalCameraHandler()
        {
            _camera = new DebugCamera();

            ThermalData = new ushort[ImageHeight, ImageWidth];

            _plotX = new DataPoint[ImageWidth];
            PlotXModel = new ReactiveProperty<PlotModel>(new PlotModel());

            _plotY = new DataPoint[ImageHeight];
            PlotYModel = new ReactiveProperty<PlotModel>(new PlotModel());
            SetPlotAxes(OxyColors.White);

            ReactiveProperty<IBaseTheme> baseTheme = General.Instance.BaseTheme;
            baseTheme.Subscribe(t => { SetPlotAxes(t == MaterialDesignThemes.Wpf.Theme.Dark ? OxyColors.White : OxyColors.Black); });

            PaletteImage = new ReactiveProperty<Bitmap>(new Bitmap(ImageWidth, ImageHeight));

            MaxTempX = new ReactiveProperty<double>();
            MinTempX = new ReactiveProperty<double>();
            AverageTempX = new ReactiveProperty<double>();
            MaxTempY = new ReactiveProperty<double>();
            MinTempY = new ReactiveProperty<double>();
            AverageTempY = new ReactiveProperty<double>();
            MinTempTotal = new ReactiveProperty<double>();
            MaxTempTotal = new ReactiveProperty<double>();
            AverageTempTotal = new ReactiveProperty<double>();
        }

        private void SetPlotAxes(OxyColor color)
        {
            PlotXModel.Value.PlotAreaBorderColor = color;
            PlotXModel.Value.Axes.Clear();
            PlotXModel.Value.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = ImageWidth,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                AxislineColor = color,
                MajorGridlineColor = color,
                MinorGridlineColor = color,
                TextColor = color,
                TicklineColor = color
            });
            PlotXModel.Value.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AxislineColor = color,
                MajorGridlineColor = color,
                MinorGridlineColor = color,
                TextColor = color,
                TicklineColor = color
            });

            PlotYModel.Value.PlotAreaBorderColor = color;
            PlotYModel.Value.Axes.Clear();
            PlotYModel.Value.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = ImageHeight,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                AxislineColor = color,
                MajorGridlineColor = color,
                MinorGridlineColor = color,
                TextColor = color,
                TicklineColor = color
            });
            PlotYModel.Value.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AxislineColor = color,
                MajorGridlineColor = color,
                MinorGridlineColor = color,
                TextColor = color,
                TicklineColor = color
            });

            var lineSeriesX = new LineSeries();
            lineSeriesX.Points.AddRange(_plotX);
            PlotXModel.Value.Series.Add(lineSeriesX);
            PlotXModel.Value.InvalidatePlot(true);

            var lineSeriesY = new LineSeries();
            lineSeriesY.Points.AddRange(_plotY);
            PlotYModel.Value.Series.Add(lineSeriesY);
            PlotYModel.Value.InvalidatePlot(true);
        }

        public void Connect()
        {
            _camera.Connect("generic.xml");
            _grabImage = true;
            _thermalHandler = Task.Run(ImageGrabberMethod);
        }

        public async void Disconnect()
        {
            _grabImage = false;
            if (_thermalHandler != null) await _thermalHandler;
            _camera.Disconnect();
        }

        private async Task ImageGrabberMethod()
        {
            while (_grabImage)
            {
                try
                {
                    var (bitmap, thermal) = _camera.GrabImage();
                    PaletteImage.Value = bitmap;
                    ThermalData = thermal;
                    Update();
                }
                catch (Exception ex)
                {
                    var vm = new ErrorDialogViewModel { Message = { Value = ex.Message } };
                    var dialog = new ErrorDialog
                    {
                        DataContext = vm
                    };
                    await DialogHost.Show(dialog, "MessageDialogHost");
                    _grabImage = false;
                }
            }
        }

        public void Update()
        {
            var avgX = 0.0;
            var avgY = 0.0;
            var avgT = 0.0;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var maxT = double.MinValue;
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var minT = double.MaxValue;

            for (var i = 0; i < ThermalData.GetLength(0); i++)
                for (var j = 0; j < ThermalData.GetLength(1); j++)
                {
                    var temp = ConvertToTemp(ThermalData[i, j]);
                    UpdateProperty(temp, ref avgT, ref maxT, ref minT);

                    if (i == ViewY)
                    {
                        _plotX[j] = new DataPoint(j, temp);
                        UpdateProperty(temp, ref avgX, ref maxX, ref minX);
                    }

                    if (j != ViewX) continue;
                    _plotY[i] = new DataPoint(_plotY.Length - 1 - i, temp);
                    UpdateProperty(temp, ref avgY, ref maxY, ref minY);
                }

            AverageTempTotal.Value = avgT / ThermalData.Length;
            AverageTempX.Value = avgX / _plotX.Length;
            AverageTempY.Value = avgY / _plotY.Length;

            MaxTempTotal.Value = maxT;
            MaxTempX.Value = maxX;
            MaxTempY.Value = maxY;

            MinTempTotal.Value = minT;
            MinTempX.Value = minX;
            MinTempY.Value = minY;

            PlotXModel.Value.Series.Clear();
            var seriesX = new LineSeries();
            seriesX.Points.AddRange(_plotX);
            PlotXModel.Value.Series.Add(seriesX);
            PlotXModel.Value.InvalidatePlot(true);

            PlotYModel.Value.Series.Clear();
            var seriesY = new LineSeries();
            seriesY.Points.AddRange(_plotY);
            PlotYModel.Value.Series.Add(seriesY);
            PlotYModel.Value.InvalidatePlot(true);
        }

        public static double ConvertToTemp(ushort data)
        {
            return (data - 1000.0) / 10.0;
        }

        private static void UpdateProperty(double temp, ref double avg, ref double max, ref double min)
        {
            avg += temp;
            max = Math.Max(max, temp);
            min = Math.Min(min, temp);
        }

        public void SetCursorY(int y)
        {
            ViewY = y;
            Update();
        }

        public void SetCursorX(int x)
        {
            ViewX = x;
            Update();
        }

        public double GetTemp(int x, int y)
        {
            return ConvertToTemp(ThermalData[y, x]);
        }
    }
}
