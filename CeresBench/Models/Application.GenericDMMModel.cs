using CommunityToolkit.Mvvm.ComponentModel;
using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CeresBench.Models.Application;

public partial class CeresGenericDMMMModel : ObservableObject
{

    public class ComboNumericType
    {
        public enum Type
        {
            FixedList,
            MinMax
        };

        public Type NumericType = Type.FixedList;
        public List<string> ValueList = new List<string>();
        public string ValueMin = "";
        public string ValueMax = "";

        public ComboNumericType(XmlNode node)
        {
            switch (node?.Attributes?.GetNamedItem("type")?.Value)
            {
                case "FixedList":
                    NumericType = Type.FixedList;
                    ValueList = (node?.Attributes?.GetNamedItem("list")
                                                ?.Value?.Split(',')
                                                       ?.ToList()) ?? new();
                    break;
                case "MinMax":
                    NumericType = Type.MinMax;
                    ValueMin = node?.Attributes?.GetNamedItem("min")?.Value ?? "";
                    ValueMax = node?.Attributes?.GetNamedItem("max")?.Value ?? "";
                    break;
                default:
                    throw new NotImplementedException();

            }
        }

        public ComboNumericType()
        {
        }
    }

    public partial class MeasurementModeItem : ObservableObject
    {
        [ObservableProperty]
        private string _mode = "";

        [ObservableProperty]
        private string _unit = "";

        public string? ModeString;

        public string? SetRangeInstruction;
        public ComboNumericType RangeCombo = new();

        public string? SetNPLCInstruction;
        public ComboNumericType NPLCCombo = new();


        public string? SetBandwidthInstruction;
        public ComboNumericType BandwidthCombo = new();

        public string? ToggleAutoRangeInstruction;
        public string? ToggleAutoZeroInstruction;
        public string? SetHiZInstruction;
    }

    public partial class TriggerModeItem : ObservableObject
    {
        [ObservableProperty]
        private string _mode = "";

        public string? _switchModeInstruction;
    }

    public ObservableCollection<MeasurementModeItem> MeasurementModeList = new();
    private List<string> _preInitInstructionList = new();
    private string? _switchModeInstrcution;
    private string? _postInitQuery;
    private string? _dispOnInstruction;
    private string? _dispOffInstruction;

    [ObservableProperty]
    private string _measuredValue = "";

    private IMessageBasedSession _instrumentSession;

    public void Send(string instruction, string param)
    {
        Debug.WriteLine($"[ApplicationCenericDMMModel] send {instruction} {param}");
        _instrumentSession.FormattedIO.WriteLine($"{instruction} {param}");
    }

    public void SwitchMode(string mode)
    {
        Debug.WriteLine($"[ApplicationCenericDMMModel] switch mode {mode}");
        _instrumentSession.FormattedIO.WriteLine($"{_switchModeInstrcution} {mode}");
    }

    public string GetMode()
    {
        ArgumentNullException.ThrowIfNull(_switchModeInstrcution);
        return Query(_switchModeInstrcution);
    }

    public string Query(string instruction)
    {
        _instrumentSession.FormattedIO.WriteLine($"{instruction}?");
        var result = _instrumentSession.FormattedIO.ReadLine().TrimEnd();
        Debug.WriteLine($"[ApplicationCenericDMMModel] query {instruction}? result {result}");
        return result;
    }

    private void Init()
    {

    }

