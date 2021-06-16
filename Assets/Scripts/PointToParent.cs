using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointToParent : MonoBehaviour
{
    void Start()
    {
        transform.LookAt(transform.parent);
        transform.rotation *= Quaternion.Euler(180f, 0f, 0f);
    }
}
