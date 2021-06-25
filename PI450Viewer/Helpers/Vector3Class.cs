/*
 * File: Vector3Class.cs
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

namespace PI450Viewer.Helpers
{
    public class Vector3Class
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3Class()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Vector3Class(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
