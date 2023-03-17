using Avalonia.Controls;
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
        public static void Navigate(UserControl instance)
        {
            MainView.Singleton.ViewModel.CurrentPage = instance;
        }

        public static void Navigate(Type t)
        {
            var instance = Activator.CreateInstance(t);
            if (instance is not UserControl uc)
                throw new ArgumentNullException($"Instance of type {t} cannot be created or is not a UserControl type");

            Navigate(uc);
        }

        public static void Navigate<T>() where T : UserControl => Navigate(typeof(T));
    }
}
