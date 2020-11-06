using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Request = BrainRequest;
using Response = BrainResponse;

[CreateAssetMenu(fileName = "Brain", menuName = "App/Brain")]
public class Brain : ScriptableObject
{
    [SerializeField] private string _port = "5065";

    private bool _isRunning = false;

    public bool isRunning => _isRunning;

    private void OnDisable()
    {
        if (isRunning)
        {
            Debug.LogWarning("Brain still running!");
            stop();
        }
    }

    public void start()
    {

    }

    public void stop()
    {

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
