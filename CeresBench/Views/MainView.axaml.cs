using Avalonia.Controls;
using CeresBench.ViewModels;

namespace CeresBench.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void TextBlock_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }

    private void CustonVisaResourceNameTextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            var textBox = sender as TextBox;

            if (textBox?.DataContext is MainViewModel viewModel)
            {
                Focus();
                viewModel.CustomVisaResourceAddressEnteredCommand.Execute(null);
            }
        }
    }
}
