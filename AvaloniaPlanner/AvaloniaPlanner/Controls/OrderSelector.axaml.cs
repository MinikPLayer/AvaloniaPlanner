﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Material.Icons;

namespace AvaloniaPlanner.Controls;

public class OrderSelectorEventArgs : RoutedEventArgs
{
    public object? Method { get; set; }
    public bool Ascending { get; set; }

    public OrderSelectorEventArgs(RoutedEvent routedEvent, object? method, bool ascending) : base(routedEvent)
    {
        this.Method = method;
        this.Ascending = ascending;
    }
}

public class OrderSelection
{
    public string Title { get; set; } = "";
    public MaterialIconKind Icon { get; set; } = MaterialIconKind.QuestionMark;
    public object? Value { get; set; }

    public OrderSelection(string title, MaterialIconKind icon, object? value = null)
    {
        Title = title;
        Icon = icon;
        Value = value;
    }
}

public partial class OrderSelector : UserControl
{
    public static RoutedEvent<OrderSelectorEventArgs> OrderMethodChangedEvent =
        RoutedEvent.Register<PaneEntry, OrderSelectorEventArgs>(nameof(OrderMethodChanged), RoutingStrategies.Bubble);

    public event EventHandler<OrderSelectorEventArgs> OrderMethodChanged
    {
        add => AddHandler(OrderMethodChangedEvent, value);
        remove => RemoveHandler(OrderMethodChangedEvent, value);
    }

    public bool GetAscending() => ProjectsOrderDirectionSelector.SelectedItem is Control { Tag: string and "asc" };
    
    public object? GetOrderMethod()
    {
        if(ProjectsOrderMethodSelector.SelectedItem is OrderSelection sel)
            return sel.Value;

        return null;
    }

    bool disableForceReoder = false;
    public void SetOrderMethods(IEnumerable<OrderSelection> methods, object? curSelection = null, bool? ascending = null)
    {
        disableForceReoder = true;
        var orderSelections = methods.ToList();
        ProjectsOrderMethodSelector.ItemsSource = orderSelections;
        if (curSelection != null)
            ProjectsOrderMethodSelector.SelectedItem = orderSelections.FirstOrDefault(x => x.Value != null && x.Value.Equals(curSelection));

        if (ascending.HasValue)
            ProjectsOrderDirectionSelector.SelectedIndex = ascending.Value ? 0 : 1;

        disableForceReoder = false;
        ForceReorder();
    }

    public void ForceReorder()
    {
        var method = ProjectsOrderMethodSelector.SelectedItem;
        if (method is not OrderSelection sel)
            return;
        
        var ascending = ProjectsOrderDirectionSelector.SelectedItem is Control { Tag: string and "asc" };
        var ev = new OrderSelectorEventArgs(OrderMethodChangedEvent, sel.Value, ascending);
        RaiseEvent(ev);
    }

    public OrderSelector()
    {
        InitializeComponent();
        this.DataContext = this;
    }

    private void _OrderMethodChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!disableForceReoder)
            ForceReorder();
    }
}