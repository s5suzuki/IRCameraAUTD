/*
 * File: PI450.cs
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
    public class Pi450 : ICamera
    {
        private bool _isConnected;

        public void SetPaletteFormat(OptrisColoringPalette coloring, OptrisPaletteScalingMethod scaling)
        {
            if (!_isConnected) return;
            IrDirectInterface.Instance.SetPaletteFormat(coloring, scaling);
        }

        public void SetPaletteManualRange(double min, double max)
        {
            if (!_isConnected) return;
            IrDirectInterface.Instance.SetPaletteManualTemperatureRange((float)min, (float)max);
        }

        public void Connect(string xml)
        {
            IrDirectInterface.Instance.Connect(xml);
            _isConnected = true;
        }

        public void Disconnect()
        {
            IrDirectInterface.Instance.Disconnect();
            _isConnected = false;
        }

        public ThermalPaletteImage GrabImage()
        {
            return IrDirectInterface.Instance.GetThermalPaletteImage();
        }
    }
}