    public CeresGenericDMMMModel(XmlNode appNode, IMessageBasedSession instrumentSession)
    {
        _instrumentSession = instrumentSession;

        foreach (XmlNode? childNode in appNode.ChildNodes)
        {
            switch (childNode?.Name)
            {
                case "SwitchMode":
                    _switchModeInstrcution = childNode.InnerText;
                    break;
                case "Init":
                    foreach (XmlNode? key in childNode.ChildNodes)
                    {
                        switch (key?.Name)
                        {
                            case "PreInit":
                                _preInitInstructionList.Add(key.InnerText);
                                break;
                            case "PostInit":
                                _postInitQuery = key.InnerText;
                                break;
                        }
                    }
                    break;
                case "Display":
                    foreach (XmlNode? key in childNode.ChildNodes)
                    {
                        switch (key?.Name)
                        {
                            case "SetOff":
                                _dispOffInstruction = key.InnerText;
                                break;
                            case "SetOn":
                                _dispOnInstruction = key.InnerText;
                                break;
                        }
                    }
                    break;
                case "MeasureMode":
                    var modeItem = new MeasurementModeItem { Mode = childNode.Attributes?.GetNamedItem("name")?.Value ?? "Invalid Config XML" };
                    foreach (XmlNode? key in childNode.ChildNodes)
                    {
                        switch (key?.Name)
                        {
                            case "Unit":
                                modeItem.Unit = key.InnerText;
                                break;
                            case "ModeString":
                                modeItem.ModeString = key.InnerText;
                                break;
                            case "SetRange":
                                modeItem.SetRangeInstruction = key.InnerText;
                                modeItem.RangeCombo = new(key);
                                break;
                            case "SetNPLC":
                                modeItem.SetNPLCInstruction = key.InnerText;
                                modeItem.NPLCCombo = new(key);
                                break;
                            case "SetBandWidth":
                                modeItem.SetBandwidthInstruction = key.InnerText;
                                modeItem.BandwidthCombo = new(key);
                                break;
                            case "ToggleAutoRange":
                                modeItem.ToggleAutoRangeInstruction = key.InnerText;
                                break;
                            case "ToggleAutoZero":
                                modeItem.ToggleAutoZeroInstruction = key.InnerText;
                                break;
                            case "SetHiZ":
                                modeItem.SetHiZInstruction = key.InnerText;
                                break;
                        }
                    }
                    MeasurementModeList.Add(modeItem);
                    break;
            }
        }
        //Task.Run(() =>
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            if (ResourceManager.FormattedIO != null)
        //            {
        //                ResourceManager.FormattedIO.WriteLine("TRIG:SOUR IMM");
        //                ResourceManager.FormattedIO.WriteLine("READ?");
        //                var value = ResourceManager.FormattedIO.ReadDouble();
        //                if (value >= 9.90000000E+37)
        //                {
        //                    MeasuredValue = "OVLD";
        //                }
        //                else
        //                {
        //                    MeasuredValue = FormatWithCommasAndPrecision(value);
        //                }
        //            }
        //        }
        //        catch (IOTimeoutException ex)
        //        {
        //            _ = ex;
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        finally
        //        {
        //            Task.Delay(10).Wait();
        //        }
        //    }
        //});
    }

    public static string FormatWithCommasAndPrecision(double value, string unit, int totalDigits = 10)
    {
        string numStr = Math.Abs(value).ToString("F" + totalDigits);
        
        string[] parts = numStr.Split('.');
        string intPart = parts[0];
        string decimalPart = parts.Length > 1 ? parts[1] : "";

        string formattedIntPart = "";
        for (int i = intPart.Length - 1, count = 0; i >= 0; i--)
        {
            if (count > 0 && count % 3 == 0)
            {
                formattedIntPart = "," + formattedIntPart;
            }
            formattedIntPart = intPart[i] + formattedIntPart;
            count++;
        }

        string formattedDecimalPart = "";
        for (int i = 0; i < decimalPart.Length; i++)
        {
            if (i > 0 && i % 3 == 0)
            {
                formattedDecimalPart += ",";
            }
            formattedDecimalPart += decimalPart[i];
        }

        string result = formattedIntPart;
        if (!string.IsNullOrEmpty(formattedDecimalPart))
        {
            result += "." + formattedDecimalPart;
        }

        if (value < 0)
        {
            result = "-" + result;
        }
        else
        {
            result = "+" + result;
        }

        result += unit;

        return result;
    }
}
