using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodyVector<T> : IEnumerable<T>
{
    public T upperLegL = default(T);
    public T upperLegR = default(T);
    public T lowerLegL = default(T);
    public T lowerLegR = default(T);

    public int length => 4;

    public IEnumerable<T> values
    {
        get
        {
            yield return upperLegL;
            yield return upperLegR;
            yield return lowerLegL;
            yield return lowerLegR;
        }
    }

    public T this[BodyLimb limb]
    {
        get => getValue(limb);
        set => setValue(limb, value);
    }

    public T this[int index]
    {
        get => getValue(index);
        set => setValue(index, value);
    }

    public T getValue(BodyLimb limb)
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

    public T getValue(int index)
    {
        return getValue((BodyLimb)index);
    }

    public void setValue(BodyLimb limb, T value)
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

    public void setValue(int index, T value)
    {
        setValue((BodyLimb)index, value);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public BodyVector<T> copy()
    {
        var result = new BodyVector<T>();

        result.upperLegL = upperLegL;
        result.upperLegR = upperLegR;
        result.lowerLegL = lowerLegL;
        result.lowerLegR = lowerLegR;

        return result;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (obj.GetType() != typeof(BodyVector<T>))
            return false;
        if (ReferenceEquals(this, obj))
            return true;

        var other = (BodyVector<T>)obj;
        return
            this.upperLegL.Equals(other.upperLegL) &&
            this.upperLegR.Equals(other.upperLegR) &&
            this.lowerLegL.Equals(other.lowerLegL) &&
            this.lowerLegR.Equals(other.lowerLegR);
    }

    public override int GetHashCode()
    {
        return
            upperLegL.GetHashCode() ^
            upperLegR.GetHashCode() ^
            lowerLegL.GetHashCode() ^
            lowerLegR.GetHashCode();
    }

    public override string ToString()
    {
        return string.Format(
            "ULL: {0}, ULR: {1}, LLL: {2}, LLR: {3}",
            upperLegL, upperLegR, lowerLegL, lowerLegR
        );
    }
}
