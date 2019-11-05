using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRaycastSpread : MonoBehaviour
{
    public bool ignited = false;
    public bool burntOut = false;

    public float burnOutTime = 0;

    public void Ignite(float burnTime)
    {
        GetComponent<ParticleSystem>().Play();
        // Leave Burn Decal

        burnOutTime = Time.time + burnTime;
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
