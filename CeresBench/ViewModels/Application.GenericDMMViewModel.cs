using CeresBench.Models.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common.Internals;
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
    public string _measuredValue = "-123.456,789,000";
    [ObservableProperty]
    public string _measuredUnit = "VDC";
    [ObservableProperty]
    public string _avgValue = "Avg: -123.456,789,000";
    [ObservableProperty]
    public string _minValue = "Min: -123.456,789,000";
    [ObservableProperty]
    public string _maxValue = "Max: -123.456,789,000";

    public GenericDMMViewModel(XmlNode node, IMessageBasedSession instrumentSession)
    {
        _model = new CeresGenericDMMMModel(node, instrumentSession);
        _model.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(_model.MeasuredValue))
            {
                double absValue = Math.Abs(_model.MeasuredValue);
                string prefix = "";
                switch (absValue)
                {
                    case < 1e-6:
                        absValue *= 1e9;
                        prefix = "n";
                        break;
                    case < 1e-3:
                        absValue *= 1e6;
                        prefix = "u";
                        break;
                    case > 1e9:
                        absValue /= 1e6;
                        prefix = "G";
                        break;
                    case > 1e6:
                        absValue /= 1e6;
                        prefix = "M";
                        break;
                    case > 1e3:
                        absValue /= 1e3;
                        prefix = "k";
                        break;
                }
                MeasuredUnit = prefix + MeasurementModeList[MeasurementModeSelectedIndex].Unit;
                MeasuredValue = FormatWithCommasAndPrecision(_model.MeasuredValue);
                AvgValue = "Avg: " + MeasuredValue;
                MinValue = "Min: " + MeasuredValue;
                MaxValue = "Max: " + MeasuredValue;
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

    public static string FormatWithCommasAndPrecision(double value)
    {
        double absValue = Math.Abs(value);
        switch (absValue)
        {
            case < 1e-6:
                absValue *= 1e9;
                break;
            case < 1e-3:
                absValue *= 1e6;
                break;
            case > 1e9:
                absValue /= 1e6;
                break;
            case > 1e6:
                absValue /= 1e6;
                break;
            case > 1e3:
                absValue /= 1e3;
                break;
        }
        string numStr = absValue.ToString("F9");
        if ((int)absValue < 10)
        {
            numStr = "00" + numStr;
        }
        else if ((int)absValue < 100)
        {
            numStr = "0" + numStr;
        }
        int originalLength = numStr.Length;
        numStr = numStr.TrimEnd('0').TrimEnd('.');
        int trimmedCount = originalLength - numStr.Length;
        numStr += new string('0', trimmedCount % 3);

        string[] parts = numStr.Split('.');
        string intPart = parts[0];
        string decimalPart = parts.Length > 1 ? parts[1] : "";

        string formattedIntPart = "";
        for (int i = intPart.Length - 1, count = 0; i >= 0; i--)
        {
            if (count > 0 && count % 3 == 0)
                formattedIntPart = "," + formattedIntPart;
            formattedIntPart = intPart[i] + formattedIntPart;
            count++;
        }

        string formattedDecimalPart = "";
        for (int i = 0; i < decimalPart.Length; i++)
        {
            if (i > 0 && i % 3 == 0)
                formattedDecimalPart += ",";
            formattedDecimalPart += decimalPart[i];
        }

        string result = formattedIntPart;
        if (!string.IsNullOrWhiteSpace(formattedDecimalPart))
            result += "." + formattedDecimalPart;

        result = (value < 0 ? "-" : "+") + result;

        return result;
    }


}
