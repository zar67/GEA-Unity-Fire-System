using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Fire Material")]
public class FireMaterial : ScriptableObject
{
    // Fire Particle Changing Values
    [Range(0.0f, 1.0f)]
    public float spreadChance = 0;
    public float burnTime = 0;
    public float particleDistance = 0;
}
