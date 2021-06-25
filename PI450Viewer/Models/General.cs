/*
 * File: General.cs
 * Project: Models
 * Created Date: 29/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 25/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using libirimagerNet;
using MaterialDesignThemes.Wpf;
using PI450Viewer.Helpers;
using Reactive.Bindings;

namespace PI450Viewer.Models
{
    public enum AngleUnit
    {
        Radian,
        Degree
    }

    public enum Theme
    {
        Dark,
        Light
    }

    [DataContract]
    public class General : ReactivePropertyBase
    {
        private static Lazy<General> _lazy = new Lazy<General>(() => new General());
        public static General Instance { get => _lazy.Value; set => _lazy = new Lazy<General>(() => value); }

        [DataMember]
        public AngleUnit AngleUnit { get; set; }

        [DataMember]
        public OptrisColoringPalette Palette { get; set; }

        [DataMember]
        public OptrisPaletteScalingMethod Scaling { get; set; }

        [JsonIgnore]
        public ReactivePropertySlim<IBaseTheme> BaseTheme { get; set; }

        [DataMember]
        public Theme BaseThemeStore { get; set; }

        [DataMember]
        public double ManualPaletteMin { get; set; }
        [DataMember]
        public double ManualPaletteMax { get; set; }

        [DataMember]
        public bool LinkAUTDThermo { get; set; }

        private General()
        {
            AngleUnit = AngleUnit.Radian;
            Palette = OptrisColoringPalette.Iron;
            Scaling = OptrisPaletteScalingMethod.MinMax;
            BaseTheme = new ReactivePropertySlim<IBaseTheme>(MaterialDesignThemes.Wpf.Theme.Dark);
            LinkAUTDThermo = true;
            ManualPaletteMin = 0;
            ManualPaletteMax = 100;
        }

        public double ConvertAngle(double angle)
        {
            return AngleUnit == AngleUnit.Radian ? angle : angle / 180.0 * Math.PI;
        }

        internal void Store()
        {
            Instance.BaseThemeStore = Instance.BaseTheme.Value == MaterialDesignThemes.Wpf.Theme.Dark ? Theme.Dark : Theme.Light;
        }

        internal void Load()
        {
            Instance.BaseTheme.Value = Instance.BaseThemeStore == Theme.Dark ? MaterialDesignThemes.Wpf.Theme.Dark : MaterialDesignThemes.Wpf.Theme.Light;
        }
    }
}
