using AvaloniaPlanner.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Pages
{
    public static class PageManager
    {
        public static void Navigate(Type t)
        {
            var instance = Activator.CreateInstance(t);
            if (instance == null)
                throw new ArgumentNullException("Cannot create an instance of type " + t.Name);

            MainView.Singleton.ViewModel.CurrentPage = instance;
        }
    }
}
