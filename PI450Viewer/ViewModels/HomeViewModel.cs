/*
 * File: HomeViewModel.cs
 * Project: ViewModels
 * Created Date: 29/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 23/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using OxyPlot;
using PI450Viewer.Helpers;
using PI450Viewer.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace PI450Viewer.ViewModels
{
    public class HomeViewModel : ReactivePropertyBase
    {
        public ReactiveProperty<AngleUnit> AngleUnit { get; }

        public ReactiveProperty<DataPoint[]> XAxis { get; }
        public ReactiveProperty<DataPoint[]> YAxis { get; }

        public ReactiveProperty<double> MaxTempX { get; }
        public ReactiveProperty<double> MinTempX { get; }
        public ReactiveProperty<double> AverageTempX { get; }

        public ReactiveProperty<double> MaxTempY { get; }
        public ReactiveProperty<double> MinTempY { get; }
        public ReactiveProperty<double> AverageTempY { get; }

        public ReactiveProperty<double> MaxTempTotal { get; }
        public ReactiveProperty<double> MinTempTotal { get; }
        public ReactiveProperty<double> AverageTempTotal { get; }

        public HomeViewModel()
        {
            AngleUnit = General.Instance.ToReactivePropertyAsSynchronized(g => g.AngleUnit);

            XAxis = new ReactiveProperty<DataPoint[]>();
            YAxis = new ReactiveProperty<DataPoint[]>();

            MaxTempX = new ReactiveProperty<double>();
            MinTempX = new ReactiveProperty<double>();
            AverageTempX = new ReactiveProperty<double>();
            MaxTempY = new ReactiveProperty<double>();
            MinTempY = new ReactiveProperty<double>();
            AverageTempY = new ReactiveProperty<double>();
            MaxTempTotal = new ReactiveProperty<double>();
            MinTempTotal = new ReactiveProperty<double>();
            AverageTempTotal = new ReactiveProperty<double>();
        }
    }
}
