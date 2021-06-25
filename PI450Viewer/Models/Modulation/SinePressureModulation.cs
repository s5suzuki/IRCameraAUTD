/*
 * File: SinePressureModulation.cs
 * Project: Modulation
 * Created Date: 05/06/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 05/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

namespace PI450Viewer.Models.Modulation
{
    public class SinePressureModulation
    {
        public int Frequency { get; set; }
        public double Amp { get; set; }
        public double Offset { get; set; }

        public SinePressureModulation() { }

        public SinePressureModulation(int frequency, double amp, double offset)
        {
            Frequency = frequency;
            Amp = amp;
            Offset = offset;
        }

        public AUTD3Sharp.Modulation ToModulation() => AUTD3Sharp.Modulation.SinePressure(Frequency, Amp, Offset);
    }
}
