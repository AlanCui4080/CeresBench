using Avalonia.Controls;

namespace CeresBench.Views.Application;

public partial class GenericDMMView : UserControl, IVersionView
{
    string IVersionView.VersionString => "1.0.0";

    public GenericDMMView()
    {
        InitializeComponent();
    }
}