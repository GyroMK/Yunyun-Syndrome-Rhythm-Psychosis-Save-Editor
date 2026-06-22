using System.Windows;
using System.Windows.Controls;

namespace YunyunSaveEditor;

/// <summary>Pequeño diálogo de entrada de texto (no hay InputBox en WPF).</summary>
public static class Prompt
{
    public static string? Show(Window owner, string message, string? title = null)
    {
        var win = new Window
        {
            Title = title ?? Loc.T("app_name"),
            Width = 380,
            Height = 170,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            FontSize = 13
        };

        var grid = new Grid { Margin = new Thickness(16) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var lbl = new TextBlock { Text = message, Margin = new Thickness(0, 0, 0, 8), TextWrapping = TextWrapping.Wrap };
        Grid.SetRow(lbl, 0);
        var box = new TextBox { Height = 28 };
        Grid.SetRow(box, 1);

        var panel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
        var ok = new Button { Content = Loc.T("btn_ok"), Width = 90, Height = 30, IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = Loc.T("btn_cancel"), Width = 90, Height = 30, IsCancel = true };
        panel.Children.Add(ok);
        panel.Children.Add(cancel);
        Grid.SetRow(panel, 2);
        panel.VerticalAlignment = VerticalAlignment.Bottom;

        grid.Children.Add(lbl);
        grid.Children.Add(box);
        grid.Children.Add(panel);
        win.Content = grid;

        string? result = null;
        ok.Click += (_, _) => { result = box.Text; win.DialogResult = true; };
        box.Loaded += (_, _) => box.Focus();

        return win.ShowDialog() == true ? result : null;
    }
}
