using Avalonia.Controls;

namespace CeresBench.Views.Application;

public interface IVersionView
{
    public string VersionString { get; }
}

public partial class DefaultView : UserControl, IVersionView
{
    string IVersionView.VersionString => "1.0.0";

    public DefaultView()
    {
        InitializeComponent();
    }
}