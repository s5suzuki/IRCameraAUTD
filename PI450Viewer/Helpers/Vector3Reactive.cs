/*
 * File: Vector3Reactive.cs
 * Project: Helpers
 * Created Date: 06/05/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 03/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using Reactive.Bindings;

namespace PI450Viewer.Helpers
{
    public class Vector3Reactive : ReactivePropertyBase
    {
        public ReactivePropertySlim<int> No { get; }
        public ReactivePropertySlim<double> X { get; }
        public ReactivePropertySlim<double> Y { get; }
        public ReactivePropertySlim<double> Z { get; }

        public Vector3Reactive(int no)
        {
            No = new ReactivePropertySlim<int>(no);
            X = new ReactivePropertySlim<double>();
            Y = new ReactivePropertySlim<double>();
            Z = new ReactivePropertySlim<double>();
        }

        public Vector3Reactive(int no, Vector3Class v)
        {
            No = new ReactivePropertySlim<int>(no);
            X = new ReactivePropertySlim<double>(v.X);
            Y = new ReactivePropertySlim<double>(v.Y);
            Z = new ReactivePropertySlim<double>(v.Z);
        }

        public Vector3Class ToVector3() => new Vector3Class(X.Value, Y.Value, Z.Value);
    }
}
