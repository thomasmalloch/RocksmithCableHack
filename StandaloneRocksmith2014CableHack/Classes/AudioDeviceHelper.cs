using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace RocksmithCableHack;

[SupportedOSPlatform("windows")]
public static class AudioDeviceHelper
{
    public static List<AudioDevice> GetUSBAudioDevices(AppSettings? settings = null)
    {
        var devices = new List<AudioDevice>();
        var seen = new HashSet<string>();
        try
        {
            var type = Type.GetTypeFromCLSID(CoreAudioConstants.MMDeviceEnumeratorClsid);
            if(type is null)
                return devices;

            var enumerator = (IMMDeviceEnumerator)Activator.CreateInstance(type)!;
            if(enumerator.EnumAudioEndpoints(EDataFlow.All, CoreAudioConstants.DEVICE_STATE_ACTIVE, out var collection) != 0)
                return devices;

            if(collection.GetCount(out uint count) != 0)
                return devices;

            for(uint i = 0; i < count; i++)
            {
                try
                {
                    if(collection.Item(i, out var mmDevice) != 0)
                        continue;

                    if(mmDevice.GetId(out string endpointId) != 0)
                        continue;

                    if(mmDevice.OpenPropertyStore(CoreAudioConstants.STGM_READ, out var properties) != 0)
                        continue;

                    string? usbPath = FindUSBDevicePath(properties);
                    if(usbPath is null)
                        continue;

                    string? vid = ExtractHex(usbPath, @"(?<=VID_)([\dA-Fa-f]{4})");
                    string? pid = ExtractHex(usbPath, @"(?<=PID_)([\dA-Fa-f]{4})");
                    if(vid is null || pid is null)
                        continue;

                    string dedupeKey = $"{vid}:{pid}";
                    if(!seen.Add(dedupeKey))
                        continue;

                    string name = GetFriendlyName(properties) ?? "Unknown USB Audio Device";
                    devices.Add(new AudioDevice(endpointId, name, vid, pid));
                }
                catch
                {
                }
            }
        }
        catch
        {
        }

        // if the saved device isn't in the connected list, add a disconnected placeholder
        if((settings is not null) &&
            (!string.IsNullOrEmpty(settings.SelectedDeviceVid)) &&
            (!string.IsNullOrEmpty(settings.SelectedDevicePid)))
        {
            string savedKey = $"{settings.SelectedDeviceVid}:{settings.SelectedDevicePid}";
            if(!seen.Contains(savedKey))
            {
                devices.Add(AudioDevice.CreateDisconnected(
                    settings.SelectedDeviceName,
                    settings.SelectedDeviceVid,
                    settings.SelectedDevicePid));
            }
        }

        return devices;
    }

    private static string? GetFriendlyName(IPropertyStore store)
    {
        return GetPropertyString(store, PropertyKeys.DeviceInterfaceFriendlyName)
            ?? GetPropertyString(store, PropertyKeys.DeviceFriendlyName)
            ?? GetPropertyString(store, PropertyKeys.DeviceDescription);
    }

    private static string? GetPropertyString(IPropertyStore store, PROPERTYKEY key)
    {
        var variant = new PROPVARIANT();
        try
        {
            if(store.GetValue(ref key, out variant) != 0)
                return null;

            return variant.AsString();
        }
        finally
        {
            variant.Dispose();
        }
    }

    private static string? FindUSBDevicePath(IPropertyStore store)
    {
        if(store.GetCount(out uint count) != 0)
            return null;

        for(uint i = 0; i < count; i++)
        {
            if(store.GetAt(i, out var key) != 0)
                continue;

            var variant = new PROPVARIANT();
            try
            {
                if(store.GetValue(ref key, out variant) != 0)
                    continue;

                string? value = variant.AsString();
                if(value is null)
                    continue;

                if(value.Contains("VID_", StringComparison.OrdinalIgnoreCase) &&
                    value.Contains("PID_", StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }
            finally
            {
                variant.Dispose();
            }
        }

        return null;
    }

    private static string? ExtractHex(string input, string pattern)
    {
        var match = Regex.Match(input, pattern);
        return match.Success ? match.Value : null;
    }
}
