using CeresBench.Models.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using Ivi.Visa;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace CeresBench.ViewModels.Application;

public partial class GenericDMMViewModel : ViewModelBase
{
    
    private CeresGenericDMMMModel _model;


    public ObservableCollection<CeresGenericDMMMModel.MeasurementModeItem> MeasurementModeList => _model.MeasurementModeList;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRangeAvailable))]
    [NotifyPropertyChangedFor(nameof(IsNPLCAvailable))]
    [NotifyPropertyChangedFor(nameof(IsBandwidthAvailable))]
    [NotifyPropertyChangedFor(nameof(MeasurementRangeCombo))]
    [NotifyPropertyChangedFor(nameof(MeasurementNPLCCombo))]
    [NotifyPropertyChangedFor(nameof(MeasurementBandwidthCombo))]
    [NotifyPropertyChangedFor(nameof(MeasurementRangeSelectedIndex))]
    [NotifyPropertyChangedFor(nameof(MeasurementNPLCSelectedIndex))]
    [NotifyPropertyChangedFor(nameof(MeasurementBandwidthSelectedIndex))]
    [NotifyPropertyChangedFor(nameof(IsAutoRange))]
    [NotifyPropertyChangedFor(nameof(IsAutoHiZAvailable))]
    [NotifyPropertyChangedFor(nameof(IsAutoHiZ))]
    [NotifyPropertyChangedFor(nameof(IsAutoZeroAvailable))]
    [NotifyPropertyChangedFor(nameof(IsAutoZero))]
    private int _measurementModeSelectedIndex = -1;
    partial void OnMeasurementModeSelectedIndexChanged(int value)
    {
        var modeString = MeasurementModeList[MeasurementModeSelectedIndex].ModeString;
        if (modeString != null && value != -1)
        {
            _model.SwitchMode(modeString);
        }
    }

    public ObservableCollection<string> MeasurementRangeCombo => new(MeasurementModeList[MeasurementModeSelectedIndex == -1 ? 0 : MeasurementModeSelectedIndex].RangeCombo.ValueList);
    public bool IsRangeAvailable => MeasurementModeList[MeasurementModeSelectedIndex].SetRangeInstruction != null;
    public int MeasurementRangeSelectedIndex { 
        get
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].SetRangeInstruction;
            if (instruction == null)
            {
                return -1;
            }
            var now = Convert.ToDouble(_model.Query(instruction));
            for (int i = 0; i < MeasurementRangeCombo.Count; i++)
            {
                if (Convert.ToDouble(MeasurementRangeCombo[i]) == now)
                {
                    return i;
                }
            }
            return -1;
        }
        set 
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].SetRangeInstruction;
            if (instruction == null)
            {
                return;
            }
            if (value != -1)
            {
                _model.Send(instruction, MeasurementModeList[MeasurementModeSelectedIndex].RangeCombo.ValueList[value]);
            }
        }
    }

    public ObservableCollection<string> MeasurementNPLCCombo => new(MeasurementModeList[MeasurementModeSelectedIndex == -1 ? 0 : MeasurementModeSelectedIndex].NPLCCombo.ValueList);
    public bool IsNPLCAvailable => MeasurementModeList[MeasurementModeSelectedIndex].SetNPLCInstruction != null;
    public int MeasurementNPLCSelectedIndex
    {
        get
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].SetNPLCInstruction;
            if (instruction == null)
            {
                return -1;
            }
            var now = Convert.ToDouble(_model.Query(instruction));
            for (int i = 0; i < MeasurementNPLCCombo.Count; i++)
            {
                if (Convert.ToDouble(MeasurementNPLCCombo[i]) == now)
                {
                    return i;
                }
            }
            return -1;
        }
        set
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].SetNPLCInstruction;
            if (instruction == null)
            {
                return;
            }
            if (value != -1)
            {
                _model.Send(instruction, MeasurementModeList[MeasurementModeSelectedIndex].NPLCCombo.ValueList[value]);
            }
        }
    }

    public ObservableCollection<string> MeasurementBandwidthCombo => new(MeasurementModeList[MeasurementModeSelectedIndex == -1 ? 0 : MeasurementModeSelectedIndex].BandwidthCombo.ValueList);
    public bool IsBandwidthAvailable => MeasurementModeList[MeasurementModeSelectedIndex].SetBandwidthInstruction != null;
    public int MeasurementBandwidthSelectedIndex {
        get
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].SetBandwidthInstruction;
            if (instruction == null)
            {
                return -1;
            }
            var now = Convert.ToDouble(_model.Query(instruction));
            for (int i = 0; i < MeasurementBandwidthCombo.Count; i++)
            {
                if (Convert.ToDouble(MeasurementBandwidthCombo[i]) == now)
                {
                    return i;
                }
            }
            return -1;
        }
        set
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].SetBandwidthInstruction;
            if (instruction == null)
            {
                return;
            }
            if (value != -1)
            {
                _model.Send(instruction, MeasurementModeList[MeasurementModeSelectedIndex].BandwidthCombo.ValueList[value]);
            }
        }
    }

    private bool _isAutoRange = false;
    public bool IsManualRange => !_isAutoRange;
    public bool IsAutoRange
    {
        get
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].ToggleAutoRangeInstruction;
            if (instruction == null)
            {
                return false;
            }
            var value = Convert.ToUInt32(_model.Query(instruction)) == 0 ? false : true;
            if (_isAutoRange != value)
            {
                _isAutoRange = value;
            }
            OnPropertyChanged(nameof(IsManualRange));
            return value;
        }
        set
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].ToggleAutoRangeInstruction;
            if (instruction == null)
            {
                return;
            }
            _model.Send(instruction, value ? "ON" : "OFF");
        }
    }

    public bool IsAutoHiZAvailable => MeasurementModeList[MeasurementModeSelectedIndex].ToggleHiZInstruction != null;
    public bool IsAutoHiZ
    {
        get
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].ToggleHiZInstruction;
            if (instruction == null)
            {
                return false;
            }
            var value = Convert.ToUInt32(_model.Query(instruction)) == 0 ? false : true;
            if (_isAutoRange != value)
            {
                _isAutoRange = value;
            }
            return value;
        }
        set
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].ToggleHiZInstruction;
            if (instruction == null)
            {
                return;
            }
            _model.Send(instruction, value ? "ON" : "OFF");
        }
    }

    public bool IsAutoZeroAvailable => MeasurementModeList[MeasurementModeSelectedIndex].ToggleAutoZeroInstruction != null;
    public bool IsAutoZero
    {
        get
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].ToggleAutoZeroInstruction;
            if (instruction == null)
            {
                return false;
            }
            var value = Convert.ToUInt32(_model.Query(instruction)) == 0 ? false : true;
            if (_isAutoRange != value)
            {
                _isAutoRange = value;
            }
            return value;
        }
        set
        {
            var instruction = MeasurementModeList[MeasurementModeSelectedIndex].ToggleAutoZeroInstruction;
            if (instruction == null)
            {
                return;
            }
            _model.Send(instruction, value ? "ON" : "OFF");
        }
    }

    [ObservableProperty]
    private string _measuredValue = "";

    public GenericDMMViewModel(XmlNode node, IMessageBasedSession instrumentSession)
    {
        _model = new CeresGenericDMMMModel(node, instrumentSession);
        _model.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(_model.MeasuredValue))
            {
                MeasuredValue = _model.MeasuredValue;
            }
        };
        var now = _model.GetMode();
        for (int i = 0; i < MeasurementModeList.Count; i++)
        {
            if (MeasurementModeList[i].ModeString == now)
            {
                MeasurementModeSelectedIndex = i;
            }
        }
    }
}
