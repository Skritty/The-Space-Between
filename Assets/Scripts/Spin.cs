using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(new Vector3(.01f, 0, .01f), Space.World);
        GetComponent<Light>().color = new Color(Mathf.Abs(transform.rotation.x), .4f, Mathf.Abs(transform.rotation.y + transform.rotation.z));
    }
}
