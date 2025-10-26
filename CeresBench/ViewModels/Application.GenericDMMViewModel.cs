using CeresBench.Models;
using CeresBench.Models.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;

namespace CeresBench.ViewModels.Application;

public partial class GenericDMMViewModel : ViewModelBase
{
    
    private CeresGenericDMMMModel _model = new();

    public VISAResourceManagerModel ResourceManagerModel
    {
        get
        {
            return _model.ResourceManager;
        }
        set
        {
            _model.ResourceManager = value;
        }
    }

    public ObservableCollection<CeresGenericDMMMModel.MeasurementModeItem> MeasurementModeList => _model.MeasurementModeList;
    [ObservableProperty]
    private int _measurementModeSelectedIndex = -1;
    partial void OnMeasurementModeSelectedIndexChanged(int value)
    {
        if (value != -1)
        {
            IsRangeAvailable = MeasurementModeList[MeasurementModeSelectedIndex].SetRangeInstruction != null;
            
            IsNPLCAvailable = MeasurementModeList[MeasurementModeSelectedIndex].SetNPLCInstruction != null;
            
            MeasurementRangeSelectedIndex = 0;
            MeasurementNPLCSelectedIndex = 0;
        }
    }

    public ObservableCollection<string> MeasurementRangeCombo => new(MeasurementModeList[MeasurementModeSelectedIndex == -1 ? 0 : MeasurementModeSelectedIndex].RangeCombo.ValueList);
    [ObservableProperty]
    private bool _isRangeAvailable;
    [ObservableProperty]
    private int _measurementRangeSelectedIndex = 0;
    partial void OnMeasurementRangeSelectedIndexChanged(int value)
    {
        if (IsRangeAvailable)
        {
            _model.SendInstruction(MeasurementModeList[MeasurementModeSelectedIndex].SetRangeInstruction,
                           MeasurementModeList[MeasurementModeSelectedIndex].RangeCombo.ValueList[MeasurementRangeSelectedIndex]);
        }
    }

    public ObservableCollection<string> MeasurementNPLCCombo => new(MeasurementModeList[MeasurementModeSelectedIndex == -1 ? 0 : MeasurementModeSelectedIndex].NPLCCombo.ValueList);
    [ObservableProperty]
    private bool _isNPLCAvailable;
    [ObservableProperty]
    private int _measurementNPLCSelectedIndex = 0;
    partial void OnMeasurementNPLCSelectedIndexChanged(int value)
    {
        if (IsNPLCAvailable)
        {
            _model.SendInstruction(MeasurementModeList[MeasurementModeSelectedIndex].SetNPLCInstruction,
                           MeasurementModeList[MeasurementModeSelectedIndex].NPLCCombo.ValueList[MeasurementNPLCSelectedIndex]);
        }
    }

    [ObservableProperty]
    private string _measuredValue = "";

    public GenericDMMViewModel()
    {
        _model.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(_model.MeasuredValue))
            {
                MeasuredValue = _model.MeasuredValue;
            }
        };
    }
}
