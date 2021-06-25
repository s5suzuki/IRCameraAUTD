/*
 * File: PlaneWave.cs
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
    public class PlaneWave : IGain
    {
        public double DirX { get; set; }
        public double DirY { get; set; }
        public double DirZ { get; set; }

        public byte Duty { get; set; }

        public PlaneWave() { }

        public PlaneWave(double dx, double dy, double dz, byte duty = 0xFF)
        {
            DirX = dx;
            DirY = dy;
            DirZ = dz;
            Duty = duty;
        }

        public AUTD3Sharp.Gain ToGain() => AUTD3Sharp.Gain.PlaneWave(new Vector3d(DirX, DirY, DirZ), Duty);
    }
}
