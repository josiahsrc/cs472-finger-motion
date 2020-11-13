using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodyVectorFloat : BodyVector<float>
{
    public BodyVectorFloat() { }
}

[System.Serializable]
public class BodyVectorTransform : BodyVector<Transform>
{
    public BodyVectorTransform() { }
}

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

    public T this[BodyPart limb]
    {
        get => getValue(limb);
        set => setValue(limb, value);
    }

    public T this[int index]
    {
        get => getValue(index);
        set => setValue(index, value);
    }

    public T getValue(BodyPart limb)
    {
        switch (limb)
        {
            case BodyPart.upperLegL:
                return upperLegL;
            case BodyPart.upperLegR:
                return upperLegR;
            case BodyPart.lowerLegL:
                return lowerLegL;
            case BodyPart.lowerLegR:
                return lowerLegR;
            default:
                throw new System.NotImplementedException(limb.ToString());
        }
    }

    public T getValue(int index)
    {
        return getValue((BodyPart)index);
    }

    public void setValue(BodyPart limb, T value)
    {
        switch (limb)
        {
            case BodyPart.upperLegL:
                upperLegL = value;
                break;
            case BodyPart.upperLegR:
                upperLegR = value;
                break;
            case BodyPart.lowerLegL:
                lowerLegL = value;
                break;
            case BodyPart.lowerLegR:
                lowerLegR = value;
                break;
            default:
                throw new System.NotImplementedException(limb.ToString());
        }
    }

    public void setValue(int index, T value)
    {
        setValue((BodyPart)index, value);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public List<T> asList()
    {
        var result = new List<T>(length);

        foreach (var value in values)
        {
            result.Add(value);
        }

        return result;
    }

    public T[] asArray()
    {
        var result = new T[length];

        for (int i = 0; i < length; ++i)
        {
            result[i] = this[i];   
        }

        return result;
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
