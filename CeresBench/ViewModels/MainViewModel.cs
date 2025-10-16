using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using CeresBench.Models;

namespace CeresBench.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public class VisaAddressItem
    {
        public string FriendlyName { get; set; }

        public string VisaResourceName { get; set; }

        public VisaAddressItem(string visaResoucreName, string friendlyName = "Unknown")
        {
            FriendlyName = friendlyName;
            VisaResourceName = visaResoucreName;
        }
    }

    public string Greeting => "Welcome to Avalonia!";


    private VISAResourceManagerModel _visaResourceManagerModel = new();

    public string CurrentlyUsedVISALibrary => _visaResourceManagerModel.VisaManufacture;
    public string CurrentlyUsedVISAVersion => _visaResourceManagerModel.VisaLibraryVersion.ToString();
    public string CurrentlyUsedVISASpecification => _visaResourceManagerModel.VisaSpecificationVersion.ToString();

    public List<VisaAddressItem> VisaResourceList = new();

}
