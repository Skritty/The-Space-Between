using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectArea : MonoBehaviour
{
    [SerializeField] private Effect type;
    [SerializeField] private int amount;
    [SerializeField] private bool destroyOnUse;
    [SerializeField] private float secondsBetweenEffect;
    private float timeLeft = 0;
    private void OnTriggerStay(Collider other)
    {
        if(type == Effect.Health && other.GetComponent<Health>() && timeLeft <= 0)
        {
            other.GetComponent<Health>().currentHealth += amount;
            if (other.GetComponent<Health>().currentHealth > other.GetComponent<Health>().maxHealth) other.GetComponent<Health>().currentHealth = other.GetComponent<Health>().maxHealth;
            timeLeft = secondsBetweenEffect;
            if (destroyOnUse) Destroy(gameObject);
        }
        else if(type == Effect.Ammo && other.GetComponent<PlayerController>() && timeLeft <= 0)
        {
            other.GetComponent<PlayerController>().currentAmmo += amount;
            if(other.GetComponent<PlayerController>().currentAmmo > other.GetComponent<PlayerController>().maxAmmo) other.GetComponent<PlayerController>().currentAmmo = other.GetComponent<PlayerController>().maxAmmo;
            timeLeft = secondsBetweenEffect;
            if (destroyOnUse) Destroy(gameObject);
        }
    }
    private void FixedUpdate()
    {
        timeLeft -= Time.fixedDeltaTime;
    }
}
enum Effect { Health, Ammo }