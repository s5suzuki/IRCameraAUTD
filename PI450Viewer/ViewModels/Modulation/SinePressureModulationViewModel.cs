/*
 * File: SinePressureModulationViewModel.cs
 * Project: Modulation
 * Created Date: 05/06/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 05/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using PI450Viewer.Helpers;
using PI450Viewer.Models;
using PI450Viewer.Models.Modulation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace PI450Viewer.ViewModels.Modulation
{
    public class SinePressureModulationViewModel : ReactivePropertyBase
    {
        public ReactivePropertySlim<SinePressureModulation> SinePressure { get; }

        public SinePressureModulationViewModel()
        {
            SinePressure = AUTDSettings.Instance.ToReactivePropertySlimAsSynchronized(i => i.SinePressure);
        }
    }
}
