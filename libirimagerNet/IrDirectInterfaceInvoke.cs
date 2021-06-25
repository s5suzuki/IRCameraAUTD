using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Evocortex.irDirectBinding {


    /// <summary>
    /// IR Flag States
    /// </summary>
    public enum EvoIRFlagState
    {
        Open, /*!< Flag is open */
        Close,/*!< Flag is closed */
        Opening, /*!< Flag is opening */
        Closing, /*!< Flag is closing */
        Error /*!< Flag Error */
    }

    /// <summary>
    /// Additional frame metadata
    /// </summary>
    /// 
    [StructLayout(LayoutKind.Sequential)]
    public struct EvoIRFrameMetadata
    {
        uint counter;     /*!< Consecutively numbered for each received frame */
        uint counterHW;   /*!< Hardware counter received from device */
        long timestamp;      /*!< Time stamp in UNITS (10000000 per second) */
        long timestampMedia;
        EvoIRFlagState flagState; /*!< State of shutter flag at capturing time */
        float tempChip;           /*!< Chip temperature */
        float tempFlag;           /*!< Shutter flag temperature */
        float tempBox;            /*!< Temperature inside camera housing */
    }

    internal class IrDirectInterfaceInvoke {
        /**
         * @brief Initializes an IRImager instance connected to this computer via USB
         * @param[in] xml_config path to xml config
         * @param[in] formats_def path to folder containing formants.def (for default path use: "")
         * @param[in] log_file path for log file. Set to null or "" for disable logging.
         * @return 0 on success, -1 on error
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_usb_init(string xml_config, string formats_def, string log_file);

        /**
         * @brief Initializes the TCP connection to the daemon process (non-blocking)
         * @param[in] IP address of the machine where the daemon process is running ("localhost" can be resolved)
         * @param port Port of daemon, default 1337
         * @return  error code: 0 on success, -1 on host not found (wrong IP, daemon not running), -2 on fatal error
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_tcp_init(string ip, int port);

        /**
         * @brief Disconnects the camera, either connected via USB or TCP
         * @return 0 on success, -1 on error
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_terminate();

        /**
         * @brief Accessor to image width and height
         * @param[out] w width
         * @param[out] h height
         * @return 0 on success, -1 on error
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_thermal_image_size(out int w, out int h);

        /**
         * @brief Accessor to width and height of false color coded palette image
         * @param[out] w width
         * @param[out] h height
         * @return 0 on success, -1 on error
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_palette_image_size(out int w, out int h);

        /**
         * @brief Accessor to thermal image by reference
         * Conversion to temperature values are to be performed as follows:
         * t = ((double)data[x] - 1000.0) / 10.0;
         * @param[in] w image width
         * @param[in] h image height
         * @param[out] data pointer to unsigned short array allocate by the user (size of w * h)
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_thermal_image(out int w, out int h, ushort[,] data);

        /**
         * @brief Accessor to an RGB palette image by reference
         * data format: unsigned char array (size 3 * w * h) r,g,b
         * @param[in] w image width
         * @param[in] h image height
         * @param[out] data pointer to unsigned char array allocate by the user (size of 3 * w * h)
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_palette_image(out int w, out int h, IntPtr data);

        /**
         * @brief Accessor to an RGB palette image and a thermal image by reference
         * @param[in] w_t width of thermal image
         * @param[in] h_t height of thermal image
         * @param[out] data_t data pointer to unsigned short array allocate by the user (size of w * h)
         * @param[in] w_p width of palette image (can differ from thermal image width due to striding)
         * @param[in] h_p height of palette image (can differ from thermal image height due to striding)
         * @param[out] data_p data pointer to unsigned char array allocate by the user (size of 3 * w * h)
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_thermal_palette_image(int w_t, int h_t, ushort[,] data_t, int w_p, int h_p, IntPtr data_p);

        /**
         * @brief Accessor to thermal image by reference
         * Conversion to temperature values are to be performed as follows:
         * t = ((double)data[x] - 1000.0) / 10.0;
         * @param[in] w image width
         * @param[in] h image height
         * @param[out] data pointer to unsigned short array allocate by the user (size of w * h)
         * @param[out] metadata pointer to EvoIRFrameMetadata allocate by the user
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_thermal_image_metadata(out int w, out int h, ushort[,] data, out EvoIRFrameMetadata metadata);

        /**
         * @brief Accessor to an RGB palette image by reference
         * data format: unsigned char array (size 3 * w * h) r,g,b
         * @param[in] w image width
         * @param[in] h image height
         * @param[out] data pointer to unsigned char array allocate by the user (size of 3 * w * h)
         * @param[out] metadata pointer to EvoIRFrameMetadata allocate by the user
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_palette_image_metadata(out int w, out int h, IntPtr data, out EvoIRFrameMetadata metadata);

        /**
         * @brief Accessor to an RGB palette image and a thermal image by reference
         * @param[in] w_t width of thermal image
         * @param[in] h_t height of thermal image
         * @param[out] data_t data pointer to unsigned short array allocate by the user (size of w * h)
         * @param[in] w_p width of palette image (can differ from thermal image width due to striding)
         * @param[in] h_p height of palette image (can differ from thermal image height due to striding)
         * @param[out] data_p data pointer to unsigned char array allocate by the user (size of 3 * w * h)
         * @param[out] metadata pointer to EvoIRFrameMetadata allocate by the user
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_thermal_palette_image_metadata(int w_t, int h_t, ushort[,] data_t, int w_p, int h_p, IntPtr data_p, out EvoIRFrameMetadata metadata);

        /**
         * @brief sets palette format to daemon.
         * Defined in IRImager Direct-SDK, see
         * enum EnumOptrisColoringPalette{eAlarmBlue   = 1,
         *                                eAlarmBlueHi = 2,
         *                                eGrayBW      = 3,
         *                                eGrayWB      = 4,
         *                                eAlarmGreen  = 5,
         *                                eIron        = 6,
         *                                eIronHi      = 7,
         *                                eMedical     = 8,
         *                                eRainbow     = 9,
         *                                eRainbowHi   = 10,
         *                                eAlarmRed    = 11 };
         *
         * @param id palette id
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_palette(int id);

        /**
         * @brief sets palette scaling method
         * Defined in IRImager Direct-SDK, see
         * enum EnumOptrisPaletteScalingMethod{eManual = 1,
         *                                     eMinMax = 2,
         *                                     eSigma1 = 3,
         *                                     eSigma3 = 4 };
         * @param scale scaling method id
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_palette_scale(int scale);

        /**
         * @brief Only available in eManual palette scale mode. Sets palette manual scaling temperature range.
         * @param min Minimum temperature
         * @param max Maximum temperature
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_palette_manual_temp_range(float min, float max);

        /**
         * @brief sets shutter flag control mode
         * @param mode 0 means manual control, 1 means automode
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_shutter_mode(int mode);

        /**
         * @brief forces a shutter flag cycle
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_trigger_shutter_flag();

        /**
         * @brief sets the minimum and maximum remperature range to the camera (also configurable in xml-config)
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_temperature_range(int t_min, int t_max);

        /**
         * @brief sets radiation properties, i.e. emissivity and transmissivity parameters (not implemented for TCP connection, usb mode only)
         * @param[in] emissivity emissivity of observed object [0;1]
         * @param[in] transmissivity transmissivity of observed object [0;1]
         * @param[in] tAmbient ambient temperature, setting invalid values (below -273,15 degrees) forces the library to take its own measurement values.
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_radiation_parameters(float emissivity, float transmissivity, float tAmbient = -999.0f);


        /*-------------------------------------------------*/
        /*-------------------- multicam -------------------*/
        /*-------------------------------------------------*/

        /**
         * @brief Initializes an IRImager instance connected to this computer via USB for multiple cameras
         * @param[in] xml_config path to xml config
         * @param[in] formats_def path to folder containing formants.def (for default path use: "")
         * @param[in] log_file path for log file. Set to null or "" for disable logging.
         * @return 0 on success, -1 on error
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_usb_init(out uint outCamId, string xml_config, string formats_def, string log_file);

        /**
         * @brief Initializes the TCP connection to the daemon process (non-blocking) for multiple cameras
         * @param[in] IP address of the machine where the daemon process is running ("localhost" can be resolved)
         * @param port Port of daemon, default 1337
         * @return  error code: 0 on success, -1 on host not found (wrong IP, daemon not running), -2 on fatal error
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_tcp_init(out uint outCamId, string ip, int port);

        /**
         * @brief Disconnects the camera, either connected via USB or TCP for multiple cameras
         * @return 0 on success, -1 on error
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_terminate(uint camId);

        /**
         * @brief Accessor to image width and height for multiple cameras
         * @param[out] w width
         * @param[out] h height
         * @return 0 on success, -1 on error
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_get_thermal_image_size(uint camId, out int w, out int h);

        /**
         * @brief Accessor to width and height of false color coded palette image for multiple cameras
         * @param[out] w width
         * @param[out] h height
         * @return 0 on success, -1 on error
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_get_palette_image_size(uint camId, out int w, out int h);

        /**
         * @brief Accessor to thermal image by reference for multiple cameras
         * Conversion to temperature values are to be performed as follows:
         * t = ((double)data[x] - 1000.0) / 10.0;
         * @param[in] w image width
         * @param[in] h image height
         * @param[out] data pointer to unsigned short array allocate by the user (size of w * h)
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_get_thermal_image(uint camId, out int w, out int h, ushort[,] data);

        /**
         * @brief Accessor to thermal image by reference for multiple cameras
         * Conversion to temperature values are to be performed as follows:
         * t = ((double)data[x] - 1000.0) / 10.0;
         * @param[in] w image width
         * @param[in] h image height
         * @param[out] data pointer to unsigned short array allocate by the user (size of w * h)
         * @param[out] metadata pointer to EvoIRFrameMetadata allocate by the user
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_get_thermal_image_metadata(uint camId, out int w, out int h, ushort[,] data, out EvoIRFrameMetadata metadata);

        /**
         * @brief Accessor to an RGB palette image by reference for multiple cameras
         * data format: unsigned char array (size 3 * w * h) r,g,b
         * @param[in] w image width
         * @param[in] h image height
         * @param[out] data pointer to unsigned char array allocate by the user (size of 3 * w * h)
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_get_palette_image(uint camId, out int w, out int h, IntPtr data);

        /**
         * @brief Accessor to an RGB palette image by reference for multiple cameras
         * data format: unsigned char array (size 3 * w * h) r,g,b
         * @param[in] w image width
         * @param[in] h image height
         * @param[out] data pointer to unsigned char array allocate by the user (size of 3 * w * h)
         * @param[out] metadata pointer to EvoIRFrameMetadata allocate by the user
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_get_palette_image_metadata(uint camId, out int w, out int h, IntPtr data, out EvoIRFrameMetadata metadata);

        /**
         * @brief Accessor to an RGB palette image and a thermal image by reference for multiple cameras
         * @param[in] w_t width of thermal image
         * @param[in] h_t height of thermal image
         * @param[out] data_t data pointer to unsigned short array allocate by the user (size of w * h)
         * @param[in] w_p width of palette image (can differ from thermal image width due to striding)
         * @param[in] h_p height of palette image (can differ from thermal image height due to striding)
         * @param[out] data_p data pointer to unsigned char array allocate by the user (size of 3 * w * h)
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_get_thermal_palette_image(uint camId, int w_t, int h_t, ushort[,] data_t, int w_p, int h_p, IntPtr data_p);


        /**
         * @brief Accessor to an RGB palette image and a thermal image by reference for multiple cameras
         * @param[in] w_t width of thermal image
         * @param[in] h_t height of thermal image
         * @param[out] data_t data pointer to unsigned short array allocate by the user (size of w * h)
         * @param[in] w_p width of palette image (can differ from thermal image width due to striding)
         * @param[in] h_p height of palette image (can differ from thermal image height due to striding)
         * @param[out] data_p data pointer to unsigned char array allocate by the user (size of 3 * w * h)
         * @param[out] metadata pointer to EvoIRFrameMetadata allocate by the user
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_get_thermal_palette_image_metadata(uint camId, int w_t, int h_t, ushort[,] data_t, int w_p, int h_p, IntPtr data_p, out EvoIRFrameMetadata metadata);

        /**
         * @brief sets palette format for multiple cameras.
         * Defined in IRImager Direct-SDK, see
         * enum EnumOptrisColoringPalette{eAlarmBlue   = 1,
         *                                eAlarmBlueHi = 2,
         *                                eGrayBW      = 3,
         *                                eGrayWB      = 4,
         *                                eAlarmGreen  = 5,
         *                                eIron        = 6,
         *                                eIronHi      = 7,
         *                                eMedical     = 8,
         *                                eRainbow     = 9,
         *                                eRainbowHi   = 10,
         *                                eAlarmRed    = 11 };
         *
         * @param id palette id
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_set_palette(uint camId, int paletteId);

        /**
         * @brief sets palette scaling method for multiple cameras
         * Defined in IRImager Direct-SDK, see
         * enum EnumOptrisPaletteScalingMethod{eManual = 1,
         *                                     eMinMax = 2,
         *                                     eSigma1 = 3,
         *                                     eSigma3 = 4 };
         * @param scale scaling method id
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_set_palette_scale(uint camId, int scale);

        /**
         * @brief Only available in eManual palette scale mode. Sets palette manual scaling temperature range for multiple cameras.
         * @param[in] camId Camera instance id  from init to apply this function
         * @param min Minimum temperature
         * @param max Maximum temperature
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_set_palette_manual_temp_range(uint camId, float min, float max);

        /**
         * @brief sets shutter flag control mode for multiple cameras
         * @param mode 0 means manual control, 1 means automode
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_set_shutter_mode(uint camId, int mode);

        /**
         * @brief forces a shutter flag cycle for multiple cameras
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_trigger_shutter_flag(uint camId);

        /**
         * @brief sets the minimum and maximum temperature range to the camera. Only values which are defined in teh optris cali files are supported.  (also configurable in xml-config) for multiple cameras
         * @param[in] camId Camera instance id  from init to apply this function
         * @param[in] t_min Minimal temperature (has to be defined in the optris cali files) 
         * @param[in] t_min Maximal temperature (has to be defined in the optris cali files)
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_set_temperature_range(uint camId, int t_min, int t_max);

        /**
         * @brief sets radiation properties, i.e. emissivity and transmissivity parameters for multiple cameras (not implemented for TCP connection, usb mode only)
         * @param[in] camId Camera instance id  from init to apply this function
         * @param[in] emissivity emissivity of observed object [0;1]
         * @param[in] transmissivity transmissivity of observed object [0;1]
         * @param[in] tAmbient ambient temperature, setting invalid values (below -273,15 degrees) forces the library to take its own measurement values.
         * @return error code: 0 on success, -1 on error, -2 on fatal error (only TCP connection)
         */
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_multi_set_radiation_parameters(uint camId, float emissivity, float transmissivity, float tAmbient = -999.0f);

    }
}
