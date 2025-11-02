using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

public interface INetworkMonitorService
{
    /// <summary>
    /// Monitor all network connections in real-time
    /// </summary>
    IObservable<NetworkActivity> MonitorNetworkActivity();

    /// <summary>
    /// Get all active connections
    /// </summary>
    Task<IEnumerable<NetworkConnection>> GetActiveConnectionsAsync();

    /// <summary>
    /// Analyze connection for threats using AI
    /// </summary>
    Task<ThreatAssessment> AnalyzeConnectionAsync(NetworkConnection connection);

    /// <summary>
    /// Block a connection/process
    /// </summary>
    Task<bool> BlockConnectionAsync(NetworkConnection connection);

    /// <summary>
    /// Get network statistics
    /// </summary>
    Task<NetworkStatistics> GetNetworkStatisticsAsync();
}
