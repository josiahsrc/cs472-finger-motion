using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPSender : IDisposable
{
    private Socket _socket;
    private string _address;
    private int _port;
    private byte[] _buffer;

    public UDPSender(string address, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _address = address;
        _port = port;
        _buffer = new byte[UDPUtility.BufferSize];
    }

    public UDPSender start()
    {
        _socket.Connect(IPAddress.Parse(_address), _port);
        return this;
    }

    public void send(string value)
    {
        var length = Encoding.ASCII.GetBytes(value, 0, value.Length, _buffer, 0);
        _socket.Send(_buffer, 0, length, SocketFlags.None);
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