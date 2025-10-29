using CommunityToolkit.Mvvm.ComponentModel;
using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.Json;
using System.Threading;
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

    public IMessageBasedSession? ConnectedSession;
    private CancellationTokenSource probeTaskCancellation = new();

    public string? IdnString
    {
        get
        {
            ConnectedSession?.FormattedIO.WriteLine("*IDN?");
            return ConnectedSession?.FormattedIO.ReadLine() ?? "";
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

        ConnectedSession = GlobalResourceManager.Open(visaResourceName, mode, timeout) as IMessageBasedSession;
        if (ConnectedSession != null)
        {
            ConnectedSession.TimeoutMilliseconds = 2000;
            if (assertRen) SetResourceToRemote(ConnectedSession);
        }
    }

    public void Disconnect()
    {
        SetResourceToLocal(ConnectedSession);
        ConnectedSession?.Dispose();
    }

    public VisaResourceItem GetVisaResourceItemByNameViaTestConnection(string visaResourceName)
    {
        VisaResourceItem visaResourceItem = new();
        visaResourceItem.VisaResourceName = visaResourceName;
        try
        {
            IMessageBasedSession? session = GlobalResourceManager.Open(visaResourceName, AccessModes.None, 1000) as IMessageBasedSession;
            if (session != null)
            {
                string idnResponse = ",,,,";
                try
                {
                    session.FormattedIO.WriteLine("*IDN?");
                    idnResponse = session.FormattedIO.ReadLine();
                    var idnParts = idnResponse.Replace("\n", "").Replace("\r", "").Split(',');
                    visaResourceItem.IdnString = idnResponse.Replace('\0', ' ');

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

    private const string CacheDirectory = ".cache";
    
    private void EnsureCacheDirectory()
    {
        if (!Directory.Exists(CacheDirectory))
        {
            Directory.CreateDirectory(CacheDirectory);
        }
    }

    private string GetCacheFilePath(string visaResourceName)
    {
        return Path.Combine(CacheDirectory, $"{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(visaResourceName))}.cache");
    }

    private VisaResourceItem? LoadFromCache(string visaResourceName)
    {
        var path = GetCacheFilePath(visaResourceName);
        if (File.Exists(path))
        {
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(VisaResourceItem));
                using var stream = File.OpenRead(path);
                return serializer.Deserialize(stream) as VisaResourceItem;
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    private void SaveToCache(VisaResourceItem item)
    {
        var path = GetCacheFilePath(item.VisaResourceName);
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(VisaResourceItem));
        using var stream = File.Create(path);
        serializer.Serialize(stream, item);
    }

    public VISAResourceManagerModel()
    {
        EnsureCacheDirectory();
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
            var cachedItem = LoadFromCache(v);
            var item = new VisaResourceItem 
            {
                VisaResourceName = v,
                FriendlyName = cachedItem?.FriendlyName ?? "Waiting for response"
            };
            if (cachedItem != null)
            {
                item = cachedItem;
                Debug.WriteLine($"[VISAResourceManagerModel] found cache for {v}");
                item.Present = "From Cache";
            }
            VisaResourceList.Add(item);
        }
        
        for (int i = 0; i < VisaResourceList.Count; i++)
        {
            var idx = i;
            Task.Run(() => {
                Debug.WriteLine($"[VISAResourceManagerModel] started probe/update for {VisaResourceList[idx].VisaResourceName}");
                var probeResult = GetVisaResourceItemByNameViaTestConnection(VisaResourceList[idx].VisaResourceName);
                VisaResourceList[idx] = probeResult;
                SaveToCache(probeResult);
                Debug.WriteLine($"[VISAResourceManagerModel] completed probe/update for {VisaResourceList[idx].VisaResourceName}");
            });
        }
    }
}
