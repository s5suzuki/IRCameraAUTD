using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using libirimagerNet;

namespace PI450Viewer.Models.Camera
{
    public class DebugCamera : ICamera
    {
        private int _t = 1000;
        private int _v = 1;

        public void SetPaletteFormat(OptrisColoringPalette coloring, OptrisPaletteScalingMethod scaling)
        {
        }

        public void Connect(string xml)
        {
        }

        public void Disconnect()
        {
        }

        public ThermalPaletteImage GrabImage()
        {
            const int width = 382;
            const int height = 288;

            _t += _v;
            if (_t < 1000) _v *= -1;
            if (_t >= 1300) _v *= -1;

            var thermal = new ushort[height, width];
            for (var i = 0; i < height; i++)
            {
                var dy = i / (double)height - 0.5;
                for (var j = 0; j < width; j++)
                {
                    var dx = j / (double)width - 0.5;
                    thermal[i, j] = (ushort)(Math.Exp(-(dx * dx + dy * dy) / 0.2) * _t);
                }
            }

            Bitmap image = new Bitmap(width, height);
            var rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb);
            var ptr = bmpData.Scan0;
            const int bytes = width * height * 4;
            byte[] rgbValues = new byte[bytes];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var idx = i * width + j;
                    rgbValues[4 * idx] = (byte)thermal[i, j];
                    rgbValues[4 * idx + 1] = (byte)thermal[i, j];
                    rgbValues[4 * idx + 2] = (byte)thermal[i, j];
                    rgbValues[4 * idx + 3] = 0;
                }
            }
            Marshal.Copy(rgbValues, 0, ptr, bytes);
            image.UnlockBits(bmpData);

            Thread.Sleep(10);

            return new ThermalPaletteImage(thermal, image);
        }
    }
}