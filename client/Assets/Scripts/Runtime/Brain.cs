using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="Brain", menuName="App/Brain")]
public class Brain : ScriptableObject
{
    BrainFitResponse fit(BrainFitRequest request)
    {
        throw new System.NotImplementedException();
    }

    BrainPredictResponse predict(BrainPredictRequest request)
    {
        throw new System.NotImplementedException();
    }

    BrainScoreResponse score(BrainScoreRequest request)
    {
        throw new System.NotImplementedException();
    }
}
