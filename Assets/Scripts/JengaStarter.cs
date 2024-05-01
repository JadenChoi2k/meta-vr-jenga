using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JengaStarter : MonoBehaviour
{
    [SerializeField]
    private GameObject floorObject;
    private List<GameObject> floors = new();
    private float blockHeight = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 1; i <= 8; i++)
        {
            StartCoroutine(waitAndCreate(i));
        }
    }

    private IEnumerator waitAndCreate(int floor)
    {
        yield return new WaitForSeconds(0.25f * floor);
        create(floor);
    }

    private IEnumerator waitAndResetDrag(int floor)
    {
        yield return new WaitForSeconds(0.2f * floor);
        var f = floors[18 - floor];
        f.transform.GetChild(0).GetComponent<Rigidbody>().drag = 20;
        f.transform.GetChild(1).GetComponent<Rigidbody>().drag = 20;
        f.transform.GetChild(2).GetComponent<Rigidbody>().drag = 20;
    }

    // 1...18
    private GameObject create(int floor)
    {
        var go = Instantiate(floorObject, transform.position + new Vector3(0, blockHeight * floor - blockHeight / 2, 0), floor % 2 == 0 ? Quaternion.identity : Quaternion.Euler(0, 90, 0));
        go.transform.parent = transform;
        floors.Add(go);
        return go;
    }
}
