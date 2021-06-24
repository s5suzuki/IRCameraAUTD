/*
 * File: BesselBeam.cs
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
    public class BesselBeam : IGain
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double DirX { get; set; }
        public double DirY { get; set; }
        public double DirZ { get; set; }

        public double Theta { get; set; }
        public byte Duty { get; set; }

        public BesselBeam() { }

        public BesselBeam(double x, double y, double z, double dx, double dy, double dz, double theta, byte duty = 0xFF)
        {
            X = x;
            Y = y;
            Z = z;
            DirX = dx;
            DirY = dy;
            DirZ = dz;
            Theta = theta;
            Duty = duty;
        }

        public AUTD3Sharp.Gain ToGain() => AUTD3Sharp.Gain.BesselBeam(new Vector3d(X, Y, Z), new Vector3d(DirX, DirY, DirZ), General.Instance.ConvertAngle(Theta), Duty);
    }
}
