/*
 * File: IModulation.cs
 * Project: Modulation
 * Created Date: 06/05/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 06/05/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

namespace PI450Viewer.Models.Modulation
{
    public interface IModulation
    {
        public AUTD3Sharp.Modulation ToModulation();
    }
}
