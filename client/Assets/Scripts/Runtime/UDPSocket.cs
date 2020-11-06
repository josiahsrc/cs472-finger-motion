using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPSocket
{
    private const int BufferSize = 8 * 1024;

    public bool debug = false;

    private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private State state = new State();
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback recv = null;

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
            log($"SEND: {bytes}, {text}");
        }, state);
    }

    private void receive()
    {
        _socket.BeginReceiveFrom(state.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, recv = (ar) =>
        {
            State so = (State)ar.AsyncState;
            int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
            _socket.BeginReceiveFrom(so.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, recv, so);
            log($"RECV: {epFrom.ToString()}: {bytes}, {Encoding.ASCII.GetString(so.buffer, 0, bytes)}");
        }, state);
    }

    private void log(object msg)
    {
        if (debug)
        {
            Debug.Log(msg);
        }
    }

    private class State
    {
        public byte[] buffer = new byte[BufferSize];
    }
}
