﻿using System.Drawing;
using libirimagerNet;

namespace PI450Viewer.Models.Camera
{
    public class Pi450 : ICamera
    {
        public void SetPaletteFormat(OptrisColoringPalette coloring, OptrisPaletteScalingMethod scaling)
        {
            IrDirectInterface.Instance.SetPaletteFormat(coloring, scaling);
        }

        public void Connect(string xml)
        {
            IrDirectInterface.Instance.Connect(xml);
        }

        public void Disconnect()
        {
            IrDirectInterface.Instance.Disconnect();
        }

        public ThermalPaletteImage GrabImage()
        {
            return IrDirectInterface.Instance.ThermalPaletteImage;
        }
    }
}
