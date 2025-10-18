using CeresBench.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Ivi.Visa;
using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using static CeresBench.ViewModels.MainViewModel;
using static System.Collections.Specialized.BitVector32;

namespace CeresBench.Models;

public partial class VISAResourceManagerModel
{
    public partial class VisaResourceItem : ObservableObject
    {
        [ObservableProperty]
        private string _friendlyName = "";
        [ObservableProperty]
        private string _visaResourceName = "";
        [ObservableProperty]
        private string _interface = "";
        [ObservableProperty]
        private string _vendor = "";
        [ObservableProperty]
        private string _model = "";
        [ObservableProperty]
        private string _serialNumber = "";
        [ObservableProperty]
        private string _firmware = "";
        [ObservableProperty]
        private string _present = "";
    }

    private NationalInstruments.Visa.ResourceManager GlobalResourceManager = new();

    public string VisaManufacture => GlobalResourceManager.ManufacturerName;
    public Version VisaLibraryVersion => GlobalResourceManager.ImplementationVersion;
    public Version VisaSpecificationVersion => GlobalResourceManager.SpecificationVersion;

    public ObservableCollection<VisaResourceItem> VisaResourceList = new();

    private void GetResourceIdentString(int visaAddressItemIndex, ObservableCollection<VisaResourceItem> visaAddressItemCollection)
    {
        var visaAddressItem = visaAddressItemCollection[visaAddressItemIndex];
        try
        {
            var session = GlobalResourceManager.Open(visaAddressItem.VisaResourceName, AccessModes.None, 2000) as MessageBasedSession;

            if (session != null)
            {
                string idnResponse = ",,,,";
                try
                {
                    session.RawIO.Write("*IDN?\n");
                    idnResponse = session.RawIO.ReadString();
                    var idnParts = idnResponse.Replace("\n", "").Replace("\r", "").Split(',');


                    if (idnParts.Length == 4)
                    {
                        var _vendorName = idnParts[0].TrimStart();
                        var _modelName = idnParts[1].TrimStart();
                        var _serialNumberString = idnParts[2].TrimStart();
                        var _firmware = idnParts[3].TrimStart();
                        var _friendlyName = $"{idnParts[0].TrimStart()} {idnParts[1].TrimStart()}";
                        visaAddressItem.Vendor = _vendorName;
                        visaAddressItem.Model = _modelName;
                        visaAddressItem.SerialNumber = _serialNumberString;
                        visaAddressItem.Firmware = _firmware;
                        visaAddressItem.FriendlyName = _friendlyName;
                        visaAddressItem.Present = "Yes";
                    }
                    else
                    {
                        visaAddressItem.FriendlyName = "Unknown";
                        visaAddressItem.Present = "Not support SCPI";

                    }

                    visaAddressItem.VisaResourceName = visaAddressItem.VisaResourceName;
                    visaAddressItem.Interface = session.HardwareInterfaceName;

                }
                catch (IOTimeoutException)
                {
                    visaAddressItem.Present = "No: Timed Out";
                    visaAddressItem.FriendlyName = "No Response";
                }

            }
        }
        catch (NativeVisaException)
        {
            visaAddressItem.Present = "No: Resource not Presented Yet";
            visaAddressItem.FriendlyName = "Not Presented";
        }

        System.Diagnostics.Debug.WriteLine($"[VISAResourceManagerModel] Completed fetching {visaAddressItemIndex} IDN for {visaAddressItem.VisaResourceName}: \"{visaAddressItem.FriendlyName}\"");
    }

    public VISAResourceManagerModel()
    {
        List<string> resources;
        try
        {
            resources = GlobalResourceManager.Find("?*").ToList();
        }
        catch (NativeVisaException ex)
        {
            switch (ex.ErrorCode)
            {
                case NativeErrorCode.ResourceNotFound: // we don't see no instrument on device as an error

                default:
                    throw;
            }
        }

        foreach (var v in resources)
        {
            var visaAddressItem = new VisaResourceItem
            {
                FriendlyName = "Waiting for Response",
                VisaResourceName = v,
                Interface = "",
                Vendor = "",
                Model = "",
                SerialNumber = "",
                Firmware = "",
                Present = ""
            };
            VisaResourceList.Add(visaAddressItem);
            var index = VisaResourceList.Count - 1;
            Task.Run(() => { GetResourceIdentString(index, VisaResourceList); });
        }
    }
}
