/*
 * File: GeometryViewModel.cs
 * Project: ViewModels
 * Created Date: 29/03/2021
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
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Reactive.Bindings;

namespace PI450Viewer.ViewModels
{
    internal class GeometryViewModel : ReactivePropertyBase, IDropTarget
    {
        public ObservableCollectionWithItemNotify<GeometrySettingReactive> Geometries { get; }
        public ReactivePropertySlim<GeometrySettingReactive?> Current { get; }

        public ReactiveCommand AddItem { get; }
        public ReactiveCommand RemoveItem { get; }
        public ReactiveCommand UpItem { get; }
        public ReactiveCommand DownItem { get; }

        public GeometryViewModel()
        {
            Geometries = AUTDSettings.Instance.GeometriesReactive;

            Current = new ReactivePropertySlim<GeometrySettingReactive?>();
            AddItem = new ReactiveCommand();
            RemoveItem = Current.Select(c => c != null).ToReactiveCommand();
            UpItem = Current.Select(c => c != null && c.No.Value != 0).ToReactiveCommand();
            DownItem = Current.Select(c => c != null && c.No.Value != Geometries.Count - 1).ToReactiveCommand();

            AddItem.Subscribe(_ =>
            {
                var item = new GeometrySettingReactive(Geometries.Count);
                Geometries.Add(item);
                Current.Value = item;
            });
            RemoveItem.Subscribe(_ =>
            {
                if (Current.Value == null) return;

                var delNo = Current.Value.No.Value;
                Geometries.RemoveAt(delNo);
                ResetNo();
                Current.Value = Geometries.Count > delNo ? Geometries[delNo] : Geometries.Count > 0 ? Geometries[delNo - 1] : null;
            });
            UpItem.Subscribe(_ =>
            {
                if (Current.Value == null) return;

                var cNo = Current.Value.No.Value;
                var up = Geometries[cNo - 1];
                Geometries.Insert(cNo - 1, Current.Value);
                Geometries.RemoveAt(cNo);
                Geometries[cNo] = up;
                ResetNo();
                Current.Value = Geometries[cNo - 1];
            });
            DownItem.Subscribe(_ =>
            {
                if (Current.Value == null) return;

                var cNo = Current.Value.No.Value;
                var down = Geometries[cNo + 1];
                Geometries.RemoveAt(cNo + 1);
                Geometries.Insert(cNo + 1, Current.Value);
                Geometries[cNo] = down;
                ResetNo();
                Current.Value = Geometries[cNo + 1];
            });
        }

        private void ResetNo()
        {
            for (var i = 0; i < Geometries.Count; i++) Geometries[i].No.Value = i;
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
                Current.Value = Geometries[insertIndex - 1];
                enumerator.Dispose();
            }
        }
    }
}
