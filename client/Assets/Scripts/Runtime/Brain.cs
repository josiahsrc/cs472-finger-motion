using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Request = BrainRequest;
using Response = BrainResponse;

[CreateAssetMenu(fileName = "Brain", menuName = "App/Brain")]
public class Brain : ScriptableObject
{
    private const int ThreadDelay = 3000;

    [SerializeField] private bool _debug = true;

    [Header("Network Settings")]
    [SerializeField] private string _brainAddress = "localhost";
    [SerializeField] private int _brainPort = 8080;
    [SerializeField] private int _clientPort = 5065;

    private UDPSocket _sockSend = null;
    private UDPSocket _sockRecv = null;
    private Thread _serverRequestThread = null;
    private Thread _serverResponseThread = null;
    private HashSet<IObserver> _observers = new HashSet<IObserver>();
    private Logger _logger = new Logger("Brain");

    public bool isRunning => _serverRequestThread != null || _serverResponseThread != null;

    private void OnValidate()
    {
        _logger.debug = _debug;
    }

    private void OnDisable()
    {
        if (isRunning)
        {
            Debug.LogWarning("Brain still running!");
            stop();
        }
    }

    private void brainServerReadLoop()
    {
        while (true)
        {
            Thread.Sleep(ThreadDelay);
        }
    }

    private void brainServerSendLoop()
    {
        while (true)
        {
            Thread.Sleep(ThreadDelay);

            try
            {
                _sockSend.send("Hello world");
            }
            catch (Exception e)
            {
                _logger.error(e.ToString());
            }
        }
    }

    public void addObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void removeObserver(IObserver observer)
    {
        _observers.Remove(observer);
    }

    private void notifyObservers(Action<IObserver> callback)
    {
        foreach (var observer in _observers)
        {
            callback(observer);
        }
    }

    public void start()
    {
        try
        {
            if (_sockSend == null)
            {
                _sockSend = UDPSocket.sender(_brainAddress, _brainPort);
            }

            if (_sockRecv == null)
            {
                //_sockSend = UDPSocket.reader(_clientPort);
            }

            if (_serverResponseThread == null)
            {
                _serverResponseThread = new Thread(new ThreadStart(brainServerReadLoop));
                _serverResponseThread.IsBackground = true;
                _serverResponseThread.Start();
            }

            if (_serverRequestThread == null)
            {
                _serverRequestThread = new Thread(new ThreadStart(brainServerSendLoop));
                _serverRequestThread.IsBackground = true;
                _serverRequestThread.Start();
            }
        }
        catch (Exception e)
        {
            stop();
            throw e;
        }

        notifyObservers((o) => o.onStart());
    }

    public void stop()
    {
        if (_serverResponseThread != null)
        {
            _serverResponseThread.Abort();
            _serverResponseThread = null;
        }

        if (_serverRequestThread != null)
        {
            _serverRequestThread.Abort();
            _serverRequestThread = null;
        }

        if (_sockSend != null)
        {
            _sockSend.Dispose();
            _sockSend = null;
        }

        if (_sockRecv != null)
        {
            _sockRecv.Dispose();
            _sockRecv = null;
        }

        notifyObservers((o) => o.onStop());
    }

    public void saveModel(Request::SaveModel request)
    {

    }

    public void loadModel(Request::LoadModel request)
    {

    }

    public void appendInstance(Request::AppendInstance request)
    {

    }

    public void fit(Request::Fit request)
    {

    }

    public void predict(Request::Predict request)
    {

    }

    public void score(Request::Score request)
    {

    }

    public interface IObserver
    {
        void onStart();

        void onStop();

        void onSaveModel(Response::SaveModel response);

        void onLoadModel(Response::LoadModel response);

        void onAppendInstance(Response::AppendInstance response);

        void onFit(Response::Fit response);

        void onPredict(Response::Predict response);

        void onScore(Response::Score response);

        void onLog(Response::Log response);
    }
}
