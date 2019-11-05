using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FireRaycastProp : MonoBehaviour
{
    public GameObject firePrefab;
    public GameObject flammableObjectsParent;
    public Transform fireParent;
    public LayerMask fireLayerMask;
    public Material burntMaterial;

    public int areaSize;
    public float spreadChance;
    public float burnTime;

    public float particleDistance;

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
                Collider[] colliders = Physics.OverlapSphere(particle.transform.position, particleDistance * 2);
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

                    float random_percent = Random.Range(0.0f, colliders.Length) / (spreadChance / 100);

                    if (random_percent < ignited_neighbours && !fire_spread.ignited)
                    {
                        fire_spread.Ignite(burnTime);
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

        Transform[] objectsInScene = flammableObjectsParent.GetComponentsInChildren<Transform>();

        foreach (Transform obj in objectsInScene)
        {
            if (obj.gameObject != gameObject)
            {
                Collider collider = obj.gameObject.GetComponent<Collider>();
                if (collider != null)
                {
                    if (obj.GetComponent<MeshCollider>() != null)
                    {
                        if (obj.GetComponent<MeshCollider>().convex)
                        {
                            GenerateParticlesOnObject(collider);
                        }
                    }
                    else
                    {
                        GenerateParticlesOnObject(collider);
                    }
                }

            }
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, particleDistance * 1.5f);
        if (colliders.Length != 0)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.GetComponent<FireRaycastSpread>() != null)
                {
                    collider.gameObject.GetComponent<FireRaycastSpread>().Ignite(burnTime);
                    break;
                }
            }
        }
    }

    /*void GenerateGrid()
    {
        for (int i = -areaSize / 2; i < areaSize / 2; i++)
        {
            for (int j = -areaSize / 2; j < areaSize / 2; j++)
            {
                GameObject temp_obj = GenerateParticle(i * particleDistance, transform.position.y, j * particleDistance);

                if (i == 0 && j == 0)
                {
                    temp_obj.GetComponent<FireRaycastSpread>().Ignite(burnTime);
                }

                Collider[] colliders = Physics.OverlapSphere(temp_obj.transform.position, 1);
                if (colliders.Length != 0)
                {
                    foreach (Collider collider in colliders)
                    {
                        if (!colliding_objects.Contains(collider.gameObject) && ((1 << collider.gameObject.layer) & fireLayerMask) != 0)
                        {
                            GenerateParticlesOnObject(collider);
                        }
                    }
                }
            }
        }
    }*/

    void GenerateParticlesOnObject(GameObject obj)
    {
        Mesh mesh_filter = obj.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh_filter.vertices;

        foreach (Vector3 vertex in vertices)
        {
            Vector3 world_pos = obj.transform.TransformPoint(vertex);
            GenerateParticle(world_pos.x, world_pos.y, world_pos.z);
        }

        colliding_objects.Add(obj);
    }

    void GenerateParticlesOnObject(Collider obj)
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
                        GenerateParticle(pos_x, pos_y, pos_z);
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

    GameObject GenerateParticle(float x, float y, float z)
    {
        GameObject temp_obj = Instantiate(firePrefab, fireParent);
        temp_obj.transform.position = new Vector3(x, y, z);

        particles.Add(temp_obj);

        return temp_obj;
    }
}
