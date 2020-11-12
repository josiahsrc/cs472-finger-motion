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
    [SerializeField] private string _brainAddress = "127.0.0.1";
    [SerializeField] private int _brainPort = 8080;
    [Space]
    [SerializeField] private string _clientAddress = "127.0.0.1";
    [SerializeField] private int _clientPort = 5065;

    private Thread _sendThread = null;
    private Thread _recvThread = null;
    private UDPSender _sendSocket = null;
    private UDPReceiver _recvSocket = null;
    private HashSet<IObserver> _observers = new HashSet<IObserver>();
    private Logger _logger = new Logger("Brain");

    public bool isRunning => _sendThread != null || _recvThread != null;

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

    public string statusReport()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Brain Status Report [{DateTime.Now.ToShortTimeString()}]");
        builder.AppendLine($"Is running: {isRunning}");

        return builder.ToString();
    }

    private void threadProcess_send()
    {
        _sendSocket.start();

        while (true)
        {
            Thread.Sleep(ThreadDelay);

            try
            {
                _sendSocket.send("Hello world!\n");
            }
            catch (ThreadAbortException e)
            {
                _logger.info(e.ToString());
            }
            catch (Exception e)
            {
                var builder = new StringBuilder();
                builder.Append($"Unable to connect to the brain.\n");
                builder.Append($"This probably means that there is no UDP server (brain) ");
                builder.Append($"running on address={_brainAddress}, port={_brainPort}\n");
                builder.Append(e.ToString());
                _logger.error(builder);
            }
        }
    }

    private void threadProcess_recv()
    {
        _recvSocket.start();

        while (true)
        {
            Thread.Sleep(ThreadDelay);

            try
            {
                var response = _recvSocket.read();
                _logger.info($"RECEIVED RESPONSE: {response}");
            }
            catch (ThreadAbortException e)
            {
                _logger.info(e.ToString());
            }
            catch (Exception e)
            {
                var builder = new StringBuilder();
                builder.Append($"Unable to read client receiver.\n");
                builder.Append(e.ToString());
                _logger.error(builder);
            }
        }
    }

    public void start()
    {
        Debug.Assert(!isRunning);

        try
        {
            if (_sendThread == null)
            {
                _sendSocket = new UDPSender(_brainAddress, _brainPort);

                _sendThread = new Thread(new ThreadStart(threadProcess_send));
                _sendThread.IsBackground = true;
                _sendThread.Start();

                _logger.info("Started UDP send thread");
            }

            if (_recvThread == null)
            {
                _recvSocket = new UDPReceiver(_clientAddress, _clientPort);

                _recvThread = new Thread(new ThreadStart(threadProcess_recv));
                _recvThread.IsBackground = true;
                _recvThread.Start();

                _logger.info("Started UDP recv thread");
            }

            notifyObservers((o) => o.onStart());
        }
        catch (Exception e)
        {
            _logger.error(e.ToString());
            stop();
            throw e;
        }
    }

    public void stop()
    {
        Debug.Assert(isRunning);

        try
        {
            if (_sendThread != null)
            {
                _sendThread.Abort();
                _sendThread = null;

                _sendSocket.Dispose();
                _sendSocket = null;

                _logger.info("Stopped UDP send thread");
            }

            if (_recvThread != null)
            {
                _recvThread.Abort();
                _recvThread = null;

                _recvSocket.Dispose();
                _recvSocket = null;

                _logger.info("Stopped UDP recv thread");
            }

            notifyObservers((o) => o.onStop());
        }
        catch (Exception e)
        {
            _logger.error(e.ToString());
            throw e;
        }
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
