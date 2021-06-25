/*
 * File: SeqViewModel.cs
 * Project: ViewModels
 * Created Date: 06/05/2021
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
using Reactive.Bindings.Extensions;

namespace PI450Viewer.ViewModels
{
    internal class SeqViewModel : ReactivePropertyBase, IDropTarget
    {
        public ReactivePropertySlim<Seq> Seq { get; }

        public ObservableCollectionWithItemNotify<Vector3Reactive> Points { get; }
        public ReactivePropertySlim<Vector3Reactive?> Current { get; }

        public ReactiveCommand AddItem { get; }
        public ReactiveCommand RemoveItem { get; }
        public ReactiveCommand UpItem { get; }
        public ReactiveCommand DownItem { get; }

        public ReactiveCommand SendSeqCommand { get; }

        public SeqViewModel()
        {
            Seq = AUTDSettings.Instance.ToReactivePropertySlimAsSynchronized(i => i.Seq);
            Points = AUTDSettings.Instance.Seq.PointsReactive;

            SendSeqCommand = AUTDHandler.Instance.IsOpen.Select(b => b).ToReactiveCommand();
            SendSeqCommand.Subscribe(_ =>
            {
                if (Points.Count == 0) return;
                AUTDHandler.Instance.AppendSeq();
            });

            Current = new ReactivePropertySlim<Vector3Reactive?>();
            AddItem = new ReactiveCommand();
            RemoveItem = Current.Select(c => c != null).ToReactiveCommand();
            UpItem = Current.Select(c => c != null && c.No.Value != 0).ToReactiveCommand();
            DownItem = Current.Select(c => c != null && c.No.Value != Points.Count - 1).ToReactiveCommand();

            AddItem.Subscribe(_ =>
            {
                var item = new Vector3Reactive(Points.Count);
                Points.Add(item);
                Current.Value = item;
            });
            RemoveItem.Subscribe(_ =>
            {
                if (Current.Value == null) return;

                var delNo = Current.Value.No.Value;
                Points.RemoveAt(delNo);
                ResetNo();
                Current.Value = Points.Count > delNo ? Points[delNo] : Points.Count > 0 ? Points[delNo - 1] : null;
            });
            UpItem.Subscribe(_ =>
            {
                if (Current.Value == null) return;

                var cNo = Current.Value.No.Value;
                var up = Points[cNo - 1];
                Points.Insert(cNo - 1, Current.Value);
                Points.RemoveAt(cNo);
                Points[cNo] = up;
                ResetNo();
                Current.Value = Points[cNo - 1];
            });
            DownItem.Subscribe(_ =>
            {
                if (Current.Value == null) return;

                var cNo = Current.Value.No.Value;
                var down = Points[cNo + 1];
                Points.RemoveAt(cNo + 1);
                Points.Insert(cNo + 1, Current.Value);
                Points[cNo] = down;
                ResetNo();
                Current.Value = Points[cNo + 1];
            });
        }

        private void ResetNo()
        {
            for (var i = 0; i < Points.Count; i++) Points[i].No.Value = i;
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
                Current.Value = Points[insertIndex - 1];
                enumerator.Dispose();
            }
        }
    }
}
