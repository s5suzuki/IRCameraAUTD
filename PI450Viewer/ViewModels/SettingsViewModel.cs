/*
 * File: SettingsViewModel.cs
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

using System;
using libirimagerNet;
using PI450Viewer.Helpers;
using PI450Viewer.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace PI450Viewer.ViewModels
{
    public class SettingsViewModel : ReactivePropertyBase
    {
        public AngleUnit[] AngleUnits { get; } = (AngleUnit[])Enum.GetValues(typeof(AngleUnit));
        public ReactivePropertySlim<AngleUnit> AngleUnit { get; }

        public OptrisColoringPalette[] Palettes { get; } = (OptrisColoringPalette[])Enum.GetValues(typeof(OptrisColoringPalette));
        public ReactivePropertySlim<OptrisColoringPalette> Palette { get; }

        public OptrisPaletteScalingMethod[] Scalings { get; } = (OptrisPaletteScalingMethod[])Enum.GetValues(typeof(OptrisPaletteScalingMethod));
        public ReactivePropertySlim<OptrisPaletteScalingMethod> Scaling { get; }

        public ReactivePropertySlim<double> ManualPaletteMin { get; }
        public ReactivePropertySlim<double> ManualPaletteMax { get; }
        public ReactivePropertySlim<bool> LinkAUTDThermo { get; }

        public SettingsViewModel()
        {
            AngleUnit = General.Instance.ToReactivePropertySlimAsSynchronized(g => g.AngleUnit);
            Palette = General.Instance.ToReactivePropertySlimAsSynchronized(g => g.Palette);
            Palette.Subscribe(p => ThermalCameraHandler.Instance.SetPalette(p));
            Scaling = General.Instance.ToReactivePropertySlimAsSynchronized(g => g.Scaling);
            Scaling.Subscribe(p => ThermalCameraHandler.Instance.SetScaling(p));

            ManualPaletteMin = General.Instance.ToReactivePropertySlimAsSynchronized(g => g.ManualPaletteMin);
            ManualPaletteMax = General.Instance.ToReactivePropertySlimAsSynchronized(g => g.ManualPaletteMax);
            ManualPaletteMin.Subscribe(p => ThermalCameraHandler.Instance.SetPaletteManualRange(p, ManualPaletteMax.Value));
            ManualPaletteMax.Subscribe(p => ThermalCameraHandler.Instance.SetPaletteManualRange(ManualPaletteMin.Value, p));

            LinkAUTDThermo = General.Instance.ToReactivePropertySlimAsSynchronized(g => g.LinkAUTDThermo);
        }
    }
}