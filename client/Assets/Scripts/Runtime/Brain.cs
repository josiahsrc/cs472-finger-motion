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

    private UDPThreadRunner _serverSendRunner = null;
    private UDPThreadRunner _serverReadRunner = null;
    private HashSet<IObserver> _observers = new HashSet<IObserver>();
    private Logger _logger = new Logger("Brain");

    public bool isRunning => _serverSendRunner != null || _serverReadRunner != null;

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

    private UDPThreadRunner buildSendRunner()
    {
        var result = new UDPThreadRunner();

        result.delay = ThreadDelay;
        result.logger = new Logger("UDP Sender", _debug);

        result.onStart = () =>
        {
            result.sock = UDPSocket.sender(_brainAddress, _brainPort);
        };

        result.onUpdate = () =>
        {
            // TODO: Send actual json
            result.sock.send("Hello world\n");
        };

        result.onStop = () =>
        {
            result.sock.Dispose();
        };

        return result;
    }

    private UDPThreadRunner buildReadRunner()
    {
        var result = new UDPThreadRunner();

        result.delay = ThreadDelay;
        result.logger = new Logger("UDP Reader", _debug);

        result.onStart = () =>
        {
            result.sock = UDPSocket.reader(_clientPort);
        };

        result.onUpdate = () =>
        {
            // TODO: Do something with response
            var response = result.sock.read();
            result.logger.info($"Received: {response}");
        };

        result.onStop = () =>
        {
            result.sock.Dispose();
        };

        return result;
    }

    public string statusReport()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Brain Status Report [{DateTime.Now.ToShortTimeString()}]");
        builder.AppendLine($"Is running: {isRunning}");

        return builder.ToString();
    }

    public void start()
    {
        Debug.Assert(!isRunning);

        try
        {
            if (_serverSendRunner == null)
            {
                _serverSendRunner = buildSendRunner();
                _serverSendRunner.start();
            }

            if (_serverReadRunner == null)
            {
                _serverReadRunner = buildReadRunner();
                _serverReadRunner.start();
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
            if (_serverSendRunner != null)
            {
                _serverSendRunner.stop();
                _serverSendRunner = null;
            }

            if (_serverReadRunner != null)
            {
                _serverReadRunner.stop();
                _serverReadRunner = null;
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

    private class UDPThreadRunner
    {
        public Logger logger = null;
        public int delay = 0;
        public UDPSocket sock = null;

        public Action onStart = null;
        public Action onUpdate = null;
        public Action onStop = null;

        private Thread _thread = null;

        public bool isRunning { get; private set; } = false;

        public void start()
        {
            Debug.Assert(!isRunning);

            isRunning = true;
            onStart?.Invoke();

            _thread = new Thread(new ThreadStart(runLoop));
            _thread.IsBackground = true;
            _thread.Start();

            logger?.info("Started thread");
        }

        public void stop()
        {
            Debug.Assert(isRunning);

            _thread.Abort();
            _thread = null;

            isRunning = false;
            onStop?.Invoke();

            logger?.info("Stopped thread");
        }

        private void runLoop()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(delay);
                    onUpdate?.Invoke();
                }
                catch (ThreadAbortException e)
                {
                    logger?.info(e.ToString());
                }
                catch (Exception e)
                {
                    logger?.error(e.ToString());
                }
            }

            throw new NotSupportedException();
        }
    }
}
