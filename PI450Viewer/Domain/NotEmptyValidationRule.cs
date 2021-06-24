/*
 * File: NotEmptyValidationRule.cs
 * Project: Domain
 * Created Date: 29/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 30/04/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System.Globalization;
using System.Windows.Controls;

namespace PI450Viewer.Domain
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace(value.ToString())
            ? new ValidationResult(false, "Field is required.")
            : ValidationResult.ValidResult;
        }
    }
}
