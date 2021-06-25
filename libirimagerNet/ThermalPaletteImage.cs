using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evocortex.irDirectBinding {

    /// <summary>
    /// Container for thermal and false color palette image.
    /// </summary>
    public class ThermalPaletteImage {

        /// <summary>
        /// Constructor for thermal and false color palette image.
        /// </summary>
        /// <param name="thermalImage">The thermal image</param>
        /// <param name="paletteImage">The palette image</param>
        public ThermalPaletteImage(ushort[,] thermalImage, Bitmap paletteImage, EvoIRFrameMetadata irFrameMetadata) {
            ThermalImage = thermalImage;
            PaletteImage = paletteImage;
            IRFrameMetadata = irFrameMetadata;
        }

        /// <summary>
        ///  Accessor to thermal image.
        ///  Conversion to temperature values are to be performed as follows:
        ///  t = ((double)data[row,column] - 1000.0) / 10.0
        /// </summary>
        /// <returns>Thermal Image as ushort[height, width]</returns>
        public ushort[,] ThermalImage { get; private set; }

        /// <summary>
        /// Accessor to false color coded palette image
        /// </summary>
        /// <returns>RGB palette image</returns>
        public Bitmap PaletteImage { get; private set; }


        /// <summary>
        /// Accessor to ir frame metadata
        /// </summary>
        /// <returns>IR frame metadata</returns>
        public EvoIRFrameMetadata IRFrameMetadata { get; private set; }
    }
}
