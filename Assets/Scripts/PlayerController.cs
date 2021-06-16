using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Stats
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform feet;
    [SerializeField] private Vector2 lookSpeed;
    [SerializeField] private float jumpRotationSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpRange;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip gravSound;
    [SerializeField][Range(0,1)] private float pullRate = .1f;
    private Health hp;
    //Calcs
    private Rigidbody rb;
    private ConstantForce cf;
    private bool jumping;
    [SerializeField] private Text ammoText;
    [SerializeField] private Image healthBar;

    private RaycastHit hit;
    private float timeFalling;
    private bool lerping;

    private StickToGround stg;
    
    private Vector3 jumpTarget;
    private Vector3 surfaceNormalAvg;

    public int maxAmmo = 100;
    public int currentAmmo;
    private GameObject[] gravObjs = new GameObject[2];
    private GameObject[] gravTargeters = new GameObject[2];
    private Location[] gravPoints = new Location[2];
    [SerializeField] private GameObject[] gravPages = new GameObject[2];
    [SerializeField] private GameObject targeter;
    [SerializeField] private float gravDistMax;
    [SerializeField] private GameObject distMarker;
    private GameObject distMarker2;

    private void Start()
    {
        transform.parent = null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        cf = GetComponent<ConstantForce>();
        stg = GetComponent<StickToGround>();
        hp = GetComponent<Health>();
        currentAmmo = maxAmmo;
        stg.movement = Vector3.zero;
        lerping = false;
    }

    private void Update()
    {
        if(healthBar) healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(25 * hp.currentHealth, 50);
        if(ammoText) ammoText.text = "Grav Selections Remaining (Ammo): "+currentAmmo+"/"+maxAmmo;

        if (Application.isEditor && Input.GetKeyDown(KeyCode.Escape)) cameraPause = !cameraPause;
        if (Input.GetKeyDown(KeyCode.Mouse2) && !jumping) stg.Jump();
        if (Input.GetKeyDown(KeyCode.Mouse1) && currentAmmo > 0) ObjectGrav(1);
        if (Input.GetKeyDown(KeyCode.Mouse0) && currentAmmo > 0) ObjectGrav(0);
        if (stg.grounded && Input.GetKey(KeyCode.Space))rb.velocity = stg.surface.normal * jumpHeight;
        if (Input.GetKeyDown(KeyCode.R)) { ResetGravObj(0); ResetGravObj(1); }
        if (Input.GetKeyDown(KeyCode.LeftShift)) { stg.Jump(); if(jumpSound) AudioSource.PlayClipAtPoint(jumpSound, transform.position, .2f); }

    }

    private void ObjectGrav(int num)
    {
        if(Cursor.lockState == CursorLockMode.Locked && Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2)), out hit))//, gravDistMax))
        {
            if (gravObjs[num])
            {
                //gravPages[num].SetActive(false);
                Destroy(gravTargeters[num]);
                foreach (GameObject g in gravObjs)
                    if (g && g.GetComponent<Rigidbody>())
                    {
                        g.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        g.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    }
                gravObjs[num] = null;
                gravPoints[num] = null;
            }
            //currentAmmo--;
            if(gravSound) AudioSource.PlayClipAtPoint(gravSound, hit.point, .6f);
            gravPoints[num] = new Location(hit.transform, hit.point, hit.normal);
            gravTargeters[num] = Instantiate(targeter, hit.collider.gameObject.transform, false);
            gravTargeters[num].transform.position = hit.point;
            gravTargeters[num].transform.localScale = new Vector3(.2f / hit.transform.lossyScale.x, .2f / hit.transform.lossyScale.y, .2f / hit.transform.lossyScale.z);
            //gravPages[num].SetActive(true);
            gravObjs[num] = hit.collider.gameObject;
        }
        else
        {
            if(!distMarker2) Destroy(distMarker2 = Instantiate(distMarker, cameraTransform.position + Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2)).direction * gravDistMax, cameraTransform.rotation, cameraTransform), 1f);
            if (gravObjs[num])
            {
                //gravPages[num].SetActive(false);
                Destroy(gravTargeters[num]);
                foreach (GameObject g in gravObjs)
                    if (g && g.GetComponent<Rigidbody>())
                    {
                        //g.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        //g.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    }
                gravObjs[num] = null;
                gravPoints[num] = null;
            }
        }
    }

    private void ResetGravObj(int num)
    {
        //gravPages[num].SetActive(false);
        Destroy(gravTargeters[num]);
        foreach (GameObject g in gravObjs)
            if (g && g.GetComponent<Rigidbody>())
            {
                //g.GetComponent<Rigidbody>().velocity = Vector3.zero;
                //g.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        gravObjs[num] = null;
        gravPoints[num] = null;
    }

    private bool cameraPause = false;
    private void CameraControls()
    {
        if (cameraPause) return;
        cameraTransform.RotateAround(cameraTransform.position, transform.up, Input.GetAxis("Mouse X") * lookSpeed.x);
        cameraTransform.RotateAround(cameraTransform.position, cameraTransform.right, Input.GetAxis("Mouse Y") * -lookSpeed.y);
        Camera.main.transform.position = cameraTransform.position;
        Camera.main.transform.rotation = cameraTransform.rotation;
    }

    private void FixedUpdate()
    {
        CameraControls();
        if (jumping) cf.relativeForce = Vector3.zero;
        Controls();
        if(gravObjs[0] && gravObjs[1]) CalculatePulls();
    }

    private void CalculatePulls()
    {
        for (int i = 0; i < 2; i++)
        {
            if (gravObjs[i].GetComponent<Rigidbody>())
            {
                //if (Vector3.Distance(gravPoints[0].Point(), gravPoints[0].Point()) < .5f)
                //{
                    Vector3 pull = (gravPoints[(i + 1) % 2].point - gravPoints[i].point).normalized * pullRate;
                    /*if (Vector3.Project(pull, gravPoints[i].Normal()).normalized == -gravPoints[i].Normal())//If behind the targeted surface
                    {
                        gravObjs[i].GetComponent<Rigidbody>().AddForce(pull, ForceMode.VelocityChange);
                    }
                    else if(pull.normalized == gravPoints[i].Normal())//If the pull direction is exactly towards the target
                    {
                        gravObjs[i].GetComponent<Rigidbody>().AddForce(pull, ForceMode.VelocityChange);
                    }*/
                    /*else */
                    gravObjs[i].GetComponent<Rigidbody>().AddForceAtPosition(pull, gravPoints[i].point, ForceMode.VelocityChange);

                    //Debug.DrawLine(gravPoints[i].Point(), gravPoints[i].Point() + gravPoints[(i + 1) % 2].Point() - gravPoints[i].Point(), Color.red);
                    //Debug.DrawRay(gravPoints[i].Point(), gravPoints[i].Normal(), Color.red);
                //}
                //else gravObjs[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
        //ResetGravObjs();
    }
    

    private void Controls()
    {
        if (stg.surface == null || !cameraTransform) return;
        stg.movement += Input.GetAxis("Vertical") * Vector3.ProjectOnPlane(cameraTransform.forward, stg.surface.normal).normalized * walkSpeed;
        stg.movement += Input.GetAxis("Horizontal") * Vector3.ProjectOnPlane(cameraTransform.right, stg.surface.normal).normalized * walkSpeed;
        //if (Physics.Raycast(new Ray(feet.position, stg.movement.normalized), out hit, stg.movement.magnitude)) stg.movement = Vector3.ProjectOnPlane(stg.movement, hit.normal);
    }

    private IEnumerator LerpRot()
    {
        lerping = true;
        Quaternion start = transform.rotation;
        Vector3 start2 = transform.up;
        Vector3 normal = stg.surface.normal;
        float lerpTime = 0;
        if (Physics.Raycast(new Ray(transform.position, -normal), out hit, .5f)) surfaceNormalAvg = (surfaceNormalAvg + hit.normal)/2;
        while (lerpTime < .1f)
        {
            lerpTime += Time.fixedDeltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.FromToRotation(Vector3.up, normal), lerpTime / .1f);
            yield return new WaitForEndOfFrame();
        }
        lerping = false;
    }
}
