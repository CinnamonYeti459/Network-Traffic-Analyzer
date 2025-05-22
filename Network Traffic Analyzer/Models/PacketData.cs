using System;

public class PacketData
{
    public int PacketID { get; set; }
    public DateTime Timestamp { get; set; }
    public string SourceIP { get; set; }
    public string SourceMAC { get; set; }
    public string DestinationIP { get; set; }
    public string DestinationMAC { get; set; }
    public string Protocol { get; set; }
    public int? SourcePort { get; set; }
    public int? DestinationPort { get; set; }
    public int PacketSize { get; set; }
    public int TTL { get; set; }
    public int IPHeaderLength { get; set; }
    public string TCPFlags { get; set; }
    public int PayloadSize { get; set; }
    public bool IsEncrypted { get; set; }
    public double? InterArrivalTimeMs { get; set; }
    public string HTTPRequest { get; set; }
    public string HTTPMethod { get; set; }
}
