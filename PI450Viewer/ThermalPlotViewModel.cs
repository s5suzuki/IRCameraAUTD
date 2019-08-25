using OxyPlot;
using System;
using System.ComponentModel;

namespace PI450Viewer
{
    internal class ThermalPlotViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly int ImageWidth = (int)(double)App.Current.Resources["Width"];
        private static readonly int ImageHeight = (int)(double)App.Current.Resources["Height"];

        private ushort[,] _thermalData;
        private int _viewX;
        private int _viewY;

        public ThermalPlotViewModel()
        {
            _thermalData = new ushort[ImageHeight, ImageWidth];
            XAxis = new DataPoint[ImageWidth];
            YAxis = new DataPoint[ImageHeight];
        }

        private void Update()
        {
            double avgX = 0.0;
            double avgY = 0.0;
            double avgT = 0.0;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double maxT = double.MinValue;
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double minT = double.MaxValue;

            for (int i = 0; i < _thermalData.GetLength(0); i++)
            {
                for (int j = 0; j < _thermalData.GetLength(1); j++)
                {
                    double temp = ConvertToTemp(_thermalData[i, j]);
                    UpdateProperty(temp, ref avgT, ref maxT, ref minT);

                    if (i == _viewY)
                    {
                        XAxis[j] = new DataPoint(j, ConvertToTemp(_thermalData[_viewY, j]));
                        UpdateProperty(temp, ref avgX, ref maxX, ref minX);
                    }

                    if (j == _viewX)
                    {
                        YAxis[i] = new DataPoint(YAxis.Length - 1 - i, ConvertToTemp(_thermalData[i, _viewX]));
                        UpdateProperty(temp, ref avgY, ref maxY, ref minY);
                    }
                }
            }
            AverageTempTotal = avgT / _thermalData.Length;
            AverageTempX = avgX / XAxis.Length;
            AverageTempY = avgY / YAxis.Length;

            MaxTempTotal = maxT;
            MaxTempX = maxX;
            MaxTempY = maxY;

            MinTempTotal = minT;
            MinTempX = minX;
            MinTempY = minY;
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

        public ushort[,] ThermalData
        {
            get => _thermalData;
            set
            {
                _thermalData = value;
                Update();
            }
        }
        public int ViewX
        {
            set
            {
                _viewX = value;
                Update();
            }
        }
        public int ViewY
        {
            set
            {
                _viewY = value;
                Update();
            }
        }

        #region AverageTempTotal

        public double AverageTempTotal
        {
            get => _AverageTempTotal;
            set
            {
                _AverageTempTotal = value;
                PropertyChanged.Raise(this, _nameAverageTempTotal);
            }
        }

        private double _AverageTempTotal;
        private readonly string _nameAverageTempTotal = PropertyName<ThermalPlotViewModel>.Get(x => x.AverageTempTotal);

        #endregion
        #region MaxTempTotal

        public double MaxTempTotal
        {
            get => _MaxTempTotal;
            set
            {
                _MaxTempTotal = value;
                PropertyChanged.Raise(this, _nameMaxTempTotal);
            }
        }

        private double _MaxTempTotal;
        private readonly string _nameMaxTempTotal = PropertyName<ThermalPlotViewModel>.Get(x => x.MaxTempTotal);

        #endregion
        #region MinTempTotal

        public double MinTempTotal
        {
            get => _MinTempTotal;
            set
            {
                _MinTempTotal = value;
                PropertyChanged.Raise(this, _nameMinTempTotal);
            }
        }

        private double _MinTempTotal;
        private readonly string _nameMinTempTotal = PropertyName<ThermalPlotViewModel>.Get(x => x.MinTempTotal);

        #endregion

        #region AverageTempX

        public double AverageTempX
        {
            get => _AverageTempX;
            set
            {
                _AverageTempX = value;
                PropertyChanged.Raise(this, _nameAverageTempX);
            }
        }

        private double _AverageTempX;
        private readonly string _nameAverageTempX = PropertyName<ThermalPlotViewModel>.Get(x => x.AverageTempX);

        #endregion
        #region MaxTempX

        public double MaxTempX
        {
            get => _MaxTempX;
            set
            {
                _MaxTempX = value;
                PropertyChanged.Raise(this, _nameMaxTempX);
            }
        }

        private double _MaxTempX;
        private readonly string _nameMaxTempX = PropertyName<ThermalPlotViewModel>.Get(x => x.MaxTempX);

        #endregion
        #region MinTempX

        public double MinTempX
        {
            get => _MinTempX;
            set
            {
                _MinTempX = value;
                PropertyChanged.Raise(this, _nameMinTempX);
            }
        }

        private double _MinTempX;
        private readonly string _nameMinTempX = PropertyName<ThermalPlotViewModel>.Get(x => x.MinTempX);

        #endregion

        #region AverageTempY

        public double AverageTempY
        {
            get => _AverageTempY;
            set
            {
                _AverageTempY = value;
                PropertyChanged.Raise(this, _nameAverageTempY);
            }
        }

        private double _AverageTempY;
        private readonly string _nameAverageTempY = PropertyName<ThermalPlotViewModel>.Get(x => x.AverageTempY);

        #endregion
        #region MaxTempY

        public double MaxTempY
        {
            get => _MaxTempY;
            set
            {
                _MaxTempY = value;
                PropertyChanged.Raise(this, _nameMaxTempY);
            }
        }

        private double _MaxTempY;
        private readonly string _nameMaxTempY = PropertyName<ThermalPlotViewModel>.Get(x => x.MaxTempY);

        #endregion
        #region MinTempY

        public double MinTempY
        {
            get => _MinTempY;
            set
            {
                _MinTempY = value;
                PropertyChanged.Raise(this, _nameMinTempY);
            }
        }

        private double _MinTempY;
        private readonly string _nameMinTempY = PropertyName<ThermalPlotViewModel>.Get(x => x.MinTempY);

        #endregion

        #region Path

        public string Path
        {
            get => _Path;
            set
            {
                _Path = value;
                PropertyChanged.Raise(this, _namePath);
            }
        }

        private string _Path;
        private readonly string _namePath = PropertyName<ThermalPlotViewModel>.Get(x => x.Path);

        #endregion


        public DataPoint[] XAxis { get; }
        public DataPoint[] YAxis { get; }
    }
}
