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
        public ReactiveProperty<AngleUnit> AngleUnit { get; }

        public OptrisColoringPalette[] Palettes { get; } = (OptrisColoringPalette[])Enum.GetValues(typeof(OptrisColoringPalette));
        public ReactiveProperty<OptrisColoringPalette> Palette { get; }

        public OptrisPaletteScalingMethod[] Scalings { get; } = (OptrisPaletteScalingMethod[])Enum.GetValues(typeof(OptrisPaletteScalingMethod));
        public ReactiveProperty<OptrisPaletteScalingMethod> Scaling { get; }

        public ReactiveProperty<double> ManualPaletteMin { get; }
        public ReactiveProperty<double> ManualPaletteMax { get; }

        public SettingsViewModel()
        {
            AngleUnit = General.Instance.ToReactivePropertyAsSynchronized(g => g.AngleUnit);
            Palette = General.Instance.ToReactivePropertyAsSynchronized(g => g.Palette);
            Palette.Subscribe(p => ThermalCameraHandler.Instance.SetPalette(p));
            Scaling = General.Instance.ToReactivePropertyAsSynchronized(g => g.Scaling);
            Scaling.Subscribe(p => ThermalCameraHandler.Instance.SetScaling(p));

            ManualPaletteMin = General.Instance.ToReactivePropertyAsSynchronized(g => g.ManualPaletteMin);
            ManualPaletteMax = General.Instance.ToReactivePropertyAsSynchronized(g => g.ManualPaletteMax);
            ManualPaletteMin.Subscribe(p => ThermalCameraHandler.Instance.SetPaletteManualRange(p, ManualPaletteMax.Value));
            ManualPaletteMax.Subscribe(p => ThermalCameraHandler.Instance.SetPaletteManualRange(ManualPaletteMin.Value, p));
        }
    }
}