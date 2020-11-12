using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPSocket : IDisposable
{
    private Type _type = Type.none;
    private UdpClient _udpClient = null;

    private UDPSocket() { }

    public void send(string value)
    {
        Debug.Assert(this._type == Type.sender);

        var data = Encoding.ASCII.GetBytes(value);
        _udpClient.Send(data, data.Length);
    }

    public string read()
    {
        Debug.Assert(this._type == Type.reader);

        var sender = new IPEndPoint(IPAddress.Any, 0);
        var data = _udpClient.Receive(ref sender);
        var result = Encoding.ASCII.GetString(data);
        return result;
    }

    public static UDPSocket sender(string address, int port)
    {
        var result = new UDPSocket();

        result._type = Type.sender;

        var client = new UdpClient(address, port);
        result._udpClient = client;

        return result;
    }

    public static UDPSocket reader(int port)
    {
        var result = new UDPSocket();

        result._type = Type.reader;

        var endPoint = new IPEndPoint(IPAddress.Any, port);
        result._udpClient = new UdpClient(endPoint);

        return result;
    }

    public void Dispose()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
            _udpClient.Dispose();
            _udpClient = null;
        }
    }

    public enum Type
    {
        none,
        sender,
        reader,
    }
}
