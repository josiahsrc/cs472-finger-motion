using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

using Request = BrainRequest;
using Response = BrainResponse;

[CreateAssetMenu(fileName = "Brain", menuName = "App/Brain")]
public class Brain : ScriptableObject
{
    [SerializeField] private bool _debug = true;

    [Header("Network Settings")]
    [SerializeField] private int _threadDelay = 10;
    [Space]
    [SerializeField] private string _sendAddress = "127.0.0.1";
    [SerializeField] private int _sendPort = 8080;
    [Space]
    [SerializeField] private string _recvAddress = "127.0.0.1";
    [SerializeField] private int _recvPort = 5065;

    private HashSet<IObserver> _observers = new HashSet<IObserver>();
    private ICoroutineDriver _driver = null;
    private Thread _sendThread = null;
    private Thread _recvThread = null;
    private UDPSender _sendSocket = null;
    private UDPReceiver _recvSocket = null;
    private Logger _logger = new Logger("Brain");
    private ConcurrentQueue<string> _rawRequests = null;
    private ConcurrentQueue<string> _rawResponses = null;

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

    public void notifyObservers(Action<IObserver> callback)
    {
        foreach (var observer in _observers)
        {
            callback(observer);
        }
    }

    public void setCoroutineDriver(ICoroutineDriver driver)
    {
        Debug.Assert(!isRunning);
        _driver = driver;
    }

    public string statusReport()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Brain Status Report [{DateTime.Now.ToShortTimeString()}]");
        builder.AppendLine($"Is running: {isRunning}");
        builder.AppendLine($"Request Queue Size: {_rawRequests.Count}");
        builder.AppendLine($"Response Queue Size: {_rawResponses.Count}");

        return builder.ToString();
    }

    public void start()
    {
        Debug.Assert(!isRunning);
        Debug.Assert(_driver != null);

        try
        {
            _rawRequests = new ConcurrentQueue<string>();
            _rawResponses = new ConcurrentQueue<string>();

            _sendSocket = new UDPSender(_sendAddress, _sendPort);
            _sendThread = new Thread(new ThreadStart(threadProcess_send));
            _sendThread.IsBackground = true;
            _sendThread.Start();
            _logger.info("Started UDP send thread");

            _recvSocket = new UDPReceiver(_recvAddress, _recvPort);
            _recvThread = new Thread(new ThreadStart(threadProcess_recv));
            _recvThread.IsBackground = true;
            _recvThread.Start();
            _logger.info("Started UDP recv thread");

            _driver.startSpinRoutine(spinRoutine());

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
        Debug.Assert(_driver != null);

        try
        {
            _driver.stopSpinRoutine();

            _sendThread.Abort();
            _sendThread = null;
            _sendSocket.Dispose();
            _sendSocket = null;
            _logger.info("Stopped UDP send thread");

            _recvThread.Abort();
            _recvThread = null;
            _recvSocket.Dispose();
            _recvSocket = null;
            _logger.info("Stopped UDP recv thread");

            _rawRequests = null;
            _rawResponses = null;

            notifyObservers((o) => o.onStop());
        }
        catch (Exception e)
        {
            _logger.error(e.ToString());
            throw e;
        }
    }

    public void saveModel(Request.SaveModel request)
    {
        _rawRequests.Enqueue(JsonUtility.ToJson(request));
    }

    public void loadModel(Request.LoadModel request)
    {
        _rawRequests.Enqueue(JsonUtility.ToJson(request));
    }

    public void appendInstance(Request.AppendInstance request)
    {
        _rawRequests.Enqueue(JsonUtility.ToJson(request));
    }

    public void fit(Request.Fit request)
    {
        _rawRequests.Enqueue(JsonUtility.ToJson(request));
    }

    public void predict(Request.Predict request)
    {
        _rawRequests.Enqueue(JsonUtility.ToJson(request));
    }

    public void score(Request.Score request)
    {
        _rawRequests.Enqueue(JsonUtility.ToJson(request));
    }

    private IEnumerator spinRoutine()
    {
        while (true)
        {
            yield return null;

            if (_rawResponses.IsEmpty)
            {
                continue;
            }

            string json;
            if (!_rawResponses.TryDequeue(out json))
            {
                continue;
            }

            try
            {
                var baseResponse = JsonUtility.FromJson<Response.Base>(json);
                if (baseResponse.typeID == Response.TypeIDs.saveModel)
                {
                    notifyObservers((o) => o.onSaveModel(JsonUtility.FromJson<Response.SaveModel>(json)));
                }
                else if (baseResponse.typeID == Response.TypeIDs.loadModel)
                {
                    notifyObservers((o) => o.onLoadModel(JsonUtility.FromJson<Response.LoadModel>(json)));
                }
                else if (baseResponse.typeID == Response.TypeIDs.appendInstance)
                {
                    notifyObservers((o) => o.onAppendInstance(JsonUtility.FromJson<Response.AppendInstance>(json)));
                }
                else if (baseResponse.typeID == Response.TypeIDs.fit)
                {
                    notifyObservers((o) => o.onFit(JsonUtility.FromJson<Response.Fit>(json)));
                }
                else if (baseResponse.typeID == Response.TypeIDs.predict)
                {
                    notifyObservers((o) => o.onPredict(JsonUtility.FromJson<Response.Predict>(json)));
                }
                else if (baseResponse.typeID == Response.TypeIDs.score)
                {
                    notifyObservers((o) => o.onScore(JsonUtility.FromJson<Response.Score>(json)));
                }
                else if (baseResponse.typeID == Response.TypeIDs.log)
                {
                    notifyObservers((o) => o.onLog(JsonUtility.FromJson<Response.Log>(json)));
                }
                else
                {
                    throw new NotImplementedException(baseResponse.typeID.ToString());
                }
            }
            catch (Exception e)
            {
                _logger.error(e);
                continue;
            }
        }
    }

    private void threadProcess_send()
    {
        _sendSocket.start();

        while (true)
        {
            Thread.Sleep(_threadDelay);

            if (_rawRequests.IsEmpty)
            {
                continue;
            }

            string rawRequest;
            if (!_rawRequests.TryDequeue(out rawRequest))
            {
                continue;
            }

            try
            {
                _sendSocket.send(rawRequest);
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
                builder.Append($"running on address={_sendAddress}, port={_sendPort}\n");
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
            Thread.Sleep(_threadDelay);

            string rawResponse;
            try
            {
                rawResponse = _recvSocket.read();
            }
            catch (ThreadAbortException e)
            {
                _logger.info(e.ToString());
                continue;
            }
            catch (Exception e)
            {
                var builder = new StringBuilder();
                builder.Append($"Unable to read client receiver.\n");
                builder.Append(e.ToString());
                _logger.error(builder);
                continue;
            }

            if (rawResponse == null || rawResponse == "")
            {
                continue;
            }

            _rawResponses.Enqueue(rawResponse);
        }
    }

    public interface ICoroutineDriver
    {
        void startSpinRoutine(IEnumerator routine);

        void stopSpinRoutine();
    }

    public interface IObserver
    {
        void onStart();

        void onStop();

        void onSaveModel(Response.SaveModel response);

        void onLoadModel(Response.LoadModel response);

        void onAppendInstance(Response.AppendInstance response);

        void onFit(Response.Fit response);

        void onPredict(Response.Predict response);

        void onScore(Response.Score response);

        void onLog(Response.Log response);
    }
}
