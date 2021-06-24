/*
 * File: HoloViewModel.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using PI450Viewer.Helpers;
using PI450Viewer.Models;
using PI450Viewer.Models.Gain;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace PI450Viewer.ViewModels.Gain
{

    public class HoloViewModel : ReactivePropertyBase, IDropTarget
    {


        public ReactiveProperty<Holo> Holo { get; }

        public ObservableCollectionWithItemNotify<HoloSettingReactive> HoloSettings { get; }
        public ReactiveProperty<HoloSettingReactive?> Current { get; }

        public OptMethod[] OptMethods { get; } = (OptMethod[])Enum.GetValues(typeof(OptMethod));

        public ReactiveCommand AddItem { get; }
        public ReactiveCommand RemoveItem { get; }
        public ReactiveCommand UpItem { get; }
        public ReactiveCommand DownItem { get; }

        public HoloViewModel()
        {
            Holo = AUTDSettings.Instance.ToReactivePropertyAsSynchronized(i => i.Holo);

            HoloSettings = AUTDSettings.Instance.Holo.HoloSettingsReactive;

            Current = new ReactiveProperty<HoloSettingReactive?>();
            AddItem = new ReactiveCommand();
            RemoveItem = Current.Select(c => c != null).ToReactiveCommand();
            UpItem = Current.Select(c => c != null && c.No.Value != 0).ToReactiveCommand();
            DownItem = Current.Select(c => c != null && c.No.Value != HoloSettings.Count - 1).ToReactiveCommand();

            AddItem.Subscribe(_ =>
            {
                var item = new HoloSettingReactive(HoloSettings.Count);
                HoloSettings.Add(item);
                Current.Value = item;
            });
            RemoveItem.Subscribe(_ =>
            {
                if (Current.Value == null) return;

                var delNo = Current.Value.No.Value;
                HoloSettings.RemoveAt(delNo);
                ResetNo();
                Current.Value = HoloSettings.Count > delNo ? HoloSettings[delNo] : HoloSettings.Count > 0 ? HoloSettings[delNo - 1] : null;
            });
            UpItem.Subscribe(_ =>
            {
                if (Current.Value == null) return;

                var cNo = Current.Value.No.Value;
                var up = HoloSettings[cNo - 1];
                HoloSettings.Insert(cNo - 1, Current.Value);
                HoloSettings.RemoveAt(cNo);
                HoloSettings[cNo] = up;
                ResetNo();
                Current.Value = HoloSettings[cNo - 1];
            });
            DownItem.Subscribe(_ =>
            {
                if (Current.Value == null) return;

                var cNo = Current.Value.No.Value;
                var down = HoloSettings[cNo + 1];
                HoloSettings.RemoveAt(cNo + 1);
                HoloSettings.Insert(cNo + 1, Current.Value);
                HoloSettings[cNo] = down;
                ResetNo();
                Current.Value = HoloSettings[cNo + 1];
            });
        }

        private void ResetNo()
        {
            for (var i = 0; i < HoloSettings.Count; i++) HoloSettings[i].No.Value = i;
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (!(dropInfo.Data is GeometrySettingReactive)) return;

            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }

        private static IEnumerable ExtractData(object data)
        {
            if (data is IEnumerable enumerable && !(enumerable is string)) return enumerable;
            return Enumerable.Repeat(data, 1);
        }

        private static bool ShouldCopyData(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo == null) return false;

            var copyData = dropInfo.DragInfo.DragDropCopyKeyState != default && dropInfo.KeyStates.HasFlag(dropInfo.DragInfo.DragDropCopyKeyState)
                           || dropInfo.DragInfo.DragDropCopyKeyState.HasFlag(DragDropKeyStates.LeftMouseButton);
            copyData = copyData
                       && !(dropInfo.DragInfo.SourceItem is HeaderedContentControl)
                       && !(dropInfo.DragInfo.SourceItem is HeaderedItemsControl)
                       && !(dropInfo.DragInfo.SourceItem is ListBoxItem);
            return copyData;
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo == null) return;

            var insertIndex = dropInfo.InsertIndex != dropInfo.UnfilteredInsertIndex ? dropInfo.UnfilteredInsertIndex : dropInfo.InsertIndex;
            if (dropInfo.VisualTarget is ItemsControl itemsControl)
            {
                IEditableCollectionView editableItems = itemsControl.Items;
                var newItemPlaceholderPosition = editableItems.NewItemPlaceholderPosition;
                switch (newItemPlaceholderPosition)
                {
                    case NewItemPlaceholderPosition.AtBeginning when insertIndex == 0:
                        insertIndex++;
                        break;
                    case NewItemPlaceholderPosition.AtEnd when insertIndex == itemsControl.Items.Count:
                        insertIndex--;
                        break;
                    case NewItemPlaceholderPosition.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            var destinationList = dropInfo.TargetCollection.TryGetList();
            var data = ExtractData(dropInfo.Data).OfType<object>().ToList();
            List<object>.Enumerator enumerator;
            if (!ShouldCopyData(dropInfo))
            {
                var sourceList = dropInfo.DragInfo.SourceCollection.TryGetList();
                if (sourceList != null)
                {
                    enumerator = data.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            var o2 = enumerator.Current;
                            var index = sourceList.IndexOf(o2);
                            if (index == -1) continue;

                            sourceList.RemoveAt(index);
                            if (destinationList != null && Equals(sourceList, destinationList) && index < insertIndex)
                            {
                                insertIndex--;
                            }
                        }
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                }
            }

            if (destinationList == null) return;

            var cloneData = dropInfo.Effects.HasFlag(DragDropEffects.Copy) || dropInfo.Effects.HasFlag(DragDropEffects.Link);
            enumerator = data.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    var o = enumerator.Current;
                    var obj2Insert = o;
                    if (cloneData)
                    {
                        if (o is ICloneable cloneable)
                        {
                            obj2Insert = cloneable.Clone();
                        }
                    }
                    destinationList.Insert(insertIndex++, obj2Insert);
                    if (!(dropInfo.VisualTarget is TabControl tabControl)) continue;

                    if (tabControl.ItemContainerGenerator.ContainerFromItem(obj2Insert) is TabItem obj)
                    {
                        obj.ApplyTemplate();
                    }
                    tabControl.SetSelectedItem(obj2Insert);
                }
            }
            finally
            {
                ResetNo();
                Current.Value = HoloSettings[insertIndex - 1];
                enumerator.Dispose();
            }
        }
    }
}
