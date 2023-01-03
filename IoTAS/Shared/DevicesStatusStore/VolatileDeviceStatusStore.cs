//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Collections.Generic;
using Serilog;

namespace IoTAS.Shared.DevicesStatusStore;

/// <summary>
/// Volatile (in memory) implementation for IDeviceStatusStore
/// </summary>
public sealed class VolatileDeviceStatusStore : IDeviceStatusStore
{
    private readonly ILogger _logger;

    private readonly Dictionary<int, DeviceReportingStatus> _store;

    public VolatileDeviceStatusStore()
    {
        _store = new Dictionary<int, DeviceReportingStatus>();

        _logger = Log.ForContext<VolatileDeviceStatusStore>();
        _logger!.Information("Store created");
    }

    public int Count { get => _store.Count; }

    public void Clear() => _store.Clear();

    public DeviceReportingStatus GetDeviceStatus(int deviceId)
    {
        _logger.Debug(
            nameof(GetDeviceStatus) + " - " +
            "Getting " + 
            nameof(DeviceReportingStatus) + 
            " for Device {DeviceId}",
            deviceId);

        if (_store.GetValueOrDefault(deviceId) is DeviceReportingStatus status)
        {
            return status;
        }

        _logger.Warning(
            nameof(GetDeviceStatus) + " - " +
            "Attempt to retrieve a non-existing " + 
            nameof(DeviceReportingStatus) +
            " for Device {DeviceId}",
            deviceId);

        return new DeviceReportingStatus(deviceId, default, default, default);
    }

    public IEnumerable<DeviceReportingStatus> GetDevicesStatusList()
    {
        var values = _store.Values;

        _logger.Debug(
            nameof(GetDevicesStatusList) + " - " +
            "Getting Reporting Status for all {DevicesCount} Devices",
            values.Count);
            
        DeviceReportingStatus[] valuesCopy = new DeviceReportingStatus[values.Count];
        values.CopyTo(valuesCopy, 0);
            
        return valuesCopy;
    }

    public void SetDeviceStatus(DeviceReportingStatus status)
    {
        _logger.Debug(
            nameof(SetDeviceStatus) + " - " +
            "Insert or update the DeviceReportingStatus for Device {DeviceId}",
            status.DeviceId);

        _store[status.DeviceId] =  status;   // overwrites if key exists, otherwise inserts
    }

    public DeviceReportingStatus UpdateHeartbeat(int deviceId, DateTime receivedAt)
    {
        _logger.Debug(
            nameof(UpdateHeartbeat) + " - " +
            "Heartbeat at {ReceivedAt} for Device {DeviceId}",
            receivedAt, deviceId);

        DeviceReportingStatus current = 
            _store.ContainsKey(deviceId) 
                ? GetDeviceStatus(deviceId) 
                : new DeviceReportingStatus(deviceId, default, default, default);

        DeviceReportingStatus updated = current with 
        { 
            LastSeenAt = receivedAt 
        };
            
        _store[deviceId] = updated;

        return updated;
    }

    public DeviceReportingStatus UpdateRegistration(int deviceId, DateTime receivedAt)
    {
        _logger.Debug(
            nameof(UpdateRegistration) + " - " +
            "Registration at {ReceivedAt} for Device {DeviceId}",
            receivedAt, deviceId);

        DeviceReportingStatus current =
            _store.ContainsKey(deviceId)
                ? GetDeviceStatus(deviceId)  
                : new DeviceReportingStatus(deviceId, receivedAt, default, default);

        DeviceReportingStatus updated = current with
        {
            LastRegisteredAt = receivedAt,
            LastSeenAt = receivedAt
        };

        _store[deviceId] = updated;

        return updated;
    }        
}