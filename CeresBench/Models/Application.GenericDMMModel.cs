using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CeresBench.Models.Application;

public partial class CeresGenericDMMMModel : ObservableObject
{
    public VISAResourceManagerModel ResourceManager = new();

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

        [ObservableProperty]
        public string? _switchModeInstruction;
        [ObservableProperty]
        public string? _setBandwidthInstruction;

        [ObservableProperty]
        public string? _setRangeInstruction;
        public ComboNumericType RangeCombo = new();

        [ObservableProperty]
        public string? _setNPLCInstruction;
        public ComboNumericType NPLCCombo = new();

        [ObservableProperty]
        public string? _toggleAutoRangeInstruction;
        [ObservableProperty]
        public string? _toggleAutoZeroInstruction;
        [ObservableProperty]
        public string? _toggleHiZInstruction;
    }

    public ObservableCollection<MeasurementModeItem> MeasurementModeList = new();

    [ObservableProperty]
    private string _measuredValue = "";

    public void SendInstruction(string instruction, string range)
    {
        ResourceManager.FormattedIO.WriteLine($"{instruction} {range}");
    }

    public CeresGenericDMMMModel()
    {
        //_resourceManager = resourceManager;

        XmlDocument xmlDocument = new();
        xmlDocument.Load("Configs/ModelApplications.xml");
        var rootElement = xmlDocument.DocumentElement;

        if (rootElement == null)
        {
            return;
        }

        foreach (XmlNode? node in rootElement.ChildNodes)
        {
            if (node?.Name == "Application" && node.Attributes?.GetNamedItem("name")?.Value == "GenericDMM")
            {
                foreach (XmlNode? childNode in node.ChildNodes)
                {
                    switch (childNode?.Name)
                    {
                        //case "Reset":
                        //    resourceManager.FormattedIO?.WriteLine(childNode.InnerText);
                        //    break;
                        case "MeasureMode":
                            MeasurementModeItem modeItem = new MeasurementModeItem();
                            modeItem.Mode = childNode.Attributes?.GetNamedItem("name")?.Value ?? "Invalid Config XML";
                            foreach (XmlNode? key in childNode.ChildNodes)
                            {
                                switch (key?.Name)
                                {
                                    case "Unit":
                                        modeItem.Unit = key.InnerText;
                                        break;
                                    case "SwitchMode":
                                        modeItem.SwitchModeInstruction = key.InnerText;
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
                                        break;
                                    case "ToggleAutoRange":
                                        modeItem.ToggleAutoRangeInstruction = key.InnerText;
                                        break;
                                    case "ToggleAutoZero":
                                        modeItem.ToggleAutoZeroInstruction = key.InnerText;
                                        break;
                                    case "ToggleHiZ":
                                        modeItem.ToggleHiZInstruction = key.InnerText;
                                        break;
                                }
                            }
                            MeasurementModeList.Add(modeItem);
                            break;
                    }
                }
            }
        }
        Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    if (ResourceManager.FormattedIO != null)
                    {
                        ResourceManager.FormattedIO.WriteLine("TRIG:SOUR IMM");
                        ResourceManager.FormattedIO.WriteLine("READ?");
                        var value = ResourceManager.FormattedIO.ReadDouble();
                        if (value >= 9.90000000E+37)
                        {
                            MeasuredValue = "OVLD";
                        }
                        else
                        {
                            MeasuredValue = FormatWithCommasAndPrecision(value);
                        }
                    }
                }
                catch
                {

                }
            }
        });
    }

    private static string FormatWithCommasAndPrecision(double number)
    {
        // 获取整数部分和小数部分
        string numberString = number.ToString("#,0.####################");

        int decimalPointIndex = numberString.IndexOf('.');

        string integerPart = (decimalPointIndex >= 0) ? numberString.Substring(0, decimalPointIndex) : numberString;
        string fractionalPart = (decimalPointIndex >= 0) ? numberString.Substring(decimalPointIndex + 1) : "";

        // 格式化整数部分（每三位插入一个逗号）
        integerPart = string.Format("{0:#,0}", long.Parse(integerPart));

        // 格式化小数部分（每三位插入一个逗号）
        string formattedFractionalPart = "";
        for (int i = 0; i < fractionalPart.Length; i += 3)
        {
            if (i + 3 <= fractionalPart.Length)
            {
                formattedFractionalPart += fractionalPart.Substring(i, 3) + ",";
            }
            else
            {
                formattedFractionalPart += fractionalPart.Substring(i);
            }
        }

        // 移除最后的逗号（如果有）
        if (formattedFractionalPart.EndsWith(","))
        {
            formattedFractionalPart = formattedFractionalPart.Substring(0, formattedFractionalPart.Length - 1);
        }

        // 合并整数和小数部分
        string formattedNumber = integerPart;

        if (!string.IsNullOrEmpty(formattedFractionalPart))
        {
            formattedNumber += "." + formattedFractionalPart;
        }

        // 确保总共十位有效数字
        int totalLength = integerPart.Length + formattedFractionalPart.Length;
        if (totalLength > 10)
        {
            if (formattedFractionalPart.Length > 0)
            {
                formattedNumber = formattedNumber.Substring(0, 10);
            }
            else
            {
                formattedNumber = formattedNumber.Substring(0, 10);
            }
        }

        return formattedNumber;
    }
}
