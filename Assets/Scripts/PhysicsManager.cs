using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager physics;
    public HasGravity[] gravity;

    private void Start()
    {
        physics = this;
    }
}
