using CeresBench.Models;
using CeresBench.Models.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common.Internals;
using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
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
