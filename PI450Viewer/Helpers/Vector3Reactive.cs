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
        public ReactiveProperty<int> No { get; }
        public ReactiveProperty<double> X { get; }
        public ReactiveProperty<double> Y { get; }
        public ReactiveProperty<double> Z { get; }

        public Vector3Reactive(int no)
        {
            No = new ReactiveProperty<int>(no);
            X = new ReactiveProperty<double>();
            Y = new ReactiveProperty<double>();
            Z = new ReactiveProperty<double>();
        }

        public Vector3Reactive(int no, Vector3Class v)
        {
            No = new ReactiveProperty<int>(no);
            X = new ReactiveProperty<double>(v.X);
            Y = new ReactiveProperty<double>(v.Y);
            Z = new ReactiveProperty<double>(v.Z);
        }

        public Vector3Class ToVector3() => new Vector3Class(X.Value, Y.Value, Z.Value);
    }
}
