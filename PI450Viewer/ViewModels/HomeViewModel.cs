/*
 * File: HomeViewModel.cs
 * Project: ViewModels
 * Created Date: 29/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 26/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System;
using PI450Viewer.Helpers;
using PI450Viewer.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using OxyPlot;
using PI450Viewer.Domain;
using Image = System.Windows.Controls.Image;

namespace PI450Viewer.ViewModels
{
    public class HomeViewModel : ReactivePropertyBase
    {
        private static readonly int ImageWidth = (int)(double)Application.Current.Resources["ThermalImageWidth"];
        private static readonly int ImageHeight = (int)(double)Application.Current.Resources["ThermalImageHeight"];
        private static readonly double Margin = (double)Application.Current.Resources["ThermalImageDragMargin"];

        public ReactivePropertySlim<PlotModel> PlotXModel { get; }
        public ReactivePropertySlim<PlotModel> PlotYModel { get; }

        public ReactivePropertySlim<bool> FixAxes { get; }
        public ReactivePropertySlim<double> AxesMinimum { get; }
        public ReactivePropertySlim<double> AxesMaximum { get; }

        public ReactivePropertySlim<double> MaxTempX { get; }
        public ReactivePropertySlim<double> MinTempX { get; }
        public ReactivePropertySlim<double> AverageTempX { get; }

        public ReactivePropertySlim<double> MaxTempY { get; }
        public ReactivePropertySlim<double> MinTempY { get; }
        public ReactivePropertySlim<double> AverageTempY { get; }

        public ReactivePropertySlim<double> MaxTempTotal { get; }
        public ReactivePropertySlim<double> MinTempTotal { get; }
        public ReactivePropertySlim<double> AverageTempTotal { get; }

        public ReactivePropertySlim<Bitmap> PaletteImage { get; }

        public ReactivePropertySlim<bool> IsConnected { get; }
        public ReactivePropertySlim<bool> IsRunning { get; }

        public AsyncReactiveCommand Connect { get; }
        public AsyncReactiveCommand Disconnect { get; }
        public ReactiveCommand Start { get; }
        public AsyncReactiveCommand Stop { get; }

        public ReactivePropertySlim<bool> CursorEnable { get; }
        public ReactivePropertySlim<double> CursorXPos { get; }
        public ReactivePropertySlim<double> CursorYPos { get; }

        public ReactiveCommand<DragDeltaEventArgs> CursorYDragDelta { get; }
        public ReactiveCommand<DragDeltaEventArgs> CursorXDragDelta { get; }

        public ReactivePropertySlim<Visibility> ThermalAtCursorVisible { get; }
        public ReactivePropertySlim<double> ThermalAtCursorX { get; }
        public ReactivePropertySlim<double> ThermalAtCursorY { get; }
        public ReactivePropertySlim<string> ThermalAtCursor { get; }

        public ReactiveCommand<MouseEventArgs> ThermalImageMouseMove { get; }
        public ReactiveCommand ThermalImageMouseRightUp { get; }
        public ReactiveCommand ThermalImageMouseRightDown { get; }

        public ReactivePropertySlim<bool> IsDataSave { get; }
        public ReactivePropertySlim<string> DataFolder { get; }
        public AsyncReactiveCommand SelectFolder { get; }

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

            CursorEnable = model.ToReactivePropertySlimAsSynchronized(m => m.CursorEnable);

            FixAxes = model.ToReactivePropertySlimAsSynchronized(m => m.FixAxes);
            FixAxes.Subscribe(_ => model.SetPlotAxes());
            AxesMinimum = model.ToReactivePropertySlimAsSynchronized(m => m.AxesMinimum);
            AxesMinimum.Subscribe(_ => model.SetPlotAxes());
            AxesMaximum = model.ToReactivePropertySlimAsSynchronized(m => m.AxesMaximum);
            AxesMaximum.Subscribe(_ => model.SetPlotAxes());

            IsConnected = new ReactivePropertySlim<bool>();
            IsRunning = model.IsStarted;

            Connect = IsConnected.Select(b => !b).ToAsyncReactiveCommand();
            Connect.Subscribe(async () =>
            {
                try
                {
                    model.Connect();
                    IsConnected.Value = true;
                    IsRunning.Value = false;
                }
                catch (Exception ex)
                {
                    var vm = new ErrorDialogViewModel { Message = { Value = ex.Message } };
                    var dialog = new ErrorDialog
                    {
                        DataContext = vm
                    };
                    await DialogHost.Show(dialog, "MessageDialogHost");
                }
            });
            Disconnect = IsConnected.Select(b => b).ToAsyncReactiveCommand();
            Disconnect.Subscribe(async () =>
            {
                try
                {
                    await model.Disconnect();
                    IsConnected.Value = false;
                    IsRunning.Value = false;
                }
                catch (Exception ex)
                {
                    var vm = new ErrorDialogViewModel { Message = { Value = ex.Message } };
                    var dialog = new ErrorDialog
                    {
                        DataContext = vm
                    };
                    await DialogHost.Show(dialog, "MessageDialogHost");
                }
            });
            Start = new[] { IsConnected, IsRunning }
                .CombineLatest(x => x[0] && !x[1]).ToReactiveCommand();
            Start.Subscribe(() =>
            {
                if (General.Instance.LinkAUTDThermo) AUTDHandler.Instance.Start();
                model.Start();
                IsRunning.Value = true;
            });
            Stop = new[] { IsConnected, IsRunning }
                .CombineLatest(x => x.All(y => y)).ToAsyncReactiveCommand();
            Stop.Subscribe(async () =>
            {
                if (General.Instance.LinkAUTDThermo) AUTDHandler.Instance.Pause();
                await model.Stop();
                IsRunning.Value = false;
            });

            CursorYPos = new ReactivePropertySlim<double>(model.ViewY - Margin);
            CursorYDragDelta = new ReactiveCommand<DragDeltaEventArgs>();
            CursorYDragDelta.Subscribe(e =>
            {
                if (!(e.Source is Thumb thumb)) return;
                var x = (int)Math.Clamp(Canvas.GetLeft(thumb) + e.HorizontalChange + Margin, 0, ImageWidth - 1);
                CursorYPos.Value = x - Margin;
                model.SetCursorX(x);
            });

            CursorXPos = new ReactivePropertySlim<double>(model.ViewX - Margin);
            CursorXDragDelta = new ReactiveCommand<DragDeltaEventArgs>();
            CursorXDragDelta.Subscribe(e =>
            {
                if (!(e.Source is Thumb thumb)) return;
                var y = (int)Math.Clamp(Canvas.GetTop(thumb) + e.VerticalChange + Margin, 0, ImageHeight - 1);
                CursorXPos.Value = y - Margin;
                model.SetCursorY(y);
            });

            ThermalAtCursorVisible = new ReactivePropertySlim<Visibility>(Visibility.Hidden);
            ThermalAtCursorX = new ReactivePropertySlim<double>();
            ThermalAtCursorY = new ReactivePropertySlim<double>();
            ThermalAtCursor = new ReactivePropertySlim<string>();

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

            IsDataSave = model.ToReactivePropertySlimAsSynchronized(m => m.IsDataSave);
            DataFolder = model.ToReactivePropertySlimAsSynchronized(m => m.DataFolder);
            SelectFolder = IsDataSave.Select(b => b).ToAsyncReactiveCommand();
            SelectFolder.Subscribe(async () =>
            {
                var dialogArgs = new OpenDirectoryDialogArguments
                {
                    Width = 600,
                    Height = 800,
                    CreateNewDirectoryEnabled = true
                };
                var result = await OpenDirectoryDialog.ShowDialogAsync("MessageDialogHost", dialogArgs);
                if (result.Canceled) return;
                try
                {
                    DataFolder.Value = result.Directory;
                }
                catch
                {
                    var vm = new ErrorDialogViewModel { Message = { Value = "Failed to open folder." } };
                    var dialog = new ErrorDialog
                    {
                        DataContext = vm
                    };
                    await DialogHost.Show(dialog, "MessageDialogHost");
                }
            });
        }
    }
}
