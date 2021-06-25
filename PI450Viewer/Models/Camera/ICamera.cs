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
