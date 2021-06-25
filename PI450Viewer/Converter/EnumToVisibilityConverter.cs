/*
 * File: EnumToVisibilityConverter.cs
 * Project: Converter
 * Created Date: 30/04/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 06/05/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PI450Viewer.Converter
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string parameterString)) return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false) return DependencyProperty.UnsetValue;

            return (int)Enum.Parse(value.GetType(), parameterString) == (int)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
