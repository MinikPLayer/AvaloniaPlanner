using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaPlanner.Controls;
using Material.Icons;

namespace AvaloniaPlanner.Pages;

public partial class TestPage : UserControl
{
    public TestPage()
    {
        InitializeComponent();
        this.OrderSelector.SetOrderMethods(new List<OrderSelection>()
        {
            new OrderSelection("Title", MaterialIconKind.SortAlphabeticalAscending, "Title"),
            new OrderSelection("Date", MaterialIconKind.SortNumericAscending, "Date"),
            new OrderSelection("Priority", MaterialIconKind.SortNumericAscending, "Priority"),
            new OrderSelection("Status", MaterialIconKind.SortNumericAscending, "Status"),
        }, "Title", true);
    }
}