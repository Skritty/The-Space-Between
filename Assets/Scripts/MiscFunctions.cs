using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location //Used to keep track of a point on a moving/rotating surface over time
{
    public Transform transform;
    private Quaternion rotFromUp;
    private float distFromCenter;
    private Quaternion surfaceNormalFromUp;
    public Vector3 point { get { return rotFromUp * -transform.up * distFromCenter + transform.position; } }
    public Vector3 normal { get { return surfaceNormalFromUp * transform.up; } }

    public Location(RaycastHit hit)
    {
        transform = hit.transform;
        rotFromUp = Quaternion.FromToRotation(transform.up, transform.position - hit.point);
        surfaceNormalFromUp = Quaternion.FromToRotation(transform.up, hit.normal);
        distFromCenter = Vector3.Distance(transform.position, hit.point);
    }

    public Location(ContactPoint cp)
    {
        transform = cp.otherCollider.transform;
        rotFromUp = Quaternion.FromToRotation(transform.up, transform.position - cp.point);
        surfaceNormalFromUp = Quaternion.FromToRotation(transform.up, cp.normal);
        distFromCenter = Vector3.Distance(transform.position, cp.point);
    }

    public Location(Transform objTransform, Vector3 surfacePoint, Vector3 normal)
    {
        transform = objTransform;
        rotFromUp = Quaternion.FromToRotation(transform.up, transform.position - surfacePoint);
        surfaceNormalFromUp = Quaternion.FromToRotation(transform.up, normal);
        distFromCenter = Vector3.Distance(transform.position, surfacePoint);
    }
}
