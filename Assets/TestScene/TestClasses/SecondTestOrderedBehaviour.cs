using OrderedBehaviors;
using UnityEngine;

public class SecondTestOrderedBehaviour : OrderedBehaviour
{
    private float _lastRunTime = 0f;

    [TimedCallback(2f, 1f)]
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

    [OrderedGroup(CallbackGroup.FirstStart, 1)]
    private void Initialize()
    {
        Debug.Log($"{name} Ordered FirstStart");
    }

    [OrderedGroup(CallbackGroup.SecondStart, 0)]
    private void Prepare()
    {
        Debug.Log($"{name} Ordered SecondStart");
    }

    [OrderedGroup(CallbackGroup.ThirdStart, 1)]
    private void Startup()
    {
        Debug.Log($"{name} Ordered ThirdStart");
    }
}
