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

        void onLoadModel(Response::SaveModel response);

        void onAppendInstance(Response::SaveModel response);

        void onFit(Response::SaveModel response);

        void onPredict(Response::SaveModel response);

        void onScore(Response::SaveModel response);

        void onLog(Response::Log response);
    }
}
