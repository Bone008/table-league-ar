using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>Displays the device's local IPv4 address in a text field.</summary>
public class IpAddressDisplay : MonoBehaviour
{
    public TMPro.TextMeshProUGUI display;

    void Start()
    {
        string localIP;
        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = (IPEndPoint)socket.LocalEndPoint;
                localIP = endPoint.Address.ToString();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to determine local IP using UDP socket:\n" + e.GetType() + ": " + e.Message);

            // Fall back to using local Dns lookup.
            var host = Dns.GetHostEntry(Dns.GetHostName());
            localIP = host.AddressList.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }

        if (!string.IsNullOrEmpty(localIP))
            display.text = localIP;
    }
}
