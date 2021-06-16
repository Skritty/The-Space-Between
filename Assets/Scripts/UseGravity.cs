using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseGravity : MonoBehaviour
{
    public bool onlyParent;
    private void Awake()
    {
        if(!GetComponent<ConstantForce>()) gameObject.AddComponent<ConstantForce>();
    }
    private void Start()
    {
        if (transform.parent) GetComponent<Rigidbody>().velocity =
                 (Vector3.ProjectOnPlane(transform.forward, (transform.position - transform.parent.gameObject.transform.position).normalized).normalized) *
                 Mathf.Sqrt(transform.parent.gameObject.GetComponent<Rigidbody>().mass *
                 ((1/(transform.parent.gameObject.transform.position - transform.position).magnitude)));
    }

    private void FixedUpdate()
    {
        GetComponent<ConstantForce>().force = CalculatePull();
    }

    private Vector3 CalculatePull()
    {
        Vector3 pull = new Vector3();
        if(onlyParent && transform.parent && transform.parent.GetComponent<HasGravity>()) pull += (transform.parent.transform.position - transform.position).normalized * 
                transform.parent.GetComponent<Rigidbody>().mass * GetComponent<Rigidbody>().mass / 
                (transform.parent.transform.position - transform.position).sqrMagnitude;
        else foreach (HasGravity g in GenerateSpace.gravity)
        {
            if (g && g.gameObject != gameObject) pull += (g.transform.position - transform.position).normalized * 
                    g.GetComponent<Rigidbody>().mass * GetComponent<Rigidbody>().mass / 
                    (g.transform.position - transform.position).sqrMagnitude;
        }
        return pull;
    }
}
