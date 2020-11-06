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
    [SerializeField] private string _port = "5065";

    private Thread _serverRequestThread = null;
    private Thread _serverResponseThread = null;
    private HashSet<IObserver> _observers = new HashSet<IObserver>();

    public bool isRunning => _serverRequestThread != null || _serverResponseThread != null;

    private void OnDisable()
    {
        if (isRunning)
        {
            Debug.LogWarning("Brain still running!");
            stop();
        }
    }

    private void brainServerResponseLoop()
    {
        while (true)
        {
            Thread.Sleep(1000);
        }
    }

    private void brainServerRequestLoop()
    {
        while (true)
        {
            Thread.Sleep(1000);
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
            if (_serverResponseThread == null)
            {
                _serverResponseThread = new Thread(new ThreadStart(brainServerResponseLoop));
                _serverResponseThread.IsBackground = true;
                _serverResponseThread.Start();
            }

            if (_serverRequestThread == null)
            {
                _serverRequestThread = new Thread(new ThreadStart(brainServerRequestLoop));
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
