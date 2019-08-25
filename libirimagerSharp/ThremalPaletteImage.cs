using System.Windows.Media.Imaging;

namespace LibirimagerSharp
{
    public class ThermalPaletteImage
    {
        public ThermalPaletteImage(ushort[,] thermalImage, BitmapSource paletteImage)
        {
            ThermalImage = thermalImage;
            PaletteImage = paletteImage;
        }

        public ushort[,] ThermalImage { get; private set; }

        public BitmapSource PaletteImage { get; private set; }
    }
}
