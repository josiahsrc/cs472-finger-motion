using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BodyVector
{
    public const int ValueCount = 4;

    public float upperLegL;
    public float upperLegR;
    public float lowerLegL;
    public float lowerLegR;

    public int valueCount
    {
        get => ValueCount;
    }

    public IEnumerable<float> values
    {
        get
        {
            yield return upperLegL;
            yield return upperLegR;
            yield return lowerLegL;
            yield return lowerLegR;
        }
    }

    public float getValue(BodyLimb limb)
    {
        switch (limb)
        {
            case BodyLimb.upperLegL:
                return upperLegL;
            case BodyLimb.upperLegR:
                return upperLegR;
            case BodyLimb.lowerLegL:
                return lowerLegL;
            case BodyLimb.lowerLegR:
                return lowerLegR;
            default:
                throw new System.NotImplementedException(limb.ToString());
        }
    }

    public float getValue(int index)
    {
        return getValue((BodyLimb)index);
    }

    public void setValue(BodyLimb limb, float value)
    {
        switch (limb)
        {
            case BodyLimb.upperLegL:
                upperLegL = value;
                break;
            case BodyLimb.upperLegR:
                upperLegR = value;
                break;
            case BodyLimb.lowerLegL:
                lowerLegL = value;
                break;
            case BodyLimb.lowerLegR:
                lowerLegR = value;
                break;
            default:
                throw new System.NotImplementedException(limb.ToString());
        }
    }

    public void setValue(int index, float value)
    {
        setValue((BodyLimb)index, value);
    }
}
