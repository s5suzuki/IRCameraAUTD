/*
 * File: FocalPoint.cs
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

using AUTD3Sharp.Utils;

namespace PI450Viewer.Models.Gain
{
    public class FocalPoint : IGain
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public byte Duty { get; set; }

        public FocalPoint() { }

        public FocalPoint(double x, double y, double z, byte duty = 0xFF)
        {
            X = x;
            Y = y;
            Z = z;
            Duty = duty;
        }

        public AUTD3Sharp.Gain ToGain() => AUTD3Sharp.Gain.FocalPoint(new Vector3d(X, Y, Z), Duty);
    }
}
