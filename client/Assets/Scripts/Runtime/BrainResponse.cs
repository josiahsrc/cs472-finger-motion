using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrainResponse
{
    [Serializable]
    public abstract class Base
    {
        [SerializeField] private string _type;

        public string type => _type;

        public Base(string type)
        {
            this._type = type;
        }
    }

    [Serializable]
    public class SaveModel : Base
    {
        public SaveModel() : base("save_model") { }
    }

    [Serializable]
    public class LoadModel : Base
    {
        public LoadModel() : base("load_model") { }
    }

    [Serializable]
    public class AppendInstance : Base
    {
        public AppendInstance() : base("append_instance") { }
    }

    [Serializable]
    public class Fit : Base
    {
        public Fit() : base("fit") { }
    }

    [Serializable]
    public class Predict : Base
    {
        public Predict() : base("predict") { }
    }

    [Serializable]
    public class Score : Base
    {
        public Score() : base("score") { }
    }

    [Serializable]
    public class Log : Base
    {
        public string message;

        public Log() : base("log") { }
    }
}
