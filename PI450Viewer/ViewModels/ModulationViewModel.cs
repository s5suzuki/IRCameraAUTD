﻿/*
 * File: ModulationViewModel.cs
 * Project: ViewModels
 * Created Date: 31/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 05/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using PI450Viewer.Helpers;
using PI450Viewer.Models;
using PI450Viewer.Views.Modulation;
using Reactive.Bindings;

namespace PI450Viewer.ViewModels
{
    internal class ModulationViewModel : ReactivePropertyBase
    {
        public ReactiveCommand SendModulationCommand { get; }

        public ReactivePropertySlim<Page> Page { get; }
        public ReactiveCommand<string> TransitPage { get; }

        public ModulationViewModel()
        {
            SendModulationCommand = AUTDHandler.Instance.IsOpen.Select(b => b).ToReactiveCommand();
            SendModulationCommand.Subscribe(_ => AUTDHandler.Instance.AppendModulation());

            Dictionary<string, Page> pageCache = new Dictionary<string, Page>();
            Page = new ReactivePropertySlim<Page>(new SineView());

            TransitPage = new ReactiveCommand<string>();
            TransitPage.Subscribe(page =>
            {
                if (!pageCache.ContainsKey(page)) pageCache.Add(page, (Page)Activator.CreateInstance(null!, page)?.Unwrap()!);
                Page.Value = pageCache[page]!;
                AUTDSettings.Instance.ModulationSelect = (ModulationSelect)Enum.Parse(typeof(ModulationSelect), page.Split('.').LastOrDefault()?[..^4] ?? string.Empty);
            });
        }
    }
}
