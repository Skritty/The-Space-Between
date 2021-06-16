using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private GameObject deathParticles;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        if (currentHealth <= 0) Kill();
    }

    public void Kill()
    {
        AudioSource.PlayClipAtPoint(deathSound, transform.position);
        Destroy(Instantiate(deathParticles, transform.position, Quaternion.identity),2);
        Destroy(gameObject);
    }
}
