using System;

namespace Network_Traffic_Analyzer.Models
{
    public class PacketData
    {
        public int PacketID { get; set; }
        public DateTime Timestamp { get; set; }

        public string SourceIP { get; set; }
        public string SourceMAC { get; set; }

        public string DestinationIP { get; set; }
        public string DestinationMAC { get; set; }

        public string Protocol { get; set; }  // TCP, UDP, ICMP, ARP, etc...

        public int? SourcePort { get; set; }
        public int? DestinationPort { get; set; }

        public int PacketSize { get; set; }
        public string HTTPRequest { get; set; }
        public string HTTPMethod { get; set; }
    }
}