using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;
using WinCheck.Core.Services.AI;

namespace WinCheck.Infrastructure.Services;

public class NetworkMonitorService : INetworkMonitorService
{
    private readonly IAIProvider? _aiProvider;
    private readonly Dictionary<string, NetworkConnection> _connectionCache = new();
    private readonly HashSet<string> _blockedIPs = new();

    public NetworkMonitorService(IAIProvider? aiProvider = null)
    {
        _aiProvider = aiProvider;
    }

    public IObservable<NetworkActivity> MonitorNetworkActivity()
    {
        return Observable.Interval(TimeSpan.FromSeconds(2))
            .SelectMany(async _ => await DetectAnomalousConnectionsAsync())
            .SelectMany(activities => activities);
    }

    public async Task<IEnumerable<NetworkConnection>> GetActiveConnectionsAsync()
    {
        var connections = new List<NetworkConnection>();

        // Get TCP connections
        var tcpConnections = GetTcpConnections();
        connections.AddRange(tcpConnections);

        // Get UDP connections
        var udpConnections = GetUdpConnections();
        connections.AddRange(udpConnections);

        // Enrich with process info
        foreach (var conn in connections)
        {
            try
            {
                var process = Process.GetProcessById(conn.ProcessId);
                conn.ProcessName = process.ProcessName;
            }
            catch { }
        }

        return connections;
    }

    public async Task<ThreatAssessment> AnalyzeConnectionAsync(NetworkConnection connection)
    {
        var assessment = new ThreatAssessment();

        // Heuristic analysis
        var heuristicScore = AnalyzeHeuristically(connection);
        assessment.Score = heuristicScore;

        // AI analysis (if available)
        if (_aiProvider != null && _aiProvider.IsConfigured)
        {
            try
            {
                var aiAnalysis = await AnalyzeWithAIAsync(connection);
                assessment.AIExplanation = aiAnalysis;
                // Adjust score based on AI
                assessment.Score = Math.Max(assessment.Score, ExtractScoreFromAI(aiAnalysis));
            }
            catch
            {
                // AI failed, use heuristic only
            }
        }

        // Determine threat level
        assessment.Level = assessment.Score switch
        {
            >= 80 => ThreatLevel.Critical,
            >= 60 => ThreatLevel.High,
            >= 40 => ThreatLevel.Medium,
            >= 20 => ThreatLevel.Low,
            _ => ThreatLevel.None
        };

        return assessment;
    }

