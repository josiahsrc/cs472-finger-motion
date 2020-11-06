using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    [SerializeField] private Brain _brain = null;
    [SerializeField] private Animator _animator = null;
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
