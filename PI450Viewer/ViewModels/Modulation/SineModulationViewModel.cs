/*
 * File: SineModulationViewModel.cs
 * Project: Modulation
 * Created Date: 06/05/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 03/06/2021
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
    public class SineModulationViewModel : ReactivePropertyBase
    {


        public ReactiveProperty<SineModulation> Sine { get; }

        public SineModulationViewModel()
        {
            Sine = AUTDSettings.Instance.ToReactivePropertyAsSynchronized(i => i.Sine);
        }
    }
}