    public async Task<bool> BlockConnectionAsync(NetworkConnection connection)
    {
        try
        {
            // Add to Windows Firewall
            var ruleName = $"WinCheck_Block_{connection.RemoteIP}";

            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"advfirewall firewall add rule name=\"{ruleName}\" dir=out action=block remoteip={connection.RemoteIP}",
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            var process = Process.Start(psi);
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                _blockedIPs.Add(connection.RemoteIP);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<NetworkStatistics> GetNetworkStatisticsAsync()
    {
        var stats = new NetworkStatistics();

        var connections = await GetActiveConnectionsAsync();
        stats.ActiveConnections = connections.Count();

        // Calculate bandwidth by process
        var byProcess = connections
            .GroupBy(c => c.ProcessName)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(c => c.BytesSent + c.BytesReceived)
            )
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        stats.TopProcessesByBandwidth = byProcess;
        stats.TotalBytesSent = connections.Sum(c => c.BytesSent);
        stats.TotalBytesReceived = connections.Sum(c => c.BytesReceived);
        stats.BlockedConnections = _blockedIPs.Count;

        return stats;
    }

    private async Task<List<NetworkActivity>> DetectAnomalousConnectionsAsync()
    {
        var activities = new List<NetworkActivity>();
        var connections = await GetActiveConnectionsAsync();

        foreach (var conn in connections)
        {
            if (IsAnomalous(conn))
            {
                var assessment = await AnalyzeConnectionAsync(conn);

                if (assessment.Level >= ThreatLevel.Medium)
                {
                    activities.Add(new NetworkActivity
                    {
                        Connection = conn,
                        ThreatLevel = assessment.Level,
                        AIAnalysis = assessment,
                        RecommendedAction = assessment.Level >= ThreatLevel.High
                            ? RecommendedAction.Block
                            : RecommendedAction.Monitor
                    });
                }
            }
        }

        return activities;
    }

    private bool IsAnomalous(NetworkConnection conn)
    {
        // Heuristic checks

        // 1. Known malicious ports
        var suspiciousPorts = new[] { 4444, 5555, 6666, 31337, 12345 };
        if (suspiciousPorts.Contains(conn.RemotePort))
            return true;

        // 2. Large data transfer (> 100 MB)
        if (conn.BytesSent > 100 * 1024 * 1024)
            return true;

        // 3. Connection to suspicious countries (configurable)
        var suspiciousCountries = new[] { "CN", "RU", "KP" };
        if (suspiciousCountries.Contains(conn.GeoLocation.Country) && !IsKnownService(conn))
            return true;

        // 4. Non-standard process making connections
        var systemProcesses = new[] { "svchost", "System", "services" };
        if (!systemProcesses.Contains(conn.ProcessName) &&
            conn.RemotePort == 443 &&
            conn.BytesSent > 10 * 1024 * 1024)
            return true;

        return false;
    }

    private bool IsKnownService(NetworkConnection conn)
    {
        var knownServices = new[]
        {
            "chrome", "firefox", "msedge", "steam", "spotify",
            "discord", "teams", "slack", "zoom"
        };

        return knownServices.Any(s => conn.ProcessName.ToLower().Contains(s));
    }

    private double AnalyzeHeuristically(NetworkConnection conn)
    {
        double score = 0;

        // Port-based scoring
        var dangerousPorts = new[] { 4444, 5555, 6666, 31337, 12345, 1337 };
        if (dangerousPorts.Contains(conn.RemotePort))
            score += 30;

        // Data volume scoring
        var totalData = conn.BytesSent + conn.BytesReceived;
        if (totalData > 1024 * 1024 * 1024) // > 1 GB
            score += 20;
        else if (totalData > 100 * 1024 * 1024) // > 100 MB
            score += 10;

        // Geographic scoring
        if (conn.GeoLocation.Country == "CN" || conn.GeoLocation.Country == "RU")
        {
            if (!IsKnownService(conn))
                score += 25;
        }

        // Process scoring
        if (string.IsNullOrEmpty(conn.ProcessName))
            score += 15;

        // Upload-heavy connection (potential data exfiltration)
        if (conn.BytesSent > conn.BytesReceived * 3 && conn.BytesSent > 50 * 1024 * 1024)
            score += 30;

        return Math.Min(score, 100);
    }

    private async Task<string> AnalyzeWithAIAsync(NetworkConnection conn)
    {
        if (_aiProvider == null || !_aiProvider.IsConfigured)
            return string.Empty;

        var prompt = $@"
Analyze this network connection for potential security threats:

Process: {conn.ProcessName} (PID: {conn.ProcessId})
Remote IP: {conn.RemoteIP}
Remote Port: {conn.RemotePort}
Country: {conn.GeoLocation.Country}
City: {conn.GeoLocation.City}
ISP: {conn.GeoLocation.ISP}
Protocol: {conn.Protocol}
Data Sent: {conn.BytesSent / 1024 / 1024:F2} MB
Data Received: {conn.BytesReceived / 1024 / 1024:F2} MB
Duration: {conn.Duration.TotalMinutes:F1} minutes

Is this connection:
1. Normal/Expected
2. Potentially suspicious
3. Likely malicious

Provide:
- Threat score (0-100)
- Reasoning
- Potential threat type (if malicious)

Format: SCORE: [number] | REASONING: [text]
";

        try
        {
            var response = await _aiProvider.CompleteAsync(prompt, new AICompletionOptions
            {
                Temperature = 0.3, // Lower temperature for more consistent analysis
                MaxTokens = 500
            });

            return response;
        }
        catch
        {
            return string.Empty;
        }
    }

    private double ExtractScoreFromAI(string aiResponse)
    {
        try
        {
            // Parse "SCORE: 75 | REASONING: ..."
            var scoreIndex = aiResponse.IndexOf("SCORE:");
            if (scoreIndex == -1) return 0;

            var scoreStr = aiResponse.Substring(scoreIndex + 6);
            var pipeIndex = scoreStr.IndexOf("|");
            if (pipeIndex > 0)
                scoreStr = scoreStr.Substring(0, pipeIndex);

            scoreStr = scoreStr.Trim();
            if (double.TryParse(scoreStr, out var score))
                return score;
        }
        catch { }

        return 0;
    }

    private List<NetworkConnection> GetTcpConnections()
    {
        var connections = new List<NetworkConnection>();

        try
        {
            var tcpTable = GetTcpTable();

            foreach (var row in tcpTable)
            {
                connections.Add(new NetworkConnection
                {
                    ProcessId = row.owningPid,
                    LocalIP = new IPAddress(row.localAddr).ToString(),
                    LocalPort = ntohs((ushort)row.localPort),
                    RemoteIP = new IPAddress(row.remoteAddr).ToString(),
                    RemotePort = ntohs((ushort)row.remotePort),
                    Protocol = "TCP",
                    State = ((TcpState)row.state).ToString()
                });
            }
        }
        catch { }

        return connections;
    }

    private List<NetworkConnection> GetUdpConnections()
    {
        var connections = new List<NetworkConnection>();

        try
        {
            var udpTable = GetUdpTable();

            foreach (var row in udpTable)
            {
                connections.Add(new NetworkConnection
                {
                    ProcessId = row.owningPid,
                    LocalIP = new IPAddress(row.localAddr).ToString(),
                    LocalPort = ntohs((ushort)row.localPort),
                    Protocol = "UDP",
                    State = "Listening"
                });
            }
        }
        catch { }

        return connections;
    }

    // P/Invoke for GetExtendedTcpTable
    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(
        IntPtr pTcpTable,
        ref int dwOutBufLen,
        bool sort,
        int ipVersion,
        TCP_TABLE_CLASS tblClass,
        int reserved);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedUdpTable(
        IntPtr pUdpTable,
        ref int dwOutBufLen,
        bool sort,
        int ipVersion,
        UDP_TABLE_CLASS tblClass,
        int reserved);

    private enum TCP_TABLE_CLASS
    {
        TCP_TABLE_OWNER_PID_ALL = 5
    }

    private enum UDP_TABLE_CLASS
    {
        UDP_TABLE_OWNER_PID = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPROW_OWNER_PID
    {
        public uint state;
        public uint localAddr;
        public uint localPort;
        public uint remoteAddr;
        public uint remotePort;
        public int owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_UDPROW_OWNER_PID
    {
        public uint localAddr;
        public uint localPort;
        public int owningPid;
    }

    private enum TcpState
    {
        Closed = 1,
        Listen = 2,
        SynSent = 3,
        SynReceived = 4,
        Established = 5,
        FinWait1 = 6,
        FinWait2 = 7,
        CloseWait = 8,
        Closing = 9,
        LastAck = 10,
        TimeWait = 11,
        DeleteTcb = 12
    }

    private static ushort ntohs(ushort netshort)
    {
        return (ushort)((netshort >> 8) | (netshort << 8));
    }

    private List<MIB_TCPROW_OWNER_PID> GetTcpTable()
    {
        var rows = new List<MIB_TCPROW_OWNER_PID>();
        int bufferSize = 0;

        // Get buffer size
        GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, false, 2, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);

        IntPtr tcpTablePtr = Marshal.AllocHGlobal(bufferSize);

        try
        {
            if (GetExtendedTcpTable(tcpTablePtr, ref bufferSize, false, 2, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0) == 0)
            {
                int rowCount = Marshal.ReadInt32(tcpTablePtr);
                IntPtr rowPtr = (IntPtr)((long)tcpTablePtr + Marshal.SizeOf(typeof(int)));

                for (int i = 0; i < rowCount; i++)
                {
                    var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                    rows.Add(row);
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID)));
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(tcpTablePtr);
        }

        return rows;
    }

    private List<MIB_UDPROW_OWNER_PID> GetUdpTable()
    {
        var rows = new List<MIB_UDPROW_OWNER_PID>();
        int bufferSize = 0;

        GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, false, 2, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);

        IntPtr udpTablePtr = Marshal.AllocHGlobal(bufferSize);

        try
        {
            if (GetExtendedUdpTable(udpTablePtr, ref bufferSize, false, 2, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0) == 0)
            {
                int rowCount = Marshal.ReadInt32(udpTablePtr);
                IntPtr rowPtr = (IntPtr)((long)udpTablePtr + Marshal.SizeOf(typeof(int)));

                for (int i = 0; i < rowCount; i++)
                {
                    var row = Marshal.PtrToStructure<MIB_UDPROW_OWNER_PID>(rowPtr);
                    rows.Add(row);
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(MIB_UDPROW_OWNER_PID)));
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(udpTablePtr);
        }

        return rows;
    }
}
