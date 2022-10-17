using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class EventTest : MonoBehaviour
{
    [SerializeField] private UnityEvent testEvent;

    private Action testAction;
    private void Start()
    {
        testEvent.Invoke();

        testAction += TestFunction;

        testAction.Invoke();
    }

    private void TestFunction()
    {

    }
}
