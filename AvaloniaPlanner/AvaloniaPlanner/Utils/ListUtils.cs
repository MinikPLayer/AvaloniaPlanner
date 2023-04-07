﻿using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Utils
{
    public static class ListUtils
    {
        public static void ConnectToList<Tin, Tout>(this ObservableCollection<Tin> source, List<Tout> list, Func<Tin, Tout> conversionFunc)
        {
            source.CollectionChanged += (s, e) => e.FillToList(list, conversionFunc);
        }

        private static void FillToList<Tin, Tout>(this NotifyCollectionChangedEventArgs e, List<Tout> list, Func<Tin, Tout> conversionFunc)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                list.Clear();
                return;
            }
            
            if (e.OldItems != null)
                foreach (var item in e.OldItems.OfType<Tin>())
                    list.Remove(conversionFunc(item));
            
            if (e.NewItems != null)
                foreach (var item in e.NewItems.OfType<Tin>())
                    list.Add(conversionFunc(item));
        }
    }
}
