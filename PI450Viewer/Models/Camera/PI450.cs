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
