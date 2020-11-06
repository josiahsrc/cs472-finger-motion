using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrainResponse
{

    [Serializable]
    public abstract class Base
    {

    }

    [Serializable]
    public class SaveModel : Base
    {

    }

    [Serializable]
    public class LoadModel : Base
    {

    }

    [Serializable]
    public class AppendInstance : Base
    {

    }

    [Serializable]
    public class Fit : Base
    {

    }


    [Serializable]
    public class Predict : Base
    {

    }


    [Serializable]
    public class Score : Base
    {

    }

    [Serializable]
    public class Log : Base
    {
        public string message;
    }
}
