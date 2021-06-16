using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitVelocity : MonoBehaviour
{
    [SerializeField] private float maxVelocity;
    [SerializeField] private float maxAngularVelocity;
    private void FixedUpdate()
    {
        
        if (GetComponent<Rigidbody>().velocity.magnitude > maxVelocity) GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * maxVelocity * Time.fixedDeltaTime;
        if (GetComponent<Rigidbody>().angularVelocity.magnitude * Mathf.Rad2Deg > maxAngularVelocity)
            GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity.normalized * maxAngularVelocity * Time.fixedDeltaTime;
    }
}
