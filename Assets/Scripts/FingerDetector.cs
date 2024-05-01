using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerDetector : MonoBehaviour
{
    public delegate void OnFinger(GameObject gameObject);

    public event OnFinger OnFingerEnter;
    public event OnFinger OnFingerExit;

    private GameObject _gameObject = null;

    void OnTriggerEnter(Collider other)
    {
        var go = other.gameObject;
        if (go.tag == "Finger" && go != _gameObject)
        {
            _gameObject = go;
            OnFingerEnter?.Invoke(go);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var go = other.gameObject;
        if (go.tag == "Finger" && go != _gameObject)
        {
            _gameObject = go;
            OnFingerEnter?.Invoke(go);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var go = other.gameObject;
        if (go.tag == "Finger")
        {
            _gameObject = null;
            OnFingerExit?.Invoke(go);
        }
    }
}
