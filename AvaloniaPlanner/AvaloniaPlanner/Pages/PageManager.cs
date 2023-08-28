using Avalonia.Controls;
using Avalonia.Threading;
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
        private static Stack<UserControl> _pages = new Stack<UserControl>();

        public static bool CanGoBack() => _pages.Count > 0;

        private static void ChangePage(UserControl instance)
        {
            MainView.Singleton.ViewModel.CurrentPage = instance;

            // Bug on android preventing the switch
            if (MainView.IsMobile)
                MainView.Singleton.MainContent.Content = instance;

            MainView.Singleton.ViewModel.CanGoBack = CanGoBack();
        }

        public static UserControl GoBack()
        {
            var instance = _pages.Pop();
            ChangePage(instance); 
            return instance;
        }

        public static void ReplaceCurrentPage(UserControl instance)
        {
            ChangePage(instance);
        }

        public static void Navigate(UserControl instance)
        {
            var curPage = MainView.Singleton.ViewModel.CurrentPage as UserControl;
            if(curPage is not null)
                _pages.Push(curPage);
            ChangePage(instance);
        }

        public static void Navigate(Type t, bool force = false)
        {
            object? instance = null;
            if(!force)
            {
                if (MainView.Singleton.ViewModel.CurrentPage.GetType() == t)
                    return;

                foreach (var page in _pages)
                {
                    if (page.GetType() == t)
                    {
                        instance = page; 
                        break;
                    }
                }
            }

            instance ??= Activator.CreateInstance(t);
            if (instance is not UserControl uc)
                throw new ArgumentNullException($"Instance of type {t} cannot be created or is not a UserControl type");

            Navigate(uc);
        }

        public static void Navigate<T>(bool force = false) where T : UserControl => Navigate(typeof(T), force);
    }
}
