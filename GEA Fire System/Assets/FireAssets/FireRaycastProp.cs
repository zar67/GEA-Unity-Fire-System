using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FireRaycastProp : MonoBehaviour
{
    public GameObject firePrefab;
    public GameObject flammableObjectsParent;
    public Transform fireParent;
    public Material burntMaterial;

    List<GameObject> particles = new List<GameObject>();
    List<GameObject> colliding_objects = new List<GameObject>();

    private void Awake()
    {
        GenerateGrid();
    }

    private void Update()
    {
        foreach (GameObject particle in particles)
        {
            FireRaycastSpread fire_spread = particle.GetComponent<FireRaycastSpread>();

            if (!fire_spread.burntOut)
            {
                Collider[] colliders = Physics.OverlapSphere(particle.transform.position, fire_spread.material.particleDistance * 2);
                if (colliders.Length != 0)
                {
                    int ignited_neighbours = 0;
                    foreach (Collider collider in colliders)
                    {
                        if (collider.GetComponent<FireRaycastSpread>() != null)
                        {
                            if (collider.GetComponent<FireRaycastSpread>().ignited)
                            {
                                ignited_neighbours += 1;
                            }

                        }
                    }

                    float random_percent = Random.Range(0.0f, colliders.Length) / (fire_spread.material.spreadChance / 100);

                    if (random_percent < ignited_neighbours && !fire_spread.ignited)
                    {
                        fire_spread.Ignite();
                    }
                }

                if (fire_spread.ignited)
                {
                    fire_spread.BurnOut();
                }
            }
        }
    }

    void GenerateGrid()
    {
        // TODO: Check if object in area collider / trigger

        Transform[] flammableObjects = flammableObjectsParent.GetComponentsInChildren<Transform>();

        foreach (Transform obj in flammableObjects)
        {
            if (obj.GetComponent<FlammableObject>() != null)
            {
                FireMaterial material = obj.GetComponent<FlammableObject>().material;
                if (obj.gameObject != gameObject)
                {
                    Collider collider = obj.gameObject.GetComponent<Collider>();
                    if (collider != null)
                    {
                        if (obj.GetComponent<MeshCollider>() != null)
                        {
                            if (obj.GetComponent<MeshCollider>().convex)
                            {
                                GenerateParticlesOnObject(collider, material.particleDistance);
                            }
                        }
                        else
                        {
                            GenerateParticlesOnObject(collider, material.particleDistance);
                        }
                    }

                }
            }
        }

        GameObject closestParticle = particles[0];

        foreach (GameObject obj in particles)
        {
            if (Vector3.Distance(transform.position, closestParticle.transform.position) > Vector3.Distance(transform.position, obj.transform.position))
            {
                closestParticle = obj;
            }
        }

        if (closestParticle.GetComponent<FireRaycastSpread>() != null)
        {
            closestParticle.GetComponent<FireRaycastSpread>().Ignite();
        }
    }

    void GenerateParticlesOnObject(Collider obj, float particleDistance)
    {
        float pos_x = obj.bounds.min.x;
        float pos_y = obj.bounds.min.y;
        float pos_z = obj.bounds.min.z;

        for (int bounds_x = 0; bounds_x < (obj.bounds.size.x / particleDistance) + 1; bounds_x++)
        {
            for (int bounds_y = 0; bounds_y < (obj.bounds.size.y / particleDistance) + 1; bounds_y++)
            {
                for (int bounds_z = 0; bounds_z < (obj.bounds.size.z / particleDistance) + 1; bounds_z++)
                {
                    Vector3 pos = new Vector3(pos_x, pos_y, pos_z);

                    if (CheckPointInCollider(obj, pos))
                    {
                        GenerateParticle(pos_x, pos_y, pos_z, obj.gameObject.GetComponent<FlammableObject>().material);
                    }

                    pos_z += particleDistance;
                }
                pos_y += particleDistance;
                pos_z = obj.bounds.min.z;
            }
            pos_x += particleDistance;
            pos_y = obj.bounds.min.y;
        }

        colliding_objects.Add(obj.gameObject);
    }

    bool CheckPointInCollider(Collider obj, Vector3 point)
    {
        Vector3 closest = obj.ClosestPoint(point);
        return closest == point;
    }

    GameObject GenerateParticle(float x, float y, float z, FireMaterial material)
    {
        GameObject temp_obj = Instantiate(firePrefab, fireParent);
        temp_obj.transform.position = new Vector3(x, y, z);
        temp_obj.GetComponent<FireRaycastSpread>().material = material;

        particles.Add(temp_obj);

        return temp_obj;
    }
}
