using CeresBench.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using static CeresBench.Models.VISAResourceManagerModel;

namespace CeresBench.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    private VISAResourceManagerModel _visaResourceManagerModel = new();

    
    [ObservableProperty]
    private bool _isVisaResouceComboDropDownOpen = false;

    [ObservableProperty]
    private int _visaResourceSelectedIndex = -1;

    public ObservableCollection<VISAResourceManagerModel.VisaResourceItem> VisaResourceList => _visaResourceManagerModel.VisaResourceList;

    [ObservableProperty]
    public VISAResourceManagerModel.VisaResourceItem _currentlySelectedResource = new();
    partial void OnVisaResourceSelectedIndexChanged(int value)
    {
        CurrentlySelectedResource = _visaResourceManagerModel.VisaResourceList[value == -1 ? 0 : value];
    }

    public string CurrentlyUsedVISALibrary => _visaResourceManagerModel.VisaManufacture;
    public string CurrentlyUsedVISAVersion => _visaResourceManagerModel.VisaLibraryVersion.ToString();
    public string CurrentlyUsedVISASpecification => _visaResourceManagerModel.VisaSpecificationVersion.ToString();

}
