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
        var devices = CaptureDeviceList.Instance;

        foreach (var dev in devices)
        {
            var deviceData = new DeviceData
            {
                Name = dev.Name,
                Description = dev.Description,
                MACAddress = dev.MacAddress != null ? dev.MacAddress.ToString() : "N/A"
            };

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
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

        var device = devices.FirstOrDefault(d =>
            d.Name == selectedDeviceData.Name ||
            d.Description == selectedDeviceData.Description);

        if (device == null)
            return;

        device.Open(DeviceModes.Promiscuous, 1000);
        device.OnPacketArrival += (sender, e) => Device_OnPacketArrival(sender, e, vm);
        device.StartCapture();
    }

    private static int packetCount = 0;
    private static void Device_OnPacketArrival(object sender, PacketCapture e, MainViewModel vm)
    {
        var rawCapture = e.GetPacket();
        var packet = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

        var ethernetPacket = packet.Extract<EthernetPacket>();
        var ipv4Packet = packet.Extract<IPv4Packet>();
        var tcpPacket = packet.Extract<TcpPacket>();
        var udpPacket = packet.Extract<UdpPacket>();

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
            PacketID = packetCount++
        };

        if (tcpPacket?.PayloadData?.Length > 0)
        {
            try
            {
                var httpText = Encoding.ASCII.GetString(tcpPacket.PayloadData);
                if (httpText.StartsWith("GET") || httpText.StartsWith("POST") || httpText.StartsWith("PUT") || httpText.StartsWith("DELETE") || httpText.StartsWith("HEAD") || httpText.StartsWith("HTTP"))
                {
                    packetData.HTTPRequest = httpText;
                    packetData.HTTPMethod = httpText.Split(' ')[0];
                }
            }
            catch (Exception ex)
            {
                packetData.HTTPRequest = $"Error parsing HTTP payload: {ex.Message}";
            }
        }

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.CapturedPackets.Add(packetData);
        });
    }
}