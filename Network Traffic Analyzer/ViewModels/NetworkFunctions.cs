using Avalonia.Threading;
using Network_Traffic_Analyzer.Models;
using Network_Traffic_Analyzer.ViewModels;
using PacketDotNet;
using SharpPcap;
using System;
using System.Linq;
using System.Text;

public static class NetworkFunctions
{
    public static async void LoadDevices(MainViewModel vm)
    {
        // All capture devices on the system
        var devices = CaptureDeviceList.Instance;

        // Loops through every device found
        foreach (var dev in devices)
        {
            // Create's a new object for device data for every device in the list
            var deviceData = new DeviceData
            {
                Name = dev.Name,
                Description = dev.Description,
                MACAddress = dev.MacAddress?.ToString() ?? "N/A"
            };

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Only add the device if it's not in the list already
                if (!vm.NetworkDevices.Any(d => d.Name == deviceData.Name))
                    vm.NetworkDevices.Add(deviceData);
            });
        }
    }

    public static async void AnalyzeTraffic(DeviceData selectedDeviceData, MainViewModel vm)
    {
        if (selectedDeviceData == null)
            return;

        var devices = CaptureDeviceList.Instance;

        // Find the device, which matches the selectedDeviceData
        var device = devices.FirstOrDefault(d =>
            d.Name == selectedDeviceData.Name ||
            d.Description == selectedDeviceData.Description);

        if (device == null)
            return;

        device.Open(DeviceModes.Promiscuous, 1000); // Set to promiscuous as it'll recieve all network traffic with a timeout of 1000
        device.OnPacketArrival += (sender, e) => Device_OnPacketArrival(sender, e, vm); // Calls Device_OnPacketArrival whenever a new packet is captured
        device.StartCapture(); // Starts capturing packets on that device
    }

    // Variable to keep track of the packet
    private static int packetCount = 0;

    private static void Device_OnPacketArrival(object sender, PacketCapture e, MainViewModel vm)
    {
        var rawCapture = e.GetPacket(); // Get the raw packet data
        var packet = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

        // Multiple variables, which tries to extract protocol specific packets from the parsed packets
        var ethernetPacket = packet.Extract<EthernetPacket>();
        var ipv4Packet = packet.Extract<IPv4Packet>();
        var tcpPacket = packet.Extract<TcpPacket>();
        var udpPacket = packet.Extract<UdpPacket>();

        // Creates a new PacketData object to hold info, which will be displayed in the GUI
        var packetData = new PacketData
        {
            Timestamp = rawCapture.Timeval.Date,
            SourceMAC = ethernetPacket?.SourceHardwareAddress.ToString() ?? "N/A",
            DestinationMAC = ethernetPacket?.DestinationHardwareAddress.ToString() ?? "N/A",
            SourceIP = ipv4Packet?.SourceAddress.ToString() ?? "N/A",
            DestinationIP = ipv4Packet?.DestinationAddress.ToString() ?? "N/A",
            Protocol = ipv4Packet?.Protocol.ToString() ?? "Unknown",
            SourcePort = tcpPacket?.SourcePort ?? udpPacket?.SourcePort,
            DestinationPort = tcpPacket?.DestinationPort ?? udpPacket?.DestinationPort,
            PacketSize = rawCapture.Data.Length,
            TTL = ipv4Packet?.TimeToLive ?? 0,
            IPHeaderLength = ipv4Packet?.HeaderLength ?? 0,
            TCPFlags = tcpPacket != null ? tcpPacket.Flags.ToString() : "",
            PayloadSize = tcpPacket?.PayloadData?.Length ?? udpPacket?.PayloadData?.Length ?? 0,
            IsEncrypted = tcpPacket?.DestinationPort == 443 || tcpPacket?.SourcePort == 443,
            PacketID = packetCount++
        };


        // Checks if TCP payload data exists
        if (tcpPacket?.PayloadData?.Length > 0)
        {
            try
            {
                // Convert TCP payload to bytes to ASCII string
                var httpText = Encoding.ASCII.GetString(tcpPacket.PayloadData);

                // Checks if the payload starts with HTTP methods
                if (httpText.StartsWith("GET") || httpText.StartsWith("POST") ||
                    httpText.StartsWith("PUT") || httpText.StartsWith("DELETE") ||
                    httpText.StartsWith("HEAD") || httpText.StartsWith("HTTP"))
                {
                    packetData.HTTPRequest = httpText; // Store the HTTP requests
                    packetData.HTTPMethod = httpText.Split(' ')[0]; // Extracts and stores the HTTP method, e.g. GET, POST, etc...
                }
            }
            catch (Exception ex)
            {
                packetData.HTTPRequest = $"Error parsing HTTP payload: {ex.Message}";
            }
        }

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.CapturedPackets.Add(packetData); // Add packet to the collection for the UI
        });
    }
}