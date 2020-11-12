using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPReceiver : IDisposable
{
    private Socket _socket;
    private EndPoint _epFrom;
    private string _address;
    private int _port;
    private byte[] _buffer;

    public UDPReceiver(string address, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _epFrom = new IPEndPoint(IPAddress.Any, 0);
        _address = address;
        _port = port;
        _buffer = new byte[UDPUtility.BufferSize];
    }

    public UDPReceiver start()
    {
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        _socket.Bind(new IPEndPoint(IPAddress.Parse(_address), _port));
        return this;
    }

    public string read()
    {
        int length = _socket.ReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref _epFrom);
        var result = Encoding.ASCII.GetString(_buffer, 0, length);
        return result;
    }
    
    public void Dispose()
    {
        if (_socket.Connected)
        {
            _socket.Disconnect(true);
        }

        if (_socket != null)
        {
            _socket.Dispose();
        }

        _socket = null;
    }
}