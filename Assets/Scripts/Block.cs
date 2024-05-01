using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rigidBody;
    [SerializeField]
    private Transform sourceTransform;
    [SerializeField]
    private List<FingerDetector> fingerDetectors;

    private Vector3 t1;
    private Vector3 t2;
    private Vector3 c;
    private Quaternion r;
    private Quaternion initialRotation;
    private Quaternion initialRotationDiff;
    private Vector3 oldNormal;
    private Vector3 f1 => g1.transform.position;
    private Vector3 f2 => g2.transform.position;

    private GameObject g1 = null;
    private GameObject g2 = null;

    void Awake()
    {
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody>();
        }
    }

    void Start()
    {
        foreach (var fd in fingerDetectors)
        {
            fd.OnFingerEnter += AddFingerObject;
            fd.OnFingerExit += RemoveFingerObject;
        }
    }

    void Update()
    {
        if (!checkSticky())
        {
            if (g1 != null && g2 != null)
            {
                rigidBody.useGravity = true;
                g1 = g2 = null;
            }
            if (g1 != null)
            {
                moveByFinger(ref t1, g1);
            }
            else if (g2 != null)
            {
                moveByFinger(ref t2, g2);
            }
            return;
        }
        rigidBody.useGravity = false;
        // sticky logic
        Vector3 tMid = (t1 + t2) * 0.5f;
        Vector3 fMid = (f1 + f2) * 0.5f;
        Vector3 translation = fMid - tMid;
        //Debug.Log($"!!! tMid={tMid} fMid={fMid} => newPos={newPos}");
        Quaternion nowRotation = Quaternion.Slerp(g1.transform.rotation, g2.transform.rotation, 0.5f);
        Quaternion rotationDiff = nowRotation * Quaternion.Inverse(initialRotation);
        Vector3 nowNormal = (nowRotation * (Vector3.forward * -1f)).normalized;
        Vector3 newPos = Quaternion.FromToRotation(oldNormal, nowNormal) * (c - tMid) + tMid + translation;
        //Debug.Log($"!!! rotationDiff={rotationDiff}");
        sourceTransform.SetPositionAndRotation(newPos, rotationDiff * r);
    }

    bool checkSticky()
    {
        if (g1 == null || g2 == null) return false;
        float far = (f2 - f1).sqrMagnitude / (t2 - t1).sqrMagnitude;
        //Debug.Log($"!!! far = {far}");
        return Mathf.Abs(far - 1) < 0.5f;
    }

    void moveByFinger(ref Vector3 initialTouch, GameObject go)
    {
        Vector3 posDiff = go.transform.position - initialTouch;
        Vector3 dir = (sourceTransform.position - initialTouch).normalized;
        float moveAmount = Vector3.Dot(posDiff, dir);
        if (moveAmount > 0)
        {
            rigidBody.MovePosition(sourceTransform.position + posDiff * moveAmount / posDiff.magnitude);
        }
        initialTouch = go.transform.position;
    }

    // void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.tag != "Jenga")
    //     {
    //         Debug.Log("Collision Enter with " + collision.gameObject.name);
    //     }
    // }

    // void OnCollisionStay(Collision collision)
    // {
    //     if (collision.gameObject.tag != "Jenga")
    //     {
    //         Debug.Log("Collision Stay with " + collision.gameObject.name);
    //     }
    // }

    void AddFingerObject(GameObject gameObject)
    {
        if (g1 == gameObject || g2 == gameObject) return;
        if (g1 != null && g2 != null) return;
        bool setFlag = false;
        if (g1 == null)
        {
            t1 = gameObject.transform.position;
            g1 = gameObject;
            setFlag = true;
        }
        else if (g2 == null)
        {
            t2 = gameObject.transform.position;
            g2 = gameObject;
            setFlag = true;
        }
        if (setFlag)
        {
            c = sourceTransform.position;
            r = sourceTransform.rotation;
            if (g1 != null && g2 != null)
            {
                initialRotation = Quaternion.Slerp(g1.transform.rotation, g2.transform.rotation, 0.5f);
                initialRotationDiff = initialRotation * Quaternion.Inverse(r);
                oldNormal = Vector3.Cross(t1 - c, t2 - c).normalized;
            }
        }
    }

    void RemoveFingerObject(GameObject gameObject)
    {
        if (g1 != null && g2 != null) return;
        if (g1 == gameObject) g1 = null;
        if (g2 == gameObject) g2 = null;
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     GameObject go = other.gameObject;
    //     if (go.tag == "Finger")
    //     {
    //         bool setFlag = false;
    //         if (g1 == null)
    //         {
    //             g1 = go;
    //             t1 = g1.transform.position;
    //             Debug.Log($"!!! g1 = {g1.name}");
    //             setFlag = true;
    //         }
    //         else if (g2 == null)
    //         {
    //             g2 = go;
    //             t2 = g2.transform.position;
    //             Debug.Log($"!!! g2 = {g2.name}");
    //             setFlag = true;
    //         }
    //         if (setFlag && g1 != null && g2 != null)
    //         {
    //             c = sourceTransform.position;
    //             r = sourceTransform.rotation;
    //         }
    //     }
    // }

    // void OnTriggerExit(Collider other)
    // {
    //     if (other.gameObject.tag == "Finger")
    //     {
    //         if (g1 == null || g2 == null)
    //         {
    //             g1 = g2 = null;
    //         }
    //     }
    // }
}
