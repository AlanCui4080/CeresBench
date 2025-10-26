using CeresBench.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ivi.Visa;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static CeresBench.Models.VISAResourceManagerModel;

namespace CeresBench.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private VISAResourceManagerModel _visaResourceManagerModel = new();

    [ObservableProperty]
    private bool _isVisaExclusiveAccess = true;
    [ObservableProperty]
    private bool _isVisaAssertREN = true;

    [ObservableProperty]
    private int _visaTimeout = 1000;
    [ObservableProperty]
    private int _terminationCharListSelectedIndex = 0;
    [ObservableProperty]
    private string[] _terminationCharList = { "\\n", "\\r", "\\r\\n", "None" };

    [ObservableProperty]
    private string _visaCustomResourceName = "";
    [ObservableProperty]
    private bool _isVisaResouceComboDropDownOpen = false;
    [ObservableProperty]
    private int _visaResourceSelectedIndex = -1;
    partial void OnVisaResourceSelectedIndexChanged(int value)
    {
        CurrentlySelectedResource = _visaResourceManagerModel.VisaResourceList[value == -1 ? 0 : value];
    }
    public ObservableCollection<VisaResourceItem> VisaResourceList => _visaResourceManagerModel.VisaResourceList;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisaResourceList))]
    public VisaResourceItem _currentlySelectedResource = new();
    [ObservableProperty]
    private bool _isCustomVisaResourceName = false;
    partial void OnIsCustomVisaResourceNameChanged(bool value)
    {
        if (IsCustomVisaResourceName)
        {
            CurrentlySelectedResource = _visaResourceManagerModel.GetVisaResourceItemByNameViaTestConnection(VisaCustomResourceName);
        }
        else
        {
            VisaResourceSelectedIndex = 0;
            CurrentlySelectedResource =
                _visaResourceManagerModel.VisaResourceList[VisaResourceSelectedIndex == -1 ? 0 : VisaResourceSelectedIndex];
        }
    }
    [RelayCommand]
    private void CustomVisaResourceAddressEntered()
    {
        OnIsCustomVisaResourceNameChanged(IsCustomVisaResourceName);
    }

    //public string CurrentlyUsedVISALibrary => _visaResourceManagerModel.VisaManufacture;
    public string CurrentlyUsedVISAVersion => _visaResourceManagerModel.VisaLibraryVersion.ToString();
    public string CurrentlyUsedVISASpecification => _visaResourceManagerModel.VisaSpecificationVersion.ToString();

    [ObservableProperty]
    private bool _isConnectedToResource;

    [ObservableProperty]
    ViewModelBase? _applicationView;
    [ObservableProperty]
    string _applicationName = "";
    [ObservableProperty]
    string _applicationVersion = "";
    [ObservableProperty]
    string _applicationMatchedBy = "";

    [RelayCommand]
    private void ConnectToResource()
    {
        string toConnectedResourceName;
        try
        {
            if (IsConnectedToResource)
            {
                if (IsCustomVisaResourceName)
                {
                    toConnectedResourceName = VisaCustomResourceName ?? "";
                }
                else
                {
                    if (VisaResourceSelectedIndex != -1)
                    {
                        toConnectedResourceName = _visaResourceManagerModel.VisaResourceList[VisaResourceSelectedIndex].VisaResourceName;
                    }
                    else
                    {
                        IsConnectedToResource = false;
                        PopExceptionOut("No Resource Selected");
                        return;
                    }
                }
                _visaResourceManagerModel.Connect(toConnectedResourceName,
                                                  IsVisaExclusiveAccess ? AccessModes.ExclusiveLock : AccessModes.None,
                                                  0,
                                                  IsVisaAssertREN);
                ApplicationView = GetMatchedViewModel(_visaResourceManagerModel.IdnString ?? "")
                                 .GetConstructor(Array.Empty<Type>())
                                 ?.Invoke(Array.Empty<Type>()) as ViewModelBase;
                var model = ApplicationView as ViewModels.Application.GenericDMMViewModel;
                if (model != null)
                {
                    model.ResourceManagerModel = _visaResourceManagerModel;
                }
                ApplicationName = ApplicationView?.GetType().Name ?? "";
                ApplicationVersion = "";
                ApplicationMatchedBy = "Ident String";
            }
            else
            {
                _visaResourceManagerModel.Disconnect();

                ApplicationView = new ViewModels.Application.DefaultViewModel();
                ApplicationName = "";
                ApplicationVersion = "";
                ApplicationMatchedBy = "";
            }
        }
        catch (Exception ex)
        {
            IsConnectedToResource = false;
            if (ex is Ivi.Visa.IOTimeoutException && IsVisaExclusiveAccess)
            {
                PopExceptionOut($"{ex.Message} This may caused by a Lock which cannot be acquired.");
            }
            else
            {
                PopExceptionOut(ex.Message);
            }
        }
    }
    [ObservableProperty]
    private bool _isPopExcetionMessage;
    [ObservableProperty]
    private string _popExceptionMessage = "";
    [RelayCommand]
    private void PopExceptionOut(string message)
    {
        Task.Run(() =>
        {
            PopExceptionMessage = $"Error:\n{message}";
            IsPopExcetionMessage = true;
            Thread.Sleep(5000);
            IsPopExcetionMessage = false;
        });
    }

    private Type GetMatchedViewModel(string idnString)
    {
        XmlDocument xmlDocument = new();
        xmlDocument.Load("./Configs/ModelApplications.xml");
        var rootElement = xmlDocument.DocumentElement;

        if (rootElement == null)
        {
            throw new NotImplementedException();
        }

        foreach (XmlNode? node in rootElement.ChildNodes)
        {
            if (node?.Name == "Application")
            {
                foreach (XmlNode? childNode in node.ChildNodes)
                {
                    if (childNode?.Name == "MatchRule")
                    {
                        if (Regex.IsMatch(idnString, childNode?.InnerText ?? ""))
                        {
                            switch (node.Attributes?.GetNamedItem("name")?.Value)
                            {
                                case "GenericDMM":
                                    return typeof(ViewModels.Application.GenericDMMViewModel);
                                case "GenericCounter":
                                    return typeof(ViewModels.Application.GenericCounterViewModel);
                            }
                        }
                    }
                }

            }
        }

        return typeof(ViewModels.Application.DefaultViewModel);
    }
}
