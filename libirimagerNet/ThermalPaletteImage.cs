using System.Drawing;

namespace libirimagerNet
{

    /// <summary>
    /// Container for thermal and false color palette image.
    /// </summary>
    public class ThermalPaletteImage
    {
        /// <summary>
        /// Constructor for thermal and false color palette image.
        /// </summary>
        /// <param name="thermalImage">The thermal image</param>
        /// <param name="paletteImage">The palette image</param>
        /// <param name="irFrameMetadata">metadata</param>
        public ThermalPaletteImage(ushort[,] thermalImage, Bitmap paletteImage, EvoIrFrameMetadata irFrameMetadata)
        {
            ThermalImage = thermalImage;
            PaletteImage = paletteImage;
            IrFrameMetadata = irFrameMetadata;
        }

        /// <summary>
        ///  Accessor to thermal image.
        ///  Conversion to temperature values are to be performed as follows:
        ///  t = ((double)data[row,column] - 1000.0) / 10.0
        /// </summary>
        /// <returns>Thermal Image as ushort[height, width]</returns>
        public ushort[,] ThermalImage { get; }

        /// <summary>
        /// Accessor to false color coded palette image
        /// </summary>
        /// <returns>RGB palette image</returns>
        public Bitmap PaletteImage { get; }


        /// <summary>
        /// Accessor to ir frame metadata
        /// </summary>
        /// <returns>IR frame metadata</returns>
        public EvoIrFrameMetadata IrFrameMetadata { get; }
    }
}
