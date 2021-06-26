using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace libirimagerNet
{

    /// <summary>
    /// OptrisColoringPalettes
    /// </summary>
    public enum OptrisColoringPalette
    {
        AlarmBlue = 1,
        AlarmBlueHi = 2,
        GrayBw = 3,
        GrayWb = 4,
        AlarmGreen = 5,
        Iron = 6,
        IronHi = 7,
        Medical = 8,
        Rainbow = 9,
        RainbowHi = 10,
        AlarmRed = 11
    }

    /// <summary>
    /// OptrisPaletteScalingMethods
    /// </summary>
    public enum OptrisPaletteScalingMethod
    {
        Manual = 1,
        MinMax = 2,
        Sigma1 = 3,
        Sigma3 = 4
    }

    /// <summary>
    /// IRImager Interface for USB or TCP-Daemon connection
    /// </summary>
    public class IrDirectInterface
    {
        #region fields

        private static IrDirectInterface _instance;
        private bool _isAutomaticShutterActive;
        #endregion

        #region ctor

        #endregion

        #region properties

        /// <summary>
        /// Singleton Instance for Access
        /// </summary>
        public static IrDirectInterface Instance => _instance ??= new IrDirectInterface();

        /// <summary>
        /// Activate or deactivates the automatic shutter
        /// </summary>
        /// <exception cref="System.Exception">Thrown on error</exception>
        public bool IsAutomaticShutterActive
        {
            get => _isAutomaticShutterActive;
            set
            {
                if (_isAutomaticShutterActive == value) return;
                _isAutomaticShutterActive = value;

                CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_set_shutter_mode(InstanceId, value ? 1 : 0));
            }
        }

        /// <summary>
        /// Returns current connection state
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Returns unique instance id
        /// </summary>
        public uint InstanceId { get; private set; }

        #endregion

        #region public methodes

        /// <summary>
        /// Connected to this computer via USB
        /// </summary>
        /// <param name="xmlConfigPath">Path to xml config</param>
        /// <param name="formatsDefPath">Path to folder containing formats.def (for default path use: "")</param>
        /// <param name="logFilePath">Path for logfile (for default path use: "")</param>
        /// <exception cref="System.Exception">Thrown on error</exception>
        public void Connect(string xmlConfigPath, string formatsDefPath = "", string logFilePath = "")
        {
            if (!File.Exists(xmlConfigPath))
            {
                throw new ArgumentException("XML Config file doesn't exist: " + xmlConfigPath, nameof(xmlConfigPath));
            }

            if (formatsDefPath.Length > 0 && !File.Exists(formatsDefPath))
            {
                throw new ArgumentException("Format Definition file doesn't exist: " + xmlConfigPath, nameof(formatsDefPath));
            }

            int error;

            if ((error = IrDirectInterfaceInvoke.evo_irimager_multi_usb_init(out var camId, xmlConfigPath, formatsDefPath, logFilePath ?? "")) < 0)
            {
                throw new Exception($"Error at camera init: {error}");
            }

            InstanceId = camId;
            IsConnected = true;
            IsAutomaticShutterActive = true;
        }


        /// <summary>
        /// Initializes the TCP connection to the daemon process (non-blocking)
        /// </summary>
        /// <param name="hostname">Hostname or IP-Address of the machine where the daemon process is running ("localhost" can be resolved)</param>
        /// <param name="port">Port of daemon, default 1337</param>
        /// <exception cref="System.Exception">Thrown on error</exception>
        public void Connect(string hostname, int port)
        {
            int error;

            if ((error = IrDirectInterfaceInvoke.evo_irimager_multi_tcp_init(out var camId, hostname, port)) < 0)
            {
                throw new Exception($"Error at camera init: {error}");
            }

            InstanceId = camId;
            IsConnected = true;
            IsAutomaticShutterActive = true;
        }


        /// <summary>
        /// Disconnects the camera, either connected via USB or TCP
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected) return;
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_terminate(InstanceId));

            IsConnected = false;
        }

        /// <summary>
        ///  Accessor to thermal image.
        ///  Conversion to temperature values are to be performed as follows:
        ///  t = ((double)data[row,column] - 1000.0) / 10.0
        /// </summary>
        /// <returns>Thermal Image as ushort[height, width]</returns>
        /// <exception cref="System.Exception">Thrown on error</exception>
        public Tuple<ushort[,], EvoIrFrameMetadata> GetThermalImage()
        {
            CheckConnectionState();
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_get_thermal_image_size(InstanceId, out var width, out var height));
            var buffer = new ushort[height, width];
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_get_thermal_image_metadata(InstanceId, out width, out height, buffer, out var metadata));
            return new Tuple<ushort[,], EvoIrFrameMetadata>(buffer, metadata);
        }

        /// <summary>
        /// Accessor to false color coded palette image
        /// </summary>
        /// <returns>RGB palette image</returns>
        /// /// <exception cref="System.Exception">Thrown on error</exception>
        public Tuple<Bitmap, EvoIrFrameMetadata> GetPaletteImage()
        {
            CheckConnectionState();
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_get_palette_image_size(InstanceId, out var width, out var height));

            var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            var bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);

            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_get_palette_image_metadata(InstanceId, out width, out height, bmpData.Scan0, out var metadata));
            image.UnlockBits(bmpData);
            return new Tuple<Bitmap, EvoIrFrameMetadata>(image, metadata);
        }


        /// <summary>
        /// Accessor to false color coded palette image and thermal image from same frame
        /// </summary>
        /// <returns cref="ThermalPaletteImage">False color coded palette and thermal image</returns>
        /// <exception cref="System.Exception">Thrown on error</exception>
        public ThermalPaletteImage GetThermalPaletteImage()
        {
            CheckConnectionState();

            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_get_palette_image_size(InstanceId, out var paletteWidth, out var paletteHeight));
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_get_thermal_image_size(InstanceId, out var thermalWidth, out var thermalHeight));

            var paletteImage = new Bitmap(paletteWidth, paletteHeight, PixelFormat.Format24bppRgb);
            var thermalImage = new ushort[thermalHeight, thermalWidth];

            var bmpData = paletteImage.LockBits(new Rectangle(0, 0, paletteImage.Width, paletteImage.Height), ImageLockMode.WriteOnly, paletteImage.PixelFormat);

            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_get_thermal_palette_image_metadata(
                InstanceId,
                thermalWidth, thermalHeight, thermalImage,
                paletteImage.Width, paletteImage.Height, bmpData.Scan0,
                out var metadata));

            paletteImage.UnlockBits(bmpData);

            return new ThermalPaletteImage(thermalImage, paletteImage, metadata);
        }

        /// <summary>
        /// Sets palette format and scaling method.
        /// </summary>
        /// <param name="format">Palette format</param>
        /// <param name="scale">Scaling method</param>
        public void SetPaletteFormat(OptrisColoringPalette format, OptrisPaletteScalingMethod scale)
        {
            CheckConnectionState();

            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_set_palette(InstanceId, (int)format));
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_set_palette_scale(InstanceId, (int)scale));
        }

        /// <summary>
        /// Only available in OptrisPaletteScalingMethod.Manual palette scale mode. Sets palette manual scaling temperature range.
        /// </summary>
        /// <param name="minTemp">Minimum temperature</param>
        /// <param name="maxTemp">Maximum temperature</param>
        public void SetPaletteManualTemperatureRange(float minTemp, float maxTemp)
        {
            CheckConnectionState();
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_set_palette_manual_temp_range(InstanceId, minTemp, maxTemp));
        }

        /// <summary>
        /// Sets the minimum and maximum temperature range to the camera (also configurable in xml-config)
        /// </summary>
        /// <param name="tMin"></param>
        /// <param name="tMax"></param>
        public void SetTemperatureRange(int tMin, int tMax)
        {
            CheckConnectionState();
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_set_temperature_range(InstanceId, tMin, tMax));
        }

        /// <summary>
        /// Triggers a shutter flag cycle
        /// </summary>
        public void TriggerShutterFlag()
        {
            CheckConnectionState();
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_trigger_shutter_flag(InstanceId));
        }

        /// <summary>
        /// sets radiation properties, i.e. emissivity and transmissivity parameters (not implemented for TCP connection, usb mode only)
        /// </summary>
        /// <param name="emissivity">emissivity emissivity of observed object [0;1]</param>
        /// <param name="transmissivity">transmissivity transmissivity of observed object [0;1]</param>
        /// <param name="tAmbient">tAmbient ambient temperature, setting invalid values (below -273,15 degrees) forces the library to take its own measurement values.</param>
        public void SetRadiationParameters(float emissivity, float transmissivity, float tAmbient = -999.0f)
        {
            if (emissivity < 0 || emissivity > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(emissivity), "Valid range is 0..1");
            }
            if (transmissivity < 0 || transmissivity > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(transmissivity), "Valid range is 0..1");
            }

            CheckConnectionState();
            CheckResult(IrDirectInterfaceInvoke.evo_irimager_multi_set_radiation_parameters(InstanceId, emissivity, transmissivity, tAmbient));
        }


        #endregion

        #region private methodes

        private static void CheckResult(int result)
        {
            if (result < 0)
            {
                throw new Exception($"Internal camera error: {result}");
            }
        }

        private void CheckConnectionState()
        {
            if (!IsConnected)
            {
                throw new Exception("Camera is disconnected. Please connect first.");
            }
        }

        #endregion

        ~IrDirectInterface()
        {
            if (IsConnected)
            {
                Disconnect();
            }
        }
    }
}
