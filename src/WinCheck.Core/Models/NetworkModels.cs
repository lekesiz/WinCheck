using System;
using System.Collections.Generic;

namespace WinCheck.Core.Models;

public class NetworkConnection
{
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public string LocalIP { get; set; } = string.Empty;
    public int LocalPort { get; set; }
    public string RemoteIP { get; set; } = string.Empty;
    public int RemotePort { get; set; }
    public string Protocol { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public DateTime EstablishedTime { get; set; }
    public TimeSpan Duration => DateTime.Now - EstablishedTime;
    public GeoLocation GeoLocation { get; set; } = new();
}

public class GeoLocation
{
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ISP { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class NetworkActivity
{
    public NetworkConnection Connection { get; set; } = new();
    public ThreatLevel ThreatLevel { get; set; }
    public ThreatAssessment? AIAnalysis { get; set; }
    public RecommendedAction RecommendedAction { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.Now;
}

public class ThreatAssessment
{
    public double Score { get; set; } // 0-100
    public ThreatLevel Level { get; set; }
    public List<string> Reasons { get; set; } = new();
    public string AIExplanation { get; set; } = string.Empty;
    public bool IsKnownThreat { get; set; }
    public string? MalwareFamily { get; set; }
}

public enum ThreatLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public class NetworkStatistics
{
    public long TotalBytesSent { get; set; }
    public long TotalBytesReceived { get; set; }
    public int ActiveConnections { get; set; }
    public int SuspiciousConnections { get; set; }
    public int BlockedConnections { get; set; }
    public Dictionary<string, long> TopProcessesByBandwidth { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
