using System.Collections;
using System.Collections.Generic;
using BrainResponse;
using UnityEngine;

public class BodyController : MonoBehaviour, Brain.ICoroutineDriver, Brain.IObserver
{
    [SerializeField] private Brain _brain = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private BodyVectorTransform _transforms = null;
    [Space]
    [SerializeField] private float _predictionDelay = 0.1f;
    [SerializeField] private float _smoothing = 30f;
    [SerializeField] private bool _debug = true;

    private BodyVectorFloat _targetSnapshot = null;
    private Coroutine _predictionRoutine = null;
    private Coroutine _brainSprinRoutine = null;
    private Logger _logger = new Logger("BodyController");

    public BodyVectorFloat takeSnapshot()
    {
        var snapshot = new BodyVectorFloat();

        for (int i = 0; i < snapshot.length; ++i)
        {
            snapshot[i] = _transforms[i].localEulerAngles.z;
        }

        return snapshot;
    }

    private void OnValidate()
    {
        _logger.debug = _debug;
    }

    private void OnEnable()
    {
        _brain.setCoroutineDriver(this);
        _brain.addObserver(this);
        _brain.start();
        _predictionRoutine = StartCoroutine(predictionRequestLoop());
    }

    private void OnDisable()
    {
        StopCoroutine(_predictionRoutine);
        _predictionRoutine = null;
        _brain.stop();
        _brain.setCoroutineDriver(null);
        _brain.removeObserver(null);
    }

    private void Start()
    {
        _animator.enabled = false;
    }

    private void Update()
    {
        if (_targetSnapshot == null)
        {
            return;
        }

        for (int i = 0; i < _transforms.length; ++i)
        {
            var a = _transforms[i].localRotation;
            var b = Quaternion.Euler(0, 0, _targetSnapshot[i]);
            _transforms[i].localRotation = Quaternion.Lerp(a, b, Time.deltaTime * _smoothing);
        }
    }

    private IEnumerator predictionRequestLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_predictionDelay);
            var request = new BrainRequest.Predict();
            _brain.predict(request);
            _logger.info($"Requested prediction from brain");
        }
    }

    void Brain.ICoroutineDriver.startSpinRoutine(IEnumerator routine)
    {
        _brainSprinRoutine = StartCoroutine(routine);
    }

    void Brain.ICoroutineDriver.stopSpinRoutine()
    {
        StopCoroutine(_brainSprinRoutine);
        _brainSprinRoutine = null;
    }

    void Brain.IObserver.onStart()
    {

    }

    void Brain.IObserver.onStop()
    {

    }

    void Brain.IObserver.onSaveModel(SaveModel response)
    {

    }

    void Brain.IObserver.onLoadModel(LoadModel response)
    {

    }

    void Brain.IObserver.onAppendInstance(AppendInstance response)
    {

    }

    void Brain.IObserver.onFit(Fit response)
    {

    }

    void Brain.IObserver.onPredict(Predict response)
    {
        if (response?.prediction == null)
        {
            _logger.warning($"Received null prediction from server...");
            return;
        }

        var snap = new BodyVectorFloat();
        for (int i = 0; i < snap.length; ++i)
        {
            snap[i] = response.prediction[i];
        }

        _logger.info($"Received prediction from server: {_targetSnapshot}");
        _targetSnapshot = snap;
    }

    void Brain.IObserver.onScore(Score response)
    {

    }

    void Brain.IObserver.onLog(Log response)
    {
        _logger.info(response.message);
    }
}
