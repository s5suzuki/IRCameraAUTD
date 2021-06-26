/*
 * File: ThermalCameraHandler.cs
 * Project: Models
 * Created Date: 25/06/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 26/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using libirimagerNet;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PI450Viewer.Helpers;
using PI450Viewer.Models.Camera;
using Reactive.Bindings;

namespace PI450Viewer.Models
{
    public enum CameraState
    {
        Idle,
        Running,
        WritingData,
        Disconnected
    }

    [DataContract]
    public class ThermalCameraHandler : ReactivePropertyBase
    {
        private static Lazy<ThermalCameraHandler> _lazy = new Lazy<ThermalCameraHandler>(() => new ThermalCameraHandler());
        public static ThermalCameraHandler Instance { get => _lazy.Value; set => _lazy = new Lazy<ThermalCameraHandler>(() => value); }

        private readonly ICamera _camera;
        private OxyColor _color;

        private static readonly int ImageWidth = (int)(double)Application.Current.Resources["ThermalImageWidth"];
        private static readonly int ImageHeight = (int)(double)Application.Current.Resources["ThermalImageHeight"];

        [JsonIgnore]
        public ReactivePropertySlim<Bitmap> PaletteImage { get; set; }
        [JsonIgnore]
        public ReactivePropertySlim<PlotModel> PlotXModel { get; }
        [JsonIgnore]
        public ReactivePropertySlim<PlotModel> PlotYModel { get; }
        private readonly DataPoint[] _plotX;
        private readonly DataPoint[] _plotY;

        private Task? _thermalHandler;
        private bool _grabImage;

        [JsonIgnore]
        public ReactivePropertySlim<CameraState> CameraState { get; set; }

        [JsonIgnore]
        public ushort[,] ThermalData { get; set; }

        [DataMember]
        public double ViewY { get; set; }
        [DataMember]
        public double ViewX { get; set; }

        [DataMember]
        public bool CursorEnable { get; set; }

        [DataMember]
        public bool FixAxes { get; set; }
        [DataMember]
        public double AxesMinimum { get; set; }
        [DataMember]
        public double AxesMaximum { get; set; }

        [JsonIgnore]
        public ReactivePropertySlim<double> MaxTempX { get; set; }
        [JsonIgnore]
        public ReactivePropertySlim<double> MinTempX { get; set; }
        [JsonIgnore]
        public ReactivePropertySlim<double> AverageTempX { get; set; }

        [JsonIgnore]
        public ReactivePropertySlim<double> MaxTempY { get; set; }
        [JsonIgnore]
        public ReactivePropertySlim<double> MinTempY { get; set; }
        [JsonIgnore]
        public ReactivePropertySlim<double> AverageTempY { get; set; }

        [JsonIgnore]
        public ReactivePropertySlim<double> MaxTempTotal { get; set; }
        [JsonIgnore]
        public ReactivePropertySlim<double> MinTempTotal { get; set; }
        [JsonIgnore]
        public ReactivePropertySlim<double> AverageTempTotal { get; set; }

        [DataMember]
        public bool IsDataSave { get; set; }
        [DataMember]
        public string DataFolder { get; set; }

        private string? _tmpPath;
        private readonly object _lock = new object();
        private readonly List<Task> _saveTasks = new List<Task>();
        private bool _updateImage;

        private Task? _timeout;

        public ThermalCameraHandler()
        {
            _camera = new Pi450();

            AxesMinimum = 0;
            AxesMaximum = 100;

            ViewY = (double)ImageWidth / 2;
            ViewX = (double)ImageHeight / 2;

            CursorEnable = true;

            ThermalData = new ushort[ImageHeight, ImageWidth];

            _plotX = new DataPoint[ImageWidth];
            PlotXModel = new ReactivePropertySlim<PlotModel>(new PlotModel());

            _plotY = new DataPoint[ImageHeight];
            PlotYModel = new ReactivePropertySlim<PlotModel>(new PlotModel());

            _color = General.Instance.BaseTheme.Value == MaterialDesignThemes.Wpf.Theme.Dark ? OxyColors.White : OxyColors.Black;
            SetPlotAxes();

            PaletteImage = new ReactivePropertySlim<Bitmap>(new Bitmap(ImageWidth, ImageHeight));

            MaxTempX = new ReactivePropertySlim<double>();
            MinTempX = new ReactivePropertySlim<double>();
            AverageTempX = new ReactivePropertySlim<double>();
            MaxTempY = new ReactivePropertySlim<double>();
            MinTempY = new ReactivePropertySlim<double>();
            AverageTempY = new ReactivePropertySlim<double>();
            MinTempTotal = new ReactivePropertySlim<double>();
            MaxTempTotal = new ReactivePropertySlim<double>();
            AverageTempTotal = new ReactivePropertySlim<double>();

            IsDataSave = true;
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            DataFolder = basePath != null ? Path.Combine(basePath, "data") : string.Empty;

            CameraState = new ReactivePropertySlim<CameraState>(Models.CameraState.Disconnected);
        }

        private void SaveData()
        {
            var path = Path.Combine(_tmpPath!, DateTime.Now.Ticks + ".bin");
            lock (_lock)
                while (true)
                {
                    if (!File.Exists(path)) break;
                    path = Path.Combine(_tmpPath!, DateTime.Now.Ticks + ".bin");
                }
            using FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            var bf = new BinaryFormatter();
            bf.Serialize(fs, ThermalData);
        }

        private void FormatToCsv()
        {
            Parallel.ForEach(Directory.EnumerateFiles(_tmpPath!), binFile =>
            {
                if (binFile == null) return;

                var fileName = binFile.Split('\\').Last();
                var tick = fileName[..^4];

                using var fis = new FileStream(binFile, FileMode.Open, FileAccess.Read);
                var bf = new BinaryFormatter();
                var data = (bf.Deserialize(fis) as ushort[,])!;

                var sb = new StringBuilder();
                for (var i = 0; i < data.GetLength(0); i++)
                {
                    for (var j = 0; j < data.GetLength(1); j++)
                    {
                        if (j != 0) sb.Append(",");
                        sb.Append(ConvertToTemp(data[i, j]));
                    }
                    sb.AppendLine();
                }

                fis.Dispose();
                File.Delete(binFile);
                var path = Path.Combine(_tmpPath![..^4], tick + ".csv");
                using var sw = new StreamWriter(path);
                sw.Write(sb.ToString());
            });
            Directory.Delete(_tmpPath!);
        }

        public void SetPlotAxes()
        {
            PlotXModel.Value.PlotAreaBorderColor = _color;
            PlotXModel.Value.Axes.Clear();
            PlotXModel.Value.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = ImageWidth,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                AxislineColor = _color,
                MajorGridlineColor = _color,
                MinorGridlineColor = _color,
                TextColor = _color,
                TicklineColor = _color
            });
            PlotXModel.Value.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = FixAxes ? AxesMinimum : double.NaN,
                Maximum = FixAxes ? AxesMaximum : double.NaN,
                AxislineColor = _color,
                MajorGridlineColor = _color,
                MinorGridlineColor = _color,
                TextColor = _color,
                TicklineColor = _color
            });

            PlotYModel.Value.PlotAreaBorderColor = _color;
            PlotYModel.Value.Axes.Clear();
            PlotYModel.Value.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = ImageHeight,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                AxislineColor = _color,
                MajorGridlineColor = _color,
                MinorGridlineColor = _color,
                TextColor = _color,
                TicklineColor = _color
            });
            PlotYModel.Value.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AxislineColor = _color,
                Minimum = FixAxes ? AxesMinimum : double.NaN,
                Maximum = FixAxes ? AxesMaximum : double.NaN,
                MajorGridlineColor = _color,
                MinorGridlineColor = _color,
                TextColor = _color,
                TicklineColor = _color
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
            if (CameraState.Value != Models.CameraState.Disconnected) return;
            _camera.Connect("generic.xml");
            _grabImage = true;
            CameraState.Value = Models.CameraState.Idle;
            _updateImage = true;
            _thermalHandler = Task.Run(ImageGrabberMethod);
        }

        public async Task Disconnect()
        {
            if (CameraState.Value == Models.CameraState.Disconnected) return;
            _updateImage = false;
            _grabImage = false;
            if (_thermalHandler != null) await _thermalHandler;
            _camera.Disconnect();
            CameraState.Value = Models.CameraState.Disconnected;
        }

        public void Pause()
        {
            _updateImage = false;
        }
        public void Resume()
        {
            _updateImage = true;
        }

        public void Start()
        {
            if (CameraState.Value != Models.CameraState.Idle) return;
            if (IsDataSave)
            {
                _tmpPath = Path.Combine(DataFolder, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), "bin");
                Directory.CreateDirectory(_tmpPath);
                _timeout = General.Instance.TimeoutMs == 0 ? null : Task.Delay(General.Instance.TimeoutMs);
            }
            CameraState.Value = Models.CameraState.Running;
        }

        public async Task Stop()
        {
            if (CameraState.Value != Models.CameraState.Running) return;
            if (IsDataSave)
            {
                CameraState.Value = Models.CameraState.WritingData;
                await Task.WhenAll(_saveTasks);
                _saveTasks.Clear();
                FormatToCsv();
            }
            CameraState.Value = Models.CameraState.Idle;
        }

        private void ImageGrabberMethod()
        {
            while (_grabImage)
            {
                if (!_updateImage) continue;

                var images = _camera.GrabImage();
                PaletteImage.Value = images.PaletteImage;
                ThermalData = images.ThermalImage;
                if (CameraState.Value == Models.CameraState.Running && IsDataSave)
                {
                    _saveTasks.Add(Task.Run(SaveData));
                    if (_timeout?.IsCompleted ?? false) Application.Current.Dispatcher.Invoke(Stop);
                }
                Update();
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

                    if (i == (int)ViewY)
                    {
                        _plotX[j] = new DataPoint(j, temp);
                        UpdateProperty(temp, ref avgX, ref maxX, ref minX);
                    }

                    if (j != (int)ViewX) continue;
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

            if (!CursorEnable) return;

            var seriesX = new LineSeries();
            seriesX.Points.AddRange(_plotX);
            lock (PlotXModel.Value.SyncRoot)
            {
                PlotXModel.Value.Series.Clear();
                PlotXModel.Value.Series.Add(seriesX);
                PlotXModel.Value.InvalidatePlot(true);
            }

            PlotYModel.Value.Series.Clear();
            var seriesY = new LineSeries();
            seriesY.Points.AddRange(_plotY);
            lock (PlotYModel.Value.SyncRoot)
            {
                PlotYModel.Value.Series.Clear();
                PlotYModel.Value.Series.Add(seriesY);
                PlotYModel.Value.InvalidatePlot(true);
            }
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

        public void SetPalette(OptrisColoringPalette optrisColoringPalette)
        {
            _camera.SetPaletteFormat(optrisColoringPalette, General.Instance.Scaling);
        }

        public void SetScaling(OptrisPaletteScalingMethod optrisPaletteScalingMethod)
        {
            _camera.SetPaletteFormat(General.Instance.Palette, optrisPaletteScalingMethod);
        }

        public void SetPaletteManualRange(double min, double max)
        {
            _camera.SetPaletteManualRange(min, max);
        }

        public void SetColor(OxyColor color)
        {
            _color = color;
        }
    }
}
