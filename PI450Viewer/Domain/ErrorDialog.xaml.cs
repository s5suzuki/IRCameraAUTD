/*
 * File: ErrorDialog.xaml.cs
 * Project: Domain
 * Created Date: 30/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 03/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using PI450Viewer.Helpers;
using Reactive.Bindings;

namespace PI450Viewer.Domain
{
    public sealed partial class ErrorDialog
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }
    }

    public class ErrorDialogViewModel : ReactivePropertyBase
    {


        public ReactiveProperty<string> Message { get; set; }

        public ErrorDialogViewModel()
        {
            Message = new ReactiveProperty<string>();
        }
    }
}
