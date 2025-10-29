using CeresBench.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ivi.Visa;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static CeresBench.Models.VISAResourceManagerModel;

namespace CeresBench.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly VISAResourceManagerModel _visaResourceManagerModel = new();

    [ObservableProperty] private bool _isVisaExclusiveAccess;
    [ObservableProperty] private bool _isVisaAssertREN = true;
    [ObservableProperty] private int _visaTimeout = 1000;

    [ObservableProperty] private int _terminationCharListSelectedIndex;
    [ObservableProperty] private string[] _terminationCharList = { "\\n", "\\r", "\\r\\n", "None" };

    [ObservableProperty] private string _visaCustomResourceName = string.Empty;
    [ObservableProperty] private bool _isVisaResouceComboDropDownOpen;
    [ObservableProperty] private int _visaResourceSelectedIndex = -1;

    public ObservableCollection<VisaResourceItem> VisaResourceList => _visaResourceManagerModel.VisaResourceList;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisaResourceList))]
    private VisaResourceItem _currentlySelectedResource = new();

    [ObservableProperty] private bool _isCustomVisaResourceName;

    partial void OnVisaResourceSelectedIndexChanged(int value)
    {
        if (value < 0 || value >= _visaResourceManagerModel.VisaResourceList.Count)
            return;

        CurrentlySelectedResource = _visaResourceManagerModel.VisaResourceList[value];
    }

    partial void OnIsCustomVisaResourceNameChanged(bool value)
    {
        CurrentlySelectedResource = value
            ? _visaResourceManagerModel.GetVisaResourceItemByNameViaTestConnection(VisaCustomResourceName)
            : _visaResourceManagerModel.VisaResourceList.ElementAtOrDefault(VisaResourceSelectedIndex) ?? new();
    }

    [RelayCommand]
    private void CustomVisaResourceAddressEntered() => OnIsCustomVisaResourceNameChanged(IsCustomVisaResourceName);

    public string CurrentlyUsedVISAVersion => _visaResourceManagerModel.VisaLibraryVersion.ToString();
    public string CurrentlyUsedVISASpecification => _visaResourceManagerModel.VisaSpecificationVersion.ToString();

    [ObservableProperty] private bool _isConnectedToResource;

    [ObservableProperty] private ViewModelBase? _applicationView;
    [ObservableProperty] private string _applicationName = string.Empty;
    [ObservableProperty] private string _applicationVersion = string.Empty;
    [ObservableProperty] private string _applicationMatchedBy = string.Empty;

    [RelayCommand]
    private void ConnectToResource()
    {
        try
        {
            if (IsConnectedToResource)
            {
                string resourceName = IsCustomVisaResourceName 
                    ? VisaCustomResourceName
                    : VisaResourceSelectedIndex != -1
                        ? _visaResourceManagerModel.VisaResourceList[VisaResourceSelectedIndex].VisaResourceName
                        : throw new ArgumentOutOfRangeException(nameof(VisaResourceSelectedIndex));
                _visaResourceManagerModel.Connect(
                    resourceName,
                    IsVisaExclusiveAccess ? AccessModes.ExclusiveLock : AccessModes.None,
                    0,
                    IsVisaAssertREN
                );

                var (viewType, node) = GetMatchedViewModel(_visaResourceManagerModel.IdnString ?? "");
                ApplicationView = Activator.CreateInstance(viewType, [node, _visaResourceManagerModel.ConnectedSession]) as ViewModelBase;

                ApplicationName = ApplicationView?.GetType().Name ?? string.Empty;
                ApplicationVersion = string.Empty;
                ApplicationMatchedBy = "Ident String";
            }
            else
            {
                _visaResourceManagerModel.Disconnect();
                ApplicationView = new ViewModels.Application.DefaultViewModel();
                ApplicationName = string.Empty;
                ApplicationVersion = string.Empty;
                ApplicationMatchedBy = string.Empty;
            }
        }
        catch (Exception ex)
        {
            IsConnectedToResource = false;

            string message = ex is Ivi.Visa.IOTimeoutException && IsVisaExclusiveAccess
                ? $"{ex.Message} This may be caused by a lock that cannot be acquired."
                : ex.Message;

            Task.Run(() => { PopExceptionOut(message); });
        }
    }

    [ObservableProperty] private bool _isPopExcetionMessage;
    [ObservableProperty] private string _popExceptionMessage = string.Empty;

    [RelayCommand]
    private void PopExceptionOut(string message)
    {
        PopExceptionMessage = $"Error:\n{message}";
        IsPopExcetionMessage = true;
        Thread.Sleep(5000);
        IsPopExcetionMessage = false;
    }

    private (Type, XmlNode) GetMatchedViewModel(string idnString)
    {
        XmlDocument xml = new();
        xml.Load("./Configs/ModelApplications.xml");

        var root = xml.DocumentElement ?? throw new InvalidOperationException("Invalid ModelApplications.xml");

        foreach (XmlNode node in root.SelectNodes("Application")!)
        {
            string pattern = node.Attributes?["match"]?.Value ?? string.Empty;
            if (Regex.IsMatch(idnString, pattern))
            {
                string appName = node.Attributes?["name"]?.Value ?? string.Empty;
                return appName switch
                {
                    "GenericDMM" => (typeof(ViewModels.Application.GenericDMMViewModel), node),
                    "GenericCounter" => (typeof(ViewModels.Application.GenericCounterViewModel), node),
                    _ => throw new NotSupportedException($"Unknown Application: {appName}")
                };
            }
        }

        throw new InvalidOperationException("No matching application found for current resource.");
    }
}
