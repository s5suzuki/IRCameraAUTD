/*
 * File: AUTDSettings.cs
 * Project: Models
 * Created Date: 29/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 24/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using PI450Viewer.Helpers;
using PI450Viewer.Models.Gain;
using PI450Viewer.Models.Modulation;
using AUTD3Sharp;
using Reactive.Bindings;

namespace PI450Viewer.Models
{
    [DataContract]
    public class GeometrySetting
    {
        [DataMember]
        public int No { get; set; }
        [DataMember]
        public double X { get; set; }
        [DataMember]
        public double Y { get; set; }
        [DataMember]
        public double Z { get; set; }
        [DataMember]
        public double RotateZ1 { get; set; }
        [DataMember]
        public double RotateY { get; set; }
        [DataMember]
        public double RotateZ2 { get; set; }

        public GeometrySetting() { }

        public GeometrySetting(GeometrySettingReactive obj)
        {
            No = obj.No.Value;
            X = obj.X.Value;
            Y = obj.Y.Value;
            Z = obj.Z.Value;
            RotateZ1 = obj.RotateZ1.Value;
            RotateY = obj.RotateY.Value;
            RotateZ2 = obj.RotateZ2.Value;
        }
    }

    public class GeometrySettingReactive : ReactivePropertyBase
    {
        public ReactivePropertySlim<int> No { get; }
        public ReactivePropertySlim<double> X { get; }
        public ReactivePropertySlim<double> Y { get; }
        public ReactivePropertySlim<double> Z { get; }
        public ReactivePropertySlim<double> RotateZ1 { get; }
        public ReactivePropertySlim<double> RotateY { get; }
        public ReactivePropertySlim<double> RotateZ2 { get; }

        public GeometrySettingReactive(int id)
        {
            No = new ReactivePropertySlim<int>(id);
            X = new ReactivePropertySlim<double>();
            Y = new ReactivePropertySlim<double>();
            Z = new ReactivePropertySlim<double>();
            RotateZ1 = new ReactivePropertySlim<double>();
            RotateY = new ReactivePropertySlim<double>();
            RotateZ2 = new ReactivePropertySlim<double>();
        }

        public GeometrySettingReactive(GeometrySetting obj)
        {
            No = new ReactivePropertySlim<int>(obj.No);
            X = new ReactivePropertySlim<double>(obj.X);
            Y = new ReactivePropertySlim<double>(obj.Y);
            Z = new ReactivePropertySlim<double>(obj.Z);
            RotateZ1 = new ReactivePropertySlim<double>(obj.RotateZ1);
            RotateY = new ReactivePropertySlim<double>(obj.RotateY);
            RotateZ2 = new ReactivePropertySlim<double>(obj.RotateZ2);
        }
    }

    public enum LinkSelect
    {
        SOEM,
        TwinCAT
    }

    public enum GainSelect
    {
        FocalPoint,
        BesselBeam,
        Holo,
        PlaneWave,
        TransducerTest
    }

    public enum ModulationSelect
    {
        Sine,
        Static,
        SinePressure,
        Square
    }

    [DataContract]
    public class AUTDSettings : ReactivePropertyBase
    {
        private static Lazy<AUTDSettings> _lazy = new Lazy<AUTDSettings>(() => new AUTDSettings());
        public static AUTDSettings Instance { get => _lazy.Value; set => _lazy = new Lazy<AUTDSettings>(() => value); }

        [JsonIgnore]
        public ObservableCollectionWithItemNotify<GeometrySettingReactive> GeometriesReactive { get; internal set; }

        [DataMember]
        public GeometrySetting[]? Geometries { get; set; }

        [DataMember]
        public LinkSelect LinkSelected { get; set; }
        [DataMember]
        public string InterfaceName { get; set; }

        [DataMember]
        public uint CycleTicks { get; set; } = 1;

        [DataMember]
        public GainSelect GainSelect { get; set; }
        [DataMember]
        public ModulationSelect ModulationSelect { get; set; }

        [DataMember] public FocalPoint Focus { get; set; } = new FocalPoint(90, 70, 150);
        [DataMember] public BesselBeam Bessel { get; set; } = new BesselBeam(90, 70, 0, 0, 0, 1, AUTD.Pi / 10);
        [DataMember] public Holo Holo { get; set; } = new Holo();
        [DataMember] public PlaneWave PlaneWave { get; set; } = new PlaneWave(0, 0, 1);
        [DataMember] public TransducerTest TransducerTest { get; set; } = new TransducerTest(0, 0xFF, 0);

        [DataMember] public SineModulation Sine { get; set; } = new SineModulation(150, 1.0, 0.5);
        [DataMember] public StaticModulation Static { get; set; } = new StaticModulation(0xFF);
        [DataMember] public SinePressureModulation SinePressure { get; set; } = new SinePressureModulation(150, 1.0, 0.5);
        [DataMember] public SquareModulation Square { get; set; } = new SquareModulation(150, 0x00, 0xFF);

        [DataMember] public Seq Seq { get; set; } = new Seq();

        private AUTDSettings()
        {
            InterfaceName = string.Empty;
            GeometriesReactive = new ObservableCollectionWithItemNotify<GeometrySettingReactive>();
            Geometries = null;
            LinkSelected = LinkSelect.SOEM;
        }

        internal void Store()
        {
            Instance.Geometries = Instance.GeometriesReactive.Select(g => new GeometrySetting(g)).ToArray();
            Instance.Holo.HoloSettings = Instance.Holo.HoloSettingsReactive.Select(s => new HoloSetting(s)).ToArray();
            Instance.Seq.Points = Instance.Seq.PointsReactive.Select(s => s.ToVector3()).ToArray();
        }

        internal void Load()
        {
            Instance.GeometriesReactive = new ObservableCollectionWithItemNotify<GeometrySettingReactive>();
            if (Instance.Geometries == null) return;
            foreach (var geometry in Instance.Geometries) Instance.GeometriesReactive.Add(new GeometrySettingReactive(geometry));

            Instance.Holo.HoloSettingsReactive = new ObservableCollectionWithItemNotify<HoloSettingReactive>();
            if (Instance.Holo.HoloSettings == null) return;
            foreach (var s in Instance.Holo.HoloSettings) Instance.Holo.HoloSettingsReactive.Add(new HoloSettingReactive(s));

            Instance.Seq.PointsReactive = new ObservableCollectionWithItemNotify<Vector3Reactive>();
            if (Instance.Seq.Points == null) return;
            foreach (var (s, i) in Instance.Seq.Points.Select((s, i) => (s, i))) Instance.Seq.PointsReactive.Add(new Vector3Reactive(i, s));
        }
    }
}
