using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace AvaloniaPlanner.Controls
{
    public class SearchEventArgs : RoutedEventArgs
    {
        public string? SearchTerm { get; set; } = "";

        public SearchEventArgs(RoutedEvent? ev, string? searchTerm) : base(ev)
        {
            this.SearchTerm = searchTerm;
        }
    }

    public partial class SearchControl : UserControl
    {
        public static RoutedEvent<SearchEventArgs> SearchRequestedEvent =
            RoutedEvent.Register<SearchControl, SearchEventArgs>(nameof(SearchRequested), RoutingStrategies.Bubble);

        public event EventHandler<SearchEventArgs> SearchRequested
        {
            add => AddHandler(SearchRequestedEvent, value);
            remove => RemoveHandler(SearchRequestedEvent, value);
        }

        public void RaiseSearchEvent()
        {
            ResetSearchButton.IsVisible = SearchTextBox.Text?.Length > 0;
            RaiseEvent(new SearchEventArgs(SearchRequestedEvent, SearchTextBox.Text));
        }

        public void ResetSearch(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            RaiseSearchEvent();
        }

        public void SearchButtonClicked(object sender, RoutedEventArgs e) => RaiseSearchEvent();

        public SearchControl()
        {
            this.DataContext = this;
            InitializeComponent();
        }
    }
}
