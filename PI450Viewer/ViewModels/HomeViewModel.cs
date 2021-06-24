/*
 * File: HomeViewModel.cs
 * Project: ViewModels
 * Created Date: 29/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 24/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System.Drawing;
using OxyPlot;
using PI450Viewer.Helpers;
using PI450Viewer.Models;
using Reactive.Bindings;
using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Image = System.Windows.Controls.Image;

namespace PI450Viewer.ViewModels
{
    public class HomeViewModel : ReactivePropertyBase
    {
        private static readonly int ImageWidth = (int)(double)Application.Current.Resources["ThermalImageWidth"];
        private static readonly int ImageHeight = (int)(double)Application.Current.Resources["ThermalImageHeight"];
        private static readonly double Margin = (double)Application.Current.Resources["ThermalImageDragMargin"];

        public ReactiveProperty<PlotModel> PlotXModel { get; }
        public ReactiveProperty<PlotModel> PlotYModel { get; }

        public ReactiveProperty<double> MaxTempX { get; }
        public ReactiveProperty<double> MinTempX { get; }
        public ReactiveProperty<double> AverageTempX { get; }

        public ReactiveProperty<double> MaxTempY { get; }
        public ReactiveProperty<double> MinTempY { get; }
        public ReactiveProperty<double> AverageTempY { get; }

        public ReactiveProperty<double> MaxTempTotal { get; }
        public ReactiveProperty<double> MinTempTotal { get; }
        public ReactiveProperty<double> AverageTempTotal { get; }

        public ReactiveProperty<Bitmap> PaletteImage { get; }

        public ReactiveProperty<bool> IsConnected { get; }
        public ReactiveProperty<bool> IsRunning { get; }

        public ReactiveCommand Connect { get; }
        public ReactiveCommand Disconnect { get; }
        public ReactiveCommand Pause { get; }
        public ReactiveCommand Resume { get; }

        public ReactiveCommand<DragDeltaEventArgs> CursorYDragDelta { get; }
        public ReactiveCommand<DragDeltaEventArgs> CursorXDragDelta { get; }

        public ReactiveProperty<Visibility> ThermalAtCursorVisible { get; }
        public ReactiveProperty<double> ThermalAtCursorX { get; }
        public ReactiveProperty<double> ThermalAtCursorY { get; }
        public ReactiveProperty<string> ThermalAtCursor { get; }

        public ReactiveCommand<MouseEventArgs> ThermalImageMouseMove { get; }
        public ReactiveCommand ThermalImageMouseRightUp { get; }
        public ReactiveCommand ThermalImageMouseRightDown { get; }

        public HomeViewModel()
        {
            var model = ThermalCameraHandler.Instance;

            PaletteImage = model.PaletteImage;

            PlotXModel = model.PlotXModel;
            PlotYModel = model.PlotYModel;

            MaxTempX = model.MaxTempX;
            MinTempX = model.MinTempX;
            AverageTempX = model.AverageTempX;
            MaxTempY = model.MaxTempY;
            MinTempY = model.MinTempY;
            AverageTempY = model.AverageTempY;
            MaxTempTotal = model.MaxTempTotal;
            MinTempTotal = model.MinTempTotal;
            AverageTempTotal = model.AverageTempTotal;

            IsConnected = new ReactiveProperty<bool>(false);
            IsRunning = new ReactiveProperty<bool>(false);

            Connect = IsConnected.Select(b => !b).ToReactiveCommand();
            Connect.Subscribe(() =>
            {
                model.Connect();
                IsConnected.Value = true;
                IsRunning.Value = true;
            });
            Disconnect = IsConnected.Select(b => b).ToReactiveCommand();
            Disconnect.Subscribe(() =>
            {
                model.Disconnect();
                IsConnected.Value = false;
                IsRunning.Value = false;
            });
            Pause = new[] { IsConnected, IsRunning }
                .CombineLatest(x => x.All(y => y)).ToReactiveCommand();
            Pause.Subscribe(() =>
            {
                model.Pause();
                IsRunning.Value = false;
            });
            Resume = new[] { IsConnected, IsRunning }
                .CombineLatest(x => x[0] && !x[1]).ToReactiveCommand();
            Resume.Subscribe(() =>
            {
                model.Resume();
                IsRunning.Value = true;
            });

            CursorYDragDelta = new ReactiveCommand<DragDeltaEventArgs>();
            CursorYDragDelta.Subscribe(e =>
            {
                if (!(e.Source is Thumb thumb)) return;
                var x = (int)Math.Clamp(Canvas.GetLeft(thumb) + e.HorizontalChange + Margin, 0, ImageWidth - 1);
                Canvas.SetLeft(thumb, x - Margin);
                model.SetCursorX(x);
            });

            CursorXDragDelta = new ReactiveCommand<DragDeltaEventArgs>();
            CursorXDragDelta.Subscribe(e =>
            {
                if (!(e.Source is Thumb thumb)) return;
                var y = (int)Math.Clamp(Canvas.GetTop(thumb) + e.VerticalChange + Margin, 0, ImageHeight - 1);
                Canvas.SetTop(thumb, y - Margin);
                model.SetCursorY(y);
            });

            ThermalAtCursorVisible = new ReactiveProperty<Visibility>(Visibility.Hidden);
            ThermalAtCursorX = new ReactiveProperty<double>();
            ThermalAtCursorY = new ReactiveProperty<double>();
            ThermalAtCursor = new ReactiveProperty<string>();

            ThermalImageMouseMove = new ReactiveCommand<MouseEventArgs>();
            ThermalImageMouseMove.Subscribe(e =>
            {
                System.Windows.Point p;
                switch (e.Source)
                {
                    case Image image:
                        p = e.GetPosition(image);
                        break;
                    case Thumb thumb:
                        p = e.GetPosition(thumb);
                        p.Offset(Canvas.GetLeft(thumb), Canvas.GetTop(thumb));
                        break;
                    default: return;
                }

                ThermalAtCursorX.Value = p.X + 5;
                ThermalAtCursorY.Value = p.Y - 10;

                var x = (int)Math.Clamp(p.X, 0, ImageWidth - 1);
                var y = (int)Math.Clamp(p.Y, 0, ImageHeight - 1);
                ThermalAtCursor.Value = model.GetTemp(x, y).ToString("00.0", CultureInfo.InvariantCulture) + "℃";
            });

            ThermalImageMouseRightUp = new ReactiveCommand();
            ThermalImageMouseRightUp.Subscribe(() => ThermalAtCursorVisible.Value = Visibility.Hidden);

            ThermalImageMouseRightDown = new ReactiveCommand();
            ThermalImageMouseRightDown.Subscribe(() => ThermalAtCursorVisible.Value = Visibility.Visible);
        }
    }
}
