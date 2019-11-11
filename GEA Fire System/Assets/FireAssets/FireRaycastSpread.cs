using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRaycastSpread : MonoBehaviour
{
    public FireMaterial material;
    
    public bool ignited = false;
    public bool burntOut = false;

    public float burnOutTime = 0;

    public void Ignite()
    {
        GetComponent<ParticleSystem>().Play();
        // Leave Burn Decal

        burnOutTime = Time.time + material.burnTime;
        ignited = true;
    }

    public void BurnOut()
    {
        if (Time.time >= burnOutTime)
        {
            GetComponent<ParticleSystem>().Stop();
            burntOut = true;
        }
    }
}
