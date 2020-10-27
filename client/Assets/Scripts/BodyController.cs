using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    [SerializeField] private BodyVectorTransform _transforms = null;

    public BodyVectorFloat snapshot()
    {
        var snapshot = new BodyVectorFloat();

        for (int i = 0; i < snapshot.length; ++i)
        {
            snapshot[i] = _transforms[i].localEulerAngles.z;
        }

        return snapshot;
    }

    private void Start()
    {
        StartCoroutine(temporaryTransformLimb(_transforms.upperLegR, -45, 45));
        StartCoroutine(temporaryTransformLimb(_transforms.lowerLegR, -45, 45));

        StartCoroutine(temporaryTransformLimb(_transforms.upperLegL, 45, -45));
        StartCoroutine(temporaryTransformLimb(_transforms.lowerLegL, 45, -45));
    }

    private IEnumerator temporaryTransformLimb(Transform limb, float startRot, float endRot)
    {
        var t = 0.0f;
        while (t < 1)
        {
            t += Time.deltaTime / 2.5f;
            var rot = Mathf.Lerp(startRot, endRot, t);

            limb.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rot));

            yield return null;
        }

        t = 0.0f;
        while (t < 1)
        {
            t += Time.deltaTime / 2.5f;
            var rot = Mathf.Lerp(endRot, startRot, t);

            limb.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rot));

            yield return null;
        }

        StartCoroutine(temporaryTransformLimb(limb, startRot, endRot));
    }
}
