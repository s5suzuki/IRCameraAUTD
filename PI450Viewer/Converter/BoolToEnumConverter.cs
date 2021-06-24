/*
 * File: BoolToEnumConverter.cs
 * Project: Converter
 * Created Date: 29/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 30/04/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System;
using System.Globalization;
using System.Windows.Data;

namespace PI450Viewer.Converter
{
    public class BoolToEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string parameterString)) return System.Windows.DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false) return System.Windows.DependencyProperty.UnsetValue;

            return (int)Enum.Parse(value.GetType(), parameterString) == (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueBool && !valueBool) return Binding.DoNothing;
            return parameter is string parameterString ? Enum.Parse(targetType, parameterString) : Binding.DoNothing;
        }
    }
}
