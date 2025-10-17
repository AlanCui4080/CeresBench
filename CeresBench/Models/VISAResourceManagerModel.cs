using CeresBench.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;

namespace CeresBench.Models;

public partial class VISAResourceManagerModel
{
    private ResourceManager GlobalResourceManager = new ResourceManager();

    public string VisaManufacture => GlobalResourceManager.ManufacturerName;
    public Version VisaLibraryVersion => GlobalResourceManager.ImplementationVersion;
    public Version VisaSpecificationVersion => GlobalResourceManager.SpecificationVersion;

    public List<MainViewModel.VisaAddressItem> Find()
    {
        return GlobalResourceManager.Find("?*").Select(x => new MainViewModel.VisaAddressItem(x)).ToList();
    }
}
