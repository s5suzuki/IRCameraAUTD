/*
 * File: ICamera.cs
 * Project: Camera
 * Created Date: 25/06/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 25/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using Evocortex.irDirectBinding;

namespace PI450Viewer.Models.Camera
{
    public interface ICamera
    {
        public void SetPaletteFormat(OptrisColoringPalette coloring, OptrisPaletteScalingMethod scaling);
        public void SetPaletteManualRange(double min, double max);
        public void Connect(string xml);
        public void Disconnect();
        public ThermalPaletteImage GrabImage();
    }
}
