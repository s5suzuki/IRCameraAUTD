/*
 * File: General.cs
 * Project: Models
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

    public class General : ReactivePropertyBase
    {
        private static Lazy<General> _lazy = new Lazy<General>(() => new General());
        public static General Instance { get => _lazy.Value; set => _lazy = new Lazy<General>(() => value); }

        public AngleUnit AngleUnit { get; set; }

        public ReactiveProperty<IBaseTheme> BaseTheme { get; set; }

        private General()
        {
            AngleUnit = AngleUnit.Radian;
            BaseTheme = new ReactiveProperty<IBaseTheme>(Theme.Dark);
        }

        public double ConvertAngle(double angle)
        {
            return AngleUnit == AngleUnit.Radian ? angle : angle / 180.0 * Math.PI;
        }
    }
}
