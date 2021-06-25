/*
 * File: AUTDHandler.cs
 * Project: Models
 * Created Date: 31/03/2021
 * Author: Shun Suzuki
 * -----
 * Last Modified: 05/06/2021
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2021 Hapis Lab. All rights reserved.
 * 
 */

using System;
using System.Linq;
using PI450Viewer.Helpers;
using AUTD3Sharp;
using AUTD3Sharp.Utils;
using Reactive.Bindings;

namespace PI450Viewer.Models
{
    internal class AUTDHandler : ReactivePropertyBase, IDisposable
    {
        private static readonly Lazy<AUTDHandler> Lazy = new Lazy<AUTDHandler>(() => new AUTDHandler());
        public static AUTDHandler Instance => Lazy.Value;

        private readonly AUTD _autd;
        public ReactivePropertySlim<bool> IsOpen { get; }
        public ReactivePropertySlim<bool> IsStarted { get; }
        public ReactivePropertySlim<bool> IsPaused { get; }

        private AUTD3Sharp.Gain _gain;
        private AUTD3Sharp.Modulation _mod;
        private PointSequence _seq;


        private AUTDHandler()
        {
            _autd = new AUTD();
            IsOpen = new ReactivePropertySlim<bool>();
            IsStarted = new ReactivePropertySlim<bool>();
            IsPaused = new ReactivePropertySlim<bool>();

            _gain = AUTD3Sharp.Gain.Null();
            _mod = AUTD3Sharp.Modulation.Static();
            _seq = PointSequence.Create();
        }

        private void AddDevices()
        {
            _autd.ClearDevices();
            foreach (var item in AUTDSettings.Instance.GeometriesReactive)
            {
                _autd.AddDevice(new Vector3d(item.X.Value, item.Y.Value, item.Z.Value), new Vector3d(item.RotateZ1.Value, item.RotateY.Value, item.RotateZ2.Value));
            }
        }

        public string? Open()
        {
            try
            {
                AddDevices();

                var link = AUTDSettings.Instance.LinkSelected switch
                {
                    LinkSelect.SOEM =>
                        Link.SOEM(
                            AUTDSettings.Instance.InterfaceName.Split(',').LastOrDefault()?.Trim() ?? string.Empty,
                            _autd.NumDevices, AUTDSettings.Instance.CycleTicks),
                    LinkSelect.TwinCAT =>
                        Link.TwinCAT(),
                    _ => throw new NotImplementedException()
                };

                if (!_autd.Open(link)) return AUTD.LastError;

                IsOpen.Value = true;
                _autd.Clear();
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public void Close()
        {
            _autd.Close();
            IsOpen.Value = false;
        }

        public void AppendGain()
        {
            var instance = AUTDSettings.Instance;
            _gain = instance.GainSelect switch
            {
                GainSelect.FocalPoint => instance.Focus.ToGain(),
                GainSelect.BesselBeam => instance.Bessel.ToGain(),
                GainSelect.Holo => instance.Holo.ToGain(),
                GainSelect.PlaneWave => instance.PlaneWave.ToGain(),
                GainSelect.TransducerTest => instance.TransducerTest.ToGain(),
                _ => throw new ArgumentOutOfRangeException()
            };
            AUTDSettings.Instance.GainMode = true;
        }

        public void AppendModulation()
        {
            var instance = AUTDSettings.Instance;
            _mod = instance.ModulationSelect switch
            {
                ModulationSelect.Static => instance.Static.ToModulation(),
                ModulationSelect.Sine => instance.Sine.ToModulation(),
                ModulationSelect.SinePressure => instance.Sine.ToModulation(),
                ModulationSelect.Square => instance.Square.ToModulation(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void AppendSeq()
        {
            var instance = AUTDSettings.Instance;
            _seq = instance.Seq.ToPointSequence();
            AUTDSettings.Instance.GainMode = false;
        }

        public bool Start()
        {
            bool r;
            if (AUTDSettings.Instance.GainMode) r = _autd.Send(_gain, _mod);
            else r = _autd.Send(_mod) && _autd.Send(_seq);
            IsStarted.Value = r;
            return r;
        }

        public bool Pause()
        {
            var r = _autd.Pause();
            IsPaused.Value = r;
            return r;
        }

        public bool Resume()
        {
            var r = _autd.Resume();
            IsPaused.Value = !r;
            return r;
        }

        public void Dispose()
        {
            _autd.Clear();
            _autd.Close();
            _autd.Dispose();
        }
    }
}
