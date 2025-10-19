using Avalonia.Controls;

namespace CeresBench.Views.Application;

public partial class GenericCounterView : UserControl, IVersionView
{
    string IVersionView.VersionString => "1.0.0";

    public GenericCounterView()
    {
        InitializeComponent();
    }
}