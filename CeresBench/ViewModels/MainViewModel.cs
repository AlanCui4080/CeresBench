using CeresBench.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
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
    private string? _visaCustomResourceName;
    [ObservableProperty]
    private bool _isVisaResouceComboDropDownOpen = false;
    [ObservableProperty]
    private int _visaResourceSelectedIndex = -1;
    public ObservableCollection<VisaResourceItem> VisaResourceList => _visaResourceManagerModel.VisaResourceList;
    [ObservableProperty]
    public VisaResourceItem? _currentlySelectedResource;
    partial void OnVisaResourceSelectedIndexChanged(int value)
    {
        CurrentlySelectedResource = _visaResourceManagerModel.VisaResourceList[value == -1 ? 0 : value];
    }
    [ObservableProperty]
    private bool _isCustomVisaResourceName = false;
    partial void OnIsCustomVisaResourceNameChanged(bool value)
    {
        if (IsCustomVisaResourceName)
        {
            CurrentlySelectedResource = _visaResourceManagerModel.GetVisaResourceItemByName(VisaCustomResourceName ?? "");
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


    [RelayCommand]
    private void PopExceptionOut(string message)
    {

    }
}
