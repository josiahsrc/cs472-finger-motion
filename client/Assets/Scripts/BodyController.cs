using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    [Header("Transforms")]
    [SerializeField] private Transform _upperLegL = null;
    [SerializeField] private Transform _upperLegR = null;
    [Space]
    [SerializeField] private Transform _lowerLegL = null;
    [SerializeField] private Transform _lowerLegR = null;


    public BodyVector snapshot()
    {
        var snapshot = new BodyVector();

        for (int i = 0; i < snapshot.valueCount; ++i)
        {
            snapshot[i] = _activeTransforms[i].localEulerAngles.z;
        }

        return snapshot;
    }

}
