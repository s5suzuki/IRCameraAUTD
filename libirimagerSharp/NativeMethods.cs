using System;
using System.Runtime.InteropServices;

namespace LibirimagerSharp
{
    internal static class NativeMethods
    {
#pragma warning disable IDE1006

        #region controller
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int evo_irimager_usb_init([MarshalAs(UnmanagedType.LPStr)]string xmlConfigPath, string formatsDefPath, string logFilePath);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int evo_irimager_tcp_init([MarshalAs(UnmanagedType.LPStr)] string ip, int port);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_terminate();
        #endregion

        #region image
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_thermal_image_size(out int w, out int h);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_palette_image_size(out int w, out int h);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_thermal_image(out int w, out int h, ushort[,] data);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_palette_image(out int w, out int h, IntPtr data);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_get_thermal_palette_image(int w_t, int h_t, ushort[,] data_t, int w_p, int h_p, IntPtr data_p);
        #endregion

        #region option
        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_palette(int id);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_palette_scale(int scale);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_shutter_mode(int mode);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_trigger_shutter_flag();

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_temperature_range(int t_min, int t_max);

        [DllImport("libirimager.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int evo_irimager_set_radiation_parameters(float emissivity, float transmissivity, float tAmbient = -999.0f);
        #endregion
#pragma warning restore IDE1006
    }
}
