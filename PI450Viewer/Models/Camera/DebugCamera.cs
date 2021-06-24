using System.Drawing;
using libirimagerNet;

namespace PI450Viewer.Models.Camera
{
    public class DebugCamera: ICamera
    {
        public void SetPaletteFormat(OptrisColoringPalette coloring, OptrisPaletteScalingMethod scaling)
        {
        }

        public void Connect(string xml)
        {
        }

        public void Disconnect()
        {
        }

        public (Bitmap, ushort[,]) GrabImage()
        {
            const int width = 382;
            const int height = 288;

            Bitmap image = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(image);
            graphics.FillRectangle(Brushes.LightGray, graphics.VisibleClipBounds);
            graphics.DrawRectangle(Pens.Black, 50, 50, 50, 50);
            graphics.Dispose();
            return (image, new ushort[width, height]);
        }
    }
}