using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterAI : MonoBehaviour
{
    [SerializeField] private float lookRot;
    [SerializeField] private float coneOfVision;
    [SerializeField] private float coneOfAttack;
    [SerializeField] private GameObject missile;
    [SerializeField] private AudioClip missileSound;
    [SerializeField] private Transform barrelExit;
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform[] pathingNodes;
    private Health hp;
    private IEnumerator firing;
    private bool tracking;
    private Transform target;
    private Vector3 movement;

    private void Start()
    {
        hp = GetComponent<Health>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        hp.currentHealth -= (int)collision.impulse.magnitude;
    }

    private void OnTriggerEnter(Collider other)//The trigger collider should be a large sphere
    {
        if (other.tag == "Player")
        {
            tracking = true;
            target = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            tracking = false;
        }
    }

    private void Update()
    {
        if (target != null)
        {
            if (GetComponent<Rigidbody>().velocity == Vector3.zero)
            {
                AIMovement();
            }
            transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, target.position - transform.position, lookRot * Mathf.Deg2Rad * Time.deltaTime, 0));
            AIAttack();
        }
            
    }

    private void FixedUpdate()
    {
        GetComponent<Rigidbody>().MovePosition(transform.position + movement);
    }

    private IEnumerator Attack()
    {
        RaycastHit hit;
        while (true)
        {
            if (Physics.Raycast(barrelExit.position, target.position - barrelExit.position, out hit) && hit.collider.tag == "Player")
            {
                GameObject m = Instantiate(missile, barrelExit.position, transform.rotation);
                yield return new WaitForSeconds(attackCooldown);
            }
            else yield return new WaitForFixedUpdate();

        }
    }

    private void AIMovement()
    {
        if (target)
        {
            Vector3 closestPosition = Vector3.zero;
            foreach (Transform node in pathingNodes)
            {
                if (node)
                {
                    if (closestPosition == Vector3.zero) closestPosition = node.position;
                    if (Vector3.Distance(target.position, node.position) < Vector3.Distance(target.position, closestPosition) &&
                        !Physics.Raycast(transform.position, transform.position - node.position, Vector3.Distance(transform.position, node.position))) closestPosition = node.position;
                }
            }
            movement = (closestPosition - transform.position)/50f;
        }
    }

    private void AIAttack()
    {
        if (tracking && Vector3.Angle(target.position - transform.position, transform.forward) <= coneOfAttack)
        {
            if (firing == null)
            {
                firing = Attack();
                StartCoroutine(firing);
            }
        }
        else if (firing != null)
        {
            StopCoroutine(firing);
            firing = null;
        }
    }
}
