using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

[assembly: CLSCompliant(false)]
namespace LibirimagerSharp
{
    public enum OptrisColoringPalette : int
    {
        None = 0,
        AlarmBlue = 1,
        AlarmBlueHI = 2,
        GrayBW = 3,
        GrayWB = 4,
        AlarmGreen = 5,
        Iron = 6,
        IronHI = 7,
        Medical = 8,
        Rainbow = 9,
        RainbowHI = 10,
        AlarmRed = 11
    }

    public enum OptrisPaletteScalingMethod
    {
        None = 0,
        Manual = 1,
        MinMax = 2,
        Sigma1 = 3,
        Sigma3 = 4
    }

    public class IRDirectInterface
    {
        #region fields

        private static IRDirectInterface _instance;
        private bool _isAutomaticShutterActive;

        #endregion

        #region ctor

        private IRDirectInterface() { }

        ~IRDirectInterface()
        {
            Disconnect();
        }

        #endregion

        #region properties

        public static IRDirectInterface Instance => _instance ?? (_instance = new IRDirectInterface());

        public bool IsAutomaticShutterActive
        {
            get => _isAutomaticShutterActive;

            set
            {
                if (_isAutomaticShutterActive != value)
                {
                    _isAutomaticShutterActive = value;
                    CheckResult(NativeMethods.evo_irimager_set_shutter_mode(value ? 1 : 0));
                }
            }
        }

        public bool IsConnected { get; private set; }

        public ThermalPaletteImage ThermalPaletteImage
        {
            get
            {
                CheckConnectionState();
                PixelFormat pf = PixelFormats.Bgr24;

                CheckResult(NativeMethods.evo_irimager_get_palette_image_size(out int paletteWidth, out int paletteHeight));
                CheckResult(NativeMethods.evo_irimager_get_thermal_image_size(out int thermalWidth, out int thermalHeight));

                int paletteStride = (paletteWidth * pf.BitsPerPixel + 7) / 8;
                byte[] rawImage = new byte[paletteStride * paletteHeight];

                BitmapSource paletteImage = BitmapSource.Create(paletteWidth, paletteHeight, 96d, 96d, pf, null, rawImage, paletteStride);
                ushort[,] thermalImage = new ushort[thermalHeight, thermalWidth];

                WriteableBitmap wpaletteImage = new WriteableBitmap(paletteImage);
                wpaletteImage.Lock();
                IntPtr bmpData = wpaletteImage.BackBuffer;
                CheckResult(NativeMethods.evo_irimager_get_thermal_palette_image(thermalWidth, thermalHeight, thermalImage, paletteImage.PixelWidth, paletteImage.PixelHeight, bmpData));

                wpaletteImage.AddDirtyRect(new Int32Rect(0, 0, paletteWidth, paletteHeight));
                wpaletteImage.Unlock();
                wpaletteImage.Freeze();

                return new ThermalPaletteImage(thermalImage, wpaletteImage);
            }
        }

        public BitmapSource PaletteImage
        {
            get
            {
                PixelFormat pf = PixelFormats.Bgr24;
                CheckConnectionState();
                CheckResult(NativeMethods.evo_irimager_get_palette_image_size(out int width, out int height));

                int stride = (width * pf.BitsPerPixel + 7) / 8;
                byte[] rawImage = new byte[stride * height];

                BitmapSource image = BitmapSource.Create(width, height, 96d, 96d, pf, null, rawImage, stride);
                WriteableBitmap wimage = new WriteableBitmap(image);

                wimage.Lock();
                IntPtr buf = wimage.BackBuffer;
                CheckResult(NativeMethods.evo_irimager_get_palette_image(out width, out height, buf));
                wimage.AddDirtyRect(new Int32Rect(0, 0, width, height));
                wimage.Unlock();
                wimage.Freeze();

                return image;
            }
        }
        #endregion

        public void Connect(string xmlConfigPath)
        {
            if (!File.Exists(xmlConfigPath))
            {
                throw new ArgumentException("XML Config file doesn't exist: " + xmlConfigPath, nameof(xmlConfigPath));
            }

            int error = NativeMethods.evo_irimager_usb_init(xmlConfigPath, "", "");
            if (error < 0)
            {
                throw new IOException($"Error at camera init: {error}");
            }

            IsConnected = true;
            IsAutomaticShutterActive = true;
        }

        public void Connect(string hostName, int port)
        {
            int error = NativeMethods.evo_irimager_tcp_init(hostName, port);
            if (error < 0)
            {
                throw new IOException($"Error at camera init: {error}");
            }

            IsConnected = true;
            IsAutomaticShutterActive = true;
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                CheckResult(NativeMethods.evo_irimager_terminate());
                IsConnected = false;
            }
        }

        public ushort[,] GetThermalImage()
        {
            CheckConnectionState();
            CheckResult(NativeMethods.evo_irimager_get_thermal_image_size(out int width, out int height));
            ushort[,] buffer = new ushort[height, width];
            CheckResult(NativeMethods.evo_irimager_get_thermal_image(out width, out height, buffer));
            return buffer;
        }

        public void SetPaletteFormat(OptrisColoringPalette format, OptrisPaletteScalingMethod scale)
        {
            CheckConnectionState();
            CheckResult(NativeMethods.evo_irimager_set_palette((int)format));
            CheckResult(NativeMethods.evo_irimager_set_palette_scale((int)scale));
        }

        public void SetTemperatureRange(int min, int max)
        {
            CheckConnectionState();
            CheckResult(NativeMethods.evo_irimager_set_temperature_range(min, max));
        }

        public void TriggerShutter()
        {
            CheckConnectionState();
            CheckResult(NativeMethods.evo_irimager_trigger_shutter_flag());
        }

        public void SetRadiationParameters(float emissivity, float transmissivity)
        {
            SetRadiationParameters(emissivity, transmissivity, -999.0f);
        }

        public void SetRadiationParameters(float emissivity, float transmissivity, float ambient)
        {
            if (emissivity < 0 || 1 < emissivity)
            {
                throw new ArgumentOutOfRangeException(nameof(emissivity), "Valid range is 0..1");
            }

            if (transmissivity < 0 || 1 < transmissivity)
            {
                throw new ArgumentOutOfRangeException(nameof(transmissivity), "Valid range is 0..1");
            }

            CheckConnectionState();
            CheckResult(NativeMethods.evo_irimager_set_radiation_parameters(emissivity, transmissivity, ambient));
        }


        private static void CheckResult(int result)
        {
            if (result < 0)
            {
                throw new IOException($"Internal camera error: {result}");
            }
        }

        private void CheckConnectionState()
        {
            if (!IsConnected)
            {
                throw new IOException($"Camera is disconnected. Please connect first.");
            }
        }
    }
}
