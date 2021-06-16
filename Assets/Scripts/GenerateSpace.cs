using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateSpace : MonoBehaviour
{
    public float thetaMin = 0;
    public float thetaMax = 2;
    public float rhoMin = 0;
    public float rhoMax = 2;
    public float radiusMax;
    public float radiusMin;
    public List<Generateable> stuff = new List<Generateable>();
    private List<Vector3> spotsTaken = new List<Vector3>();
    
    public static HasGravity[] gravity;

    [System.Serializable]
    public class Generateable
    {
        public GameObject obj;
        public int density;
        public bool scaleSamePercent = true;
        public Vector3 sizeMax;
        public Vector3 sizeMin;
        public float massScale = 1;
    }

    private void Awake()
    {
        foreach (Generateable thing in stuff) Generate(thing);
        gravity = FindObjectsOfType<HasGravity>();
       // SpaceshipControls.ship.transform.parent = transform;
    }

    private void Generate(Generateable thing)
    {
        spotsTaken.Clear();
        float threshold = thing.sizeMax.x * 4f;
        Vector3 pos;
        Vector3 scale;
        float theta;
        float rho;
        float phi;
        GameObject obj; 
        for (int i = 0; i < thing.density; i++)
        {
            theta = 2f * Mathf.PI * Random.Range(thetaMin, thetaMax);
            rho =Mathf.Acos(2f * Random.Range(rhoMin, rhoMax) - 1);
            phi = Random.Range(radiusMin, radiusMax);
            pos = new Vector3(phi * Mathf.Sin(rho) * Mathf.Cos(theta), phi * Mathf.Sin(rho) * Mathf.Sin(theta), phi * Mathf.Cos(rho)) + transform.position;
            if (float.IsNaN(pos.x)) continue;
            if (thing.scaleSamePercent)
            {
                float percent = Random.Range(0f, 1f);
                scale = new Vector3(thing.obj.transform.localScale.x * Mathf.Lerp(thing.sizeMax.x, thing.sizeMin.x, percent),
                    thing.obj.transform.localScale.y * Mathf.Lerp(thing.sizeMax.y, thing.sizeMin.y, percent),
                    thing.obj.transform.localScale.z * Mathf.Lerp(thing.sizeMax.z, thing.sizeMin.z, percent));
            }
            else scale = new Vector3(Random.Range(thing.sizeMax.x, thing.sizeMin.x), Random.Range(thing.sizeMax.y, thing.sizeMin.y), Random.Range(thing.sizeMax.z, thing.sizeMin.z));
            //print(pos - transform.position);
            if (spotsTaken.Count > 0)
                foreach (Vector3 spot in spotsTaken)
                {
                    if (Vector3.Distance(pos, spot) > threshold)
                    {
                        obj = Instantiate(thing.obj, transform);
                        obj.transform.position = pos;
                        obj.transform.rotation = Random.rotation;
                        obj.transform.localScale = scale / transform.localScale.x;
                        if (obj.GetComponent<Rigidbody>()) obj.GetComponent<Rigidbody>().mass = scale.magnitude * thing.massScale;
                        spotsTaken.Add(pos);
                        break;
                    }
                }
            else
            {
                obj = Instantiate(thing.obj, transform);
                obj.transform.position = pos;
                obj.transform.rotation = Random.rotation;
                obj.transform.localScale = scale / transform.localScale.x;
                if (obj.GetComponent<Rigidbody>()) obj.GetComponent<Rigidbody>().mass = scale.magnitude * thing.massScale;
                spotsTaken.Add(pos);
            }
        }
    }
}
