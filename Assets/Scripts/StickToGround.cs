using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToGround : MonoBehaviour
{
    [SerializeField] private Transform feet;
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float jumpRange = 15f;
    private float gravityScale;
    public bool grounded;
    public Location surface;
    public Vector3 movement;
    private Rigidbody rb;
    private ConstantForce cf;
    private RaycastHit hit;
    private bool jumping;
    private Vector3 jumpTarget;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cf = GetComponent<ConstantForce>();
        if (Physics.Raycast(new Ray(transform.position, -transform.up), out hit))
        {
            surface = new Location(hit);
        }
        gravityScale = 100 * rb.mass;
    }

    private void OnCollisionEnter(Collision collision)
    {
        grounded = true;
        if(useGravity) cf.force = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!jumping)
        {
            if (Physics.Raycast(new Ray(feet.position, -transform.up), out hit, 1f))
            {
                surface = new Location(hit);

                if (surface.transform.GetComponent<Rigidbody>())
                {
                    movement += surface.transform.GetComponent<Rigidbody>().velocity * Time.fixedDeltaTime +
                        surface.transform.position + Quaternion.Euler(surface.transform.GetComponent<Rigidbody>().angularVelocity * Time.fixedDeltaTime * Mathf.Rad2Deg) * (transform.position - surface.transform.position) - transform.position;
                }
                transform.rotation = Quaternion.LookRotation(transform.forward, surface.normal);
            }
            else if (useGravity)
            {
                grounded = false;
                if (surface != null) cf.force = -surface.normal * gravityScale;
                else cf.force = -transform.up * gravityScale;
            }
            //Debug.DrawLine(transform.position, transform.position + movement, Color.green, 30);
            //if (surface != null) Debug.DrawRay(surface.point, surface.normal, Color.black, 30);
            rb.MovePosition(transform.position + movement);
            movement = Vector3.zero;
        }
    }

    public void Jump()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2));
        if (Physics.Raycast(ray, out hit, jumpRange))
        {
            surface = new Location(hit);
            StartCoroutine(LerpJump(transform.position, transform.rotation));
        }
    }

    private IEnumerator LerpJump(Vector3 landingStartPos, Quaternion landingStartRot)
    {
        jumping = true;
        float threshold = Vector3.Distance(landingStartPos, surface.point)/40f;
        Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, surface.normal);
        float lerpTime = 0;
        while (Vector3.Distance(transform.position, jumpTarget) >= threshold && transform.up != surface.normal)
        {
            jumpTarget = surface.point + surface.normal * .1f + surface.normal * Vector3.Distance(feet.position, transform.position);
            lerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(landingStartPos, jumpTarget, lerpTime / Mathf.Sqrt((jumpTarget - landingStartPos).magnitude / 40));
            transform.rotation = Quaternion.Slerp(landingStartRot, targetRot, lerpTime / Mathf.Sqrt((jumpTarget - landingStartPos).magnitude / 50));
            yield return new WaitForFixedUpdate();
        }
        transform.position = jumpTarget;
        lerpTime = 0;
        jumping = false;
    }
}
