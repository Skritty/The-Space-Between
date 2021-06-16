using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public int damage;
    public float speed;
    [SerializeField] private float destroyAfter = 10;
    [SerializeField] private bool destroyOnImpact;
    [SerializeField] private AudioClip missileSound;

    private void Start()
    {
        Destroy(gameObject, destroyAfter);
        AudioSource.PlayClipAtPoint(missileSound, transform.position);
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.GetComponent<Health>().currentHealth -= damage;
        }
        if (destroyOnImpact) Destroy(gameObject);
    }
}
