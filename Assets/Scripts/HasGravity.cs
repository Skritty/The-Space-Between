using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasGravity : MonoBehaviour
{
    public float massScale;
    private void Awake()
    {
        GetComponent<Rigidbody>().mass = transform.localScale.magnitude * massScale * 4;
    }
}
