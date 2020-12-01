using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrainResponse
{
    public static class TypeIDs
    {
        public const string saveModel = "save_model";
        public const string loadModel = "load_model";
        public const string appendInstance = "append_instance";
        public const string fit = "fit";
        public const string predict = "predict";
        public const string score = "score";
        public const string log = "log";
    }

    [Serializable]
    public class Base
    {
        [SerializeField] private string type;

        public string typeID => type;

        public Base(string type)
        {
            this.type = type;
        }
    }

    [Serializable]
    public class SaveModel : Base
    {
        public SaveModel() : base(TypeIDs.saveModel) { }
    }

    [Serializable]
    public class LoadModel : Base
    {
        public LoadModel() : base(TypeIDs.loadModel) { }
    }

    [Serializable]
    public class AppendInstance : Base
    {
        public string message;

        public AppendInstance() : base(TypeIDs.appendInstance) { }
    }

    [Serializable]
    public class Fit : Base
    {
        public string message;

        public Fit() : base(TypeIDs.fit) { }
    }

    [Serializable]
    public class Predict : Base
    {
        public Predict() : base(TypeIDs.predict) { }
    }

    [Serializable]
    public class Score : Base
    {
        public Score() : base(TypeIDs.score) { }
    }

    [Serializable]
    public class Log : Base
    {
        public string message;

        public Log() : base(TypeIDs.log) { }
    }
}
