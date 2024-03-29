﻿/*
 * File: TransducerTestViewModel.cs
 * Project: Gain
 * Created Date: 30/04/2021
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
using PI450Viewer.Models.Gain;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace PI450Viewer.ViewModels.Gain
{
    public class TransducerTestViewModel : ReactivePropertyBase
    {
        public ReactivePropertySlim<TransducerTest> TransducerTest { get; }

        public TransducerTestViewModel()
        {
            TransducerTest = AUTDSettings.Instance.ToReactivePropertySlimAsSynchronized(i => i.TransducerTest);
        }
    }
}
