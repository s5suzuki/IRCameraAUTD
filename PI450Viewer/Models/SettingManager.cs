/*
 * File: SettingManager.cs
 * Project: Models
 * Created Date: 07/04/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 24/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System.IO;
using System.Text.Json;

namespace PI450Viewer.Models
{
    internal static class SettingManager
    {
        public class TotalSetting
        {
            public General General { get; set; }
            public AUTDSettings AUTDSettings { get; set; }
            public ThermalCameraHandler ThermalCameraHandler { get; set; }

            public TotalSetting()
            {
                General = General.Instance;
                AUTDSettings = AUTDSettings.Instance;
                ThermalCameraHandler = ThermalCameraHandler.Instance;
            }
        }

        internal static void SaveSetting(string path)
        {
            AUTDSettings.Instance.Store();
            General.Instance.Store();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var jsonString = JsonSerializer.Serialize(new TotalSetting(), options);
            File.WriteAllText(path, jsonString);
        }

        internal static void LoadSetting(string path)
        {
            var jsonString = File.ReadAllText(path);
            var obj = JsonSerializer.Deserialize<TotalSetting>(jsonString);
            AUTDSettings.Instance = obj.AUTDSettings;
            General.Instance = obj.General;
            ThermalCameraHandler.Instance = obj.ThermalCameraHandler;
            AUTDSettings.Instance.Load();
            General.Instance.Load();
        }
    }
}
