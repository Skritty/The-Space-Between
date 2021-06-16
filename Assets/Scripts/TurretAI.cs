using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAI : MonoBehaviour
{
    [SerializeField] private float lookRot;
    [SerializeField] private float coneOfVision;
    [SerializeField] private float coneOfAttack;
    [SerializeField] private GameObject missile;
    [SerializeField] private Transform barrelExit;
    [SerializeField] private float attackCooldown;
    private Health hp;
    private IEnumerator firing;
    private bool tracking;
    private Transform target;

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
        if (other.tag == "Player" && Vector3.Angle(other.transform.position - transform.position, transform.forward) <= coneOfVision)
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
        if(target != null)
        {
            AIMovement();
            AIAttack();
        }
        
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
        if (tracking)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, target.position - transform.position, lookRot * Mathf.Deg2Rad * Time.deltaTime, 0));
        }
    }

    private void AIAttack()
    {
        if (tracking && Vector3.Angle(target.position - transform.position, transform.forward) <= coneOfAttack)
        {
            if(firing == null)
            {
                firing = Attack();
                StartCoroutine(firing);
            }
        }
        else if(firing != null)
        {
            StopCoroutine(firing);
            firing = null;
        }
    }
}
