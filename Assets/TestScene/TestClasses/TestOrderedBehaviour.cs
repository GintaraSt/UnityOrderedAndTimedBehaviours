using OrderedBehaviors;
using UnityEngine;

public class TestOrderedBehaviour : OrderedBehaviour
{
    private float _lastRunTime = 0f;

    [TimedCallback(2f, 0f)]
    private void TimedCallbackTest()
    {
        _lastRunTime = Time.unscaledTime;
        Debug.Log($"name: {gameObject.name}, lastRunTime: {_lastRunTime}");
    }

    private void Awake()
    {
        Debug.Log($"{name} Awake");
    }

    protected override void Enable()
    {
        Debug.Log($"{name} Enable");
    }

    private void Start()
    {
        Debug.Log($"{name} Start");
    }

    [OrderedGroup(CallbackGroup.FirstStart, 0)]
    private void Initialize()
    {
        Debug.Log($"{name} FirstStart");
    }

    [OrderedGroup(CallbackGroup.SecondStart, 1)]
    private void Prepare()
    {
        Debug.Log($"{name} SecondStart");
    }

    [OrderedGroup(CallbackGroup.ThirdStart, 0)]
    private void Startup()
    {
        Debug.Log($"{name} ThirdStart");
    }
}
