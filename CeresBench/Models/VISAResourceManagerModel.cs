using CeresBench.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Ivi.Visa;

namespace CeresBench.Models;

public partial class VISAResourceManagerModel
{
    private ResourceManager GlobalResourceManager = new ResourceManager();

    public string VisaManufacture => GlobalResourceManager.ManufacturerName;
    public Version VisaLibraryVersion => GlobalResourceManager.ImplementationVersion;
    public Version VisaSpecificationVersion => GlobalResourceManager.SpecificationVersion;

    public List<MainViewModel.VisaAddressItem> Find()
    {
        try
        {
            return GlobalResourceManager.Find("?*").Select(x => new MainViewModel.VisaAddressItem(x)).ToList();
        }
        catch (NativeVisaException ex)
        {
            switch (ex.ErrorCode)
            {
                case NativeErrorCode.ResourceNotFound: // we don't see no instrument on device as an error
                    return new List<MainViewModel.VisaAddressItem>();
                default:
                    throw;
            }
        }
    }
}
