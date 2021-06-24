/*
 * File: TransducerTest.cs
 * Project: Gain
 * Created Date: 30/04/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 03/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

namespace PI450Viewer.Models.Gain
{
    public class TransducerTest : IGain
    {
        public int Index { get; set; }
        public byte Duty { get; set; }
        public byte Phase { get; set; }

        public TransducerTest() { }

        public TransducerTest(int index, byte duty, byte phase)
        {
            Index = index;
            Duty = duty;
            Phase = phase;
        }

        public AUTD3Sharp.Gain ToGain() => AUTD3Sharp.Gain.TransducerTest(Index, Duty, Phase);
    }
}
