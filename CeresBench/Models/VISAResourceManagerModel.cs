using CommunityToolkit.Mvvm.ComponentModel;
using Ivi.Visa;
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
        private string _idnString = "";

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
        [ObservableProperty]
        private string _visaImplementionVersion = "";
        [ObservableProperty]
        private string _visaSpecificationVersion = "";
    }

    private IMessageBasedSession? _connectedSession;

    public IMessageBasedFormattedIO? FormattedIO => _connectedSession?.FormattedIO;

    public string? IdnString
    {
        get
        {
            _connectedSession?.FormattedIO.WriteLine("*IDN?");
            return _connectedSession?.FormattedIO.ReadLine() ?? "";
        }
    }

    public Version VisaLibraryVersion => GlobalResourceManager.ImplementationVersion;
    public Version VisaSpecificationVersion => GlobalResourceManager.SpecificationVersion;

    public ObservableCollection<VisaResourceItem> VisaResourceList = new();
    public void SetResourceToLocal(IVisaSession? session)
    {
        (session as IGpibSession)?.SendRemoteLocalCommand(RemoteLocalMode.Local);
        (session as IUsbSession)?.SendRemoteLocalCommand(RemoteLocalMode.Local);
        (session as ITcpipSession)?.SendRemoteLocalCommand(RemoteLocalMode.Local);
    }

    public void SetResourceToRemote(IVisaSession? session)
    {
        (session as IGpibSession)?.SendRemoteLocalCommand(RemoteLocalMode.Remote);
        (session as IUsbSession)?.SendRemoteLocalCommand(RemoteLocalMode.Remote);
        (session as ITcpipSession)?.SendRemoteLocalCommand(RemoteLocalMode.Remote);
    }

    public void Connect(string visaResourceName, AccessModes mode, int timeout, bool assertRen)
    {
        _connectedSession = GlobalResourceManager.Open(visaResourceName, mode, timeout) as IMessageBasedSession;
        _connectedSession.TimeoutMilliseconds = 2000;
        if (assertRen) SetResourceToRemote(_connectedSession);
    }

    public void Disconnect()
    {
        SetResourceToLocal(_connectedSession);
        _connectedSession?.Dispose();
    }

    public VisaResourceItem GetVisaResourceItemByNameViaTestConnection(string visaResourceName)
    {
        VisaResourceItem visaResourceItem = new();
        visaResourceItem.VisaResourceName = visaResourceName;
        try
        {
            IMessageBasedSession? session = GlobalResourceManager.Open(visaResourceName, AccessModes.None, 0) as IMessageBasedSession;
            if (session != null)
            {
                session.TimeoutMilliseconds = 0;
                string idnResponse = ",,,,";
                try
                {
                    session.FormattedIO.WriteLine("*IDN?");
                    idnResponse = session.FormattedIO.ReadLine();
                    var idnParts = idnResponse.Replace("\n", "").Replace("\r", "").Split(',');
                    visaResourceItem.IdnString = idnResponse;

                    if (idnParts.Length == 4)
                    {
                        var _vendorName = idnParts[0].TrimStart();
                        var _modelName = idnParts[1].TrimStart();
                        var _serialNumberString = idnParts[2].TrimStart();
                        var _firmware = idnParts[3].TrimStart();
                        var _friendlyName = $"{idnParts[0].TrimStart()} {idnParts[1].TrimStart()}";
                        visaResourceItem.Vendor = _vendorName;
                        visaResourceItem.Model = _modelName;
                        visaResourceItem.SerialNumber = _serialNumberString;
                        visaResourceItem.Firmware = _firmware;
                        visaResourceItem.FriendlyName = _friendlyName;
                        visaResourceItem.Present = "Yes";
                    }
                    else
                    {
                        visaResourceItem.Present = "No: Not a Valid SCPI Response";
                        visaResourceItem.FriendlyName = "Unknown";
                    }
                }
                catch (Exception ex)
                {
                    visaResourceItem.Present = $"No: {ex.Message}";
                    visaResourceItem.FriendlyName = "No Response";
                }

                visaResourceItem.VisaLibrary = session.ResourceManufacturerName;
                visaResourceItem.VisaImplementionVersion = session.ResourceImplementationVersion.ToString();
                visaResourceItem.VisaSpecificationVersion = session.ResourceSpecificationVersion.ToString();
                visaResourceItem.Interface = session.HardwareInterfaceType.ToString().ToUpper();

                // avoiding probe cause device into remote
                SetResourceToLocal(session);

                session.Dispose();
            }
        }
        catch (Exception ex)
        {
            visaResourceItem.Present = $"No: {ex.Message}";
            visaResourceItem.FriendlyName = "Not Presented";
        }
        return visaResourceItem;
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
            Task.Run(() => { VisaResourceList[index] = GetVisaResourceItemByNameViaTestConnection(VisaResourceList[index].VisaResourceName); });
        }
    }
}
