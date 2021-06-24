/*
 * File: SineModulation.cs
 * Project: Modulation
 * Created Date: 06/05/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 03/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

namespace PI450Viewer.Models.Modulation
{
    public class SineModulation
    {
        public int Frequency { get; set; }
        public double Amp { get; set; }
        public double Offset { get; set; }

        public SineModulation() { }

        public SineModulation(int frequency, double amp, double offset)
        {
            Frequency = frequency;
            Amp = amp;
            Offset = offset;
        }

        public AUTD3Sharp.Modulation ToModulation() => AUTD3Sharp.Modulation.Sine(Frequency, Amp, Offset);
    }
}
