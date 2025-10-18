using CommunityToolkit.Mvvm.ComponentModel;
using Ivi.Visa;
using NationalInstruments.Visa;
using Keysight.Visa;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
        [ObservableProperty]
        private string _visaLibrary = "";
    }

    //private NationalInstruments.Visa.ResourceManager GlobalResourceManager = new();

    public string VisaManufacture => "Unknown";
    public Version VisaLibraryVersion => GlobalResourceManager.ImplementationVersion;
    public Version VisaSpecificationVersion => GlobalResourceManager.SpecificationVersion;

    public ObservableCollection<VisaResourceItem> VisaResourceList = new();

    private void GetResourceIdentString(int visaAddressItemIndex, ObservableCollection<VisaResourceItem> visaAddressItemCollection)
    {
        var visaAddressItem = visaAddressItemCollection[visaAddressItemIndex];
        try
        {
            IMessageBasedSession? session = GlobalResourceManager.Open(visaAddressItem.VisaResourceName, AccessModes.None, 2000) as IMessageBasedSession;
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
                        visaAddressItem.Present = "No: Not a Valid SCPI Response";
                        visaAddressItem.FriendlyName = "Unknown";
                    }

                }
                catch (Exception ex)
                {
                    visaAddressItem.Present = $"No: {ex.Message}";
                    visaAddressItem.FriendlyName = "No Response";
                }

                visaAddressItem.VisaResourceName = visaAddressItem.VisaResourceName;
                visaAddressItem.VisaLibrary = session.ResourceManufacturerName;
                visaAddressItem.Interface = session.HardwareInterfaceType.ToString().ToUpper();
            }
        }
        catch (Exception ex)
        {
            visaAddressItem.Present = $"No: {ex.Message}";
            visaAddressItem.FriendlyName = "Not Presented";
        }

        System.Diagnostics.Debug.WriteLine($"[VISAResourceManagerModel] Completed fetching {visaAddressItemIndex} IDN for {visaAddressItem.VisaResourceName}: \"{visaAddressItem.FriendlyName}\"");
    }

    public VISAResourceManagerModel()
    {

        List<string> resources;
        try
        {
            resources = GlobalResourceManager
                .Find("?*")
                .Where(v => v.Contains("::INSTR"))
                .ToList();
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
                Present = "",
                VisaLibrary = "",
            };
            VisaResourceList.Add(visaAddressItem);
            var index = VisaResourceList.Count - 1;
            Task.Run(() => { GetResourceIdentString(index, VisaResourceList); });
        }
    }
}
