using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    [SerializeField] private BodyVectorTransform _transforms = null;

    public BodyVectorFloat takeSnapshot()
    {
        var snapshot = new BodyVectorFloat();

        for (int i = 0; i < snapshot.length; ++i)
        {
            snapshot[i] = _transforms[i].localEulerAngles.z;
        }

        return snapshot;
    }
}
