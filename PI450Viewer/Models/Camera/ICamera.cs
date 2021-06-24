using System.Drawing;
using libirimagerNet;

namespace PI450Viewer.Models.Camera
{
    public interface ICamera
    {
        public void SetPaletteFormat(OptrisColoringPalette coloring, OptrisPaletteScalingMethod scaling);
        public void Connect(string xml);
        public void Disconnect();
        public (Bitmap, ushort[,]) GrabImage();
    }
}