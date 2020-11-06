using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPSocket
{
    private const int BufferSize = 8 * 1024;

    private Logger _logger = new Logger(false, "UDPSocket");
    private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private State _state = new State();
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback _recv = null;

    public bool debug
    {
        get => _logger.debug;
        set => _logger.debug = value;
    }

    public void server(string address, int port)
    {
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        receive();
    }

    public void client(string address, int port)
    {
        _socket.Connect(IPAddress.Parse(address), port);
        receive();
    }

    public void send(string text)
    {
        byte[] data = Encoding.ASCII.GetBytes(text);
        _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
        {
            State so = (State)ar.AsyncState;
            int bytes = _socket.EndSend(ar);
            _logger.info($"SEND: {bytes}, {text}");
        }, _state);
    }

    private void receive()
    {
        _socket.BeginReceiveFrom(_state.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, _recv = (ar) =>
        {
            State so = (State)ar.AsyncState;
            int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
            _socket.BeginReceiveFrom(so.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, _recv, so);
            _logger.info($"RECV: {epFrom.ToString()}: {bytes}, {Encoding.ASCII.GetString(so.buffer, 0, bytes)}");
        }, _state);
    }

    private class State
    {
        public byte[] buffer = new byte[BufferSize];
    }
}
