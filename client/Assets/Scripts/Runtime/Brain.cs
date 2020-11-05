using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="Brain", menuName="App/Brain")]
public class Brain : ScriptableObject
{
    [SerializeField] private string _port = "5065"; 

    public BrainFitResponse fit(BrainFitRequest request)
    {
        throw new System.NotImplementedException();
    }

    public BrainPredictResponse predict(BrainPredictRequest request)
    {
        throw new System.NotImplementedException();
    }

    public BrainScoreResponse score(BrainScoreRequest request)
    {
        throw new System.NotImplementedException();
    }
}
