using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;

namespace StarResonanceDpsAnalysis.WinForm.Plugin
{
    public static class CaptureDeviceHelper
    {
        /// <summary>
        /// 通过创建 UDP 连接获取本地出站 IP，然后匹配拥有该 IP 的网络接口，最后匹配 SharpPcap 设备列表。
        /// 步骤：
        /// 1. 创建一个 UDP 套接字并 "连接" 到一个公共 IP（不会发送数据包），让操作系统选择出站地址。
        /// 2. 从该套接字获取本地 IP 地址。
        /// 3. 如果找不到本地 IP，使用旧方法选择最佳网络接口。
        /// 4. 遍历所有网络接口，找到包含该本地 IP 的接口。如果找不到，尝试找同一子网的接口。
        /// 如果找不到任何接口，使用旧方法选择最佳网络接口。
        /// </summary>
        public static int GetBestNetworkCardIndex(CaptureDeviceList devices)
        {
            try
            {
                var localIp = GetOutboundLocalIp();
                if (localIp == null) return GetBestNetworkCardIndexFallback(devices);

                // Find the network interface which has this local IP
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                var activeNi = interfaces
                    .FirstOrDefault(ni => ni.GetIPProperties()
                        .UnicastAddresses.Any(ua => ua.Address is { AddressFamily: AddressFamily.InterNetwork } &&
                                                     ua.Address.Equals(localIp)));

                // Fallback: try to find any interface that has an address in the same subnet
                activeNi ??= interfaces
                    .FirstOrDefault(ni => ni.GetIPProperties().UnicastAddresses
                        .Any(ua => ua.Address is { AddressFamily: AddressFamily.InterNetwork } &&
                                   AreInSameSubnet(ua.Address, localIp, ua.IPv4Mask)));

                if (activeNi == null)
                {
                    // last fallback: Use old method
                    return GetBestNetworkCardIndexFallback(devices);
                }

                // 匹配分数最高的设备
                int bestIndex = -1, bestScore = -1;
                for (var i = 0; i < devices.Count; i++)
                {
                    var score = 0;
                    if (devices[i].Description.Contains(activeNi.Name, StringComparison.OrdinalIgnoreCase)) score += 2;
                    if (devices[i].Description.Contains(activeNi.Description, StringComparison.OrdinalIgnoreCase)) score += 3;
                    if (score <= bestScore) continue;
                    bestScore = score;
                    bestIndex = i;
                }
                return bestIndex;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DetermineInterfaceByOutboundLocalIp: failed to determine outbound interface: {ex.Message}");

                return devices.Count > 0 ? 0 : -1;
            }
        }

        private static bool AreInSameSubnet(IPAddress? addr1, IPAddress? addr2, IPAddress? mask)
        {
            if (addr1 == null || addr2 == null || mask == null) return false;
            var a1 = addr1.GetAddressBytes();
            var a2 = addr2.GetAddressBytes();
            var m = mask.GetAddressBytes();
            if (a1.Length != a2.Length || a1.Length != m.Length) return false;
            return !a1.Where((t, i) => (t & m[i]) != (a2[i] & m[i])).Any();
        }

        public static IPAddress? GetOutboundLocalIp()
        {
            try
            {
                // connect to a well-known external endpoint (no packets are sent for UDP on Connect)
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect("114.114.114.114", 65530);
                if (socket.LocalEndPoint is IPEndPoint ep)
                {
                    return ep.Address;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetOutboundLocalIp failed: {ex.Message}");
            }

            return null;
        }

        private static int GetBestNetworkCardIndexFallback(CaptureDeviceList devices)
        {
            var active = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                             ni.GetIPProperties().UnicastAddresses.Any(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork))
                .OrderByDescending(ni => ni.GetIPProperties().GatewayAddresses.Count != 0) // 网关优先
                .FirstOrDefault();

            if (active == null) return devices.Count > 0 ? 0 : -1;

            // 匹配分数最高的设备
            int bestIndex = -1, bestScore = -1;
            for (int i = 0; i < devices.Count; i++)
            {
                int score = 0;
                if (devices[i].Description.Contains(active.Name, StringComparison.OrdinalIgnoreCase)) score += 2;
                if (devices[i].Description.Contains(active.Description, StringComparison.OrdinalIgnoreCase)) score += 3;
                if (score > bestScore) { bestScore = score; bestIndex = i; }
            }
            return bestIndex;
        }
    }
}
