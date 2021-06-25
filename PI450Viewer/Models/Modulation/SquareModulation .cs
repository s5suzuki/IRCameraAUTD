/*
 * File: SquareModulation .cs
 * Project: Modulation
 * Created Date: 03/06/2021
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
    public class SquareModulation
    {
        public int Frequency { get; set; }
        public byte Low { get; set; }
        public byte High { get; set; }

        public SquareModulation() { }

        public SquareModulation(int frequency, byte low, byte high)
        {
            Frequency = frequency;
            Low = low;
            High = high;
        }

        public AUTD3Sharp.Modulation ToModulation() => AUTD3Sharp.Modulation.Square(Frequency, Low, High);
    }
}
