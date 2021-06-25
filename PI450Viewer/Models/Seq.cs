/*
 * File: Seq.cs
 * Project: Models
 * Created Date: 06/05/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 03/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System.Linq;
using System.Text.Json.Serialization;
using PI450Viewer.Helpers;
using AUTD3Sharp;
using AUTD3Sharp.Utils;

namespace PI450Viewer.Models
{
    public class Seq
    {
        [JsonIgnore]
        public ObservableCollectionWithItemNotify<Vector3Reactive> PointsReactive { get; internal set; }

        public Vector3Class[]? Points { get; set; }

        public double Frequency { get; set; }

        public Seq()
        {
            PointsReactive = new ObservableCollectionWithItemNotify<Vector3Reactive>();
            Points = null;
            Frequency = 1;
        }

        public PointSequence ToPointSequence()
        {
            var seq = PointSequence.Create();
            seq.AddPoints(PointsReactive.Select(s => new Vector3d(s.X.Value, s.Y.Value, s.Z.Value)).ToArray());
            seq.Frequency = Frequency;
            return seq;
        }
    }
}
