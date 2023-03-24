using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Utils
{
    public static class ListUtils
    {
        public static void FillToList<Tin, Tout>(this NotifyCollectionChangedEventArgs e, List<Tout> list, Func<Tin, Tout> conversionFunc)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems.OfType<Tin>())
                    list.Add(conversionFunc(item));

            if (e.OldItems != null)
                foreach (var item in e.OldItems.OfType<Tin>())
                    list.Remove(conversionFunc(item));
        }
    }
}
