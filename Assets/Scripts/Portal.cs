using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A one-sided portal that links to a counterpart like a doorway
public class Portal : MonoBehaviour
{
    //Portal Parts
    public Portal link;
    public Camera view;
    public Renderer display;
    public Transform physicsScene;
    public Light l1;
    public Light l2;

    //Rendering
    public MaterialPropertyBlock portalMaterialBlock;
    private RenderTexture rendTexture;
    
    private Dictionary<Transform, Transform> visualCopies = new Dictionary<Transform, Transform>();
    private Dictionary<Transform, Vector3> teleporting = new Dictionary<Transform, Vector3>();
    private List<Vector3> points = new List<Vector3>();
    public Dictionary<Transform, TempTransform> lastTransform = new Dictionary<Transform, TempTransform>();
    public class TempTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public TempTransform(Transform t)
        {
            position = t.position;
            rotation = t.rotation;
            scale = t.localScale;
        }
    }

    private Vector2 mainCameraNearPlaneDims;
    private Vector2 center;

    private void Start()
    {
        // If not linked, link to a "random" other portal if one exists
        if (link == null)
        {
            Portal[] ps = FindObjectsOfType<Portal>();
            for (int i = (int)Random.Range(0, ps.Length-1); i < ps.Length; i++)
            {
                Portal p = ps[i];
                if (p != this && p.link == null)
                {
                    p.link = this;
                    link = p;
                }
            }
            if(link == null)
            {
                foreach(Portal p in ps)
                {
                    if (p != this && p.link == null)
                    {
                        p.link = this;
                        link = p;
                    }
                }
            }
        }

        l1.areaSize = transform.localScale;
        l2.areaSize = transform.localScale;
        mainCameraNearPlaneDims = new Vector2(
            Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(0,0, Camera.main.nearClipPlane)), Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, Camera.main.nearClipPlane))),
            Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(0,0, Camera.main.nearClipPlane)), Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, Camera.main.nearClipPlane)))
            );
        rendTexture = new RenderTexture(Camera.main.scaledPixelWidth, Camera.main.scaledPixelHeight, 16, RenderTextureFormat.ARGB32);
        rendTexture.name = "PortalTexture";
        portalMaterialBlock = new MaterialPropertyBlock();
        view.targetTexture = rendTexture;
        //CreatePhysicsScene();
    }

    private void CreatePhysicsScene()
    {
        physicsScene = Instantiate(new GameObject()).transform;
        physicsScene.name = "Physics Scene";
        physicsScene.tag = "InPortal";

        //Get all colliders in front of the linked portal
        Collider[] colliders = Physics.OverlapBox(link.transform.position + link.transform.forward * Mathf.Min(link.transform.localScale.x, link.transform.localScale.y),
            new Vector3(link.transform.localScale.x, link.transform.localScale.y, Mathf.Max(link.transform.localScale.x, link.transform.localScale.y)), link.transform.rotation);
        
        foreach (Collider c in colliders)
        {
            if (!c.isTrigger)
            {
                GameObject o = Instantiate(new GameObject(), physicsScene);
                o.gameObject.name = "collider";
                o.tag = "InPortal";
                o.layer = 9;
                o.transform.position = c.transform.position;
                o.transform.rotation = c.transform.rotation;
                o.transform.localScale = c.transform.lossyScale;
                CopyComponent(c, o);
            }
        }
    }

    private void LateUpdate()
    {
        if (!VisibleFromCamera(display, Camera.main) || Vector3.Distance(transform.position, Camera.main.transform.position) > 100) return;

        view.transform.position = Camera.main.transform.position;
        view.transform.rotation = Camera.main.transform.rotation;
        MoveAndRotateToOtherPortal(view.transform);

        //Set Projection Matrix
        Vector3 forward = link.transform.forward * Mathf.Sign(Vector3.Dot(link.transform.position - view.transform.position, link.transform.forward));
        Plane p = new Plane(forward, link.transform.position);
        Vector4 clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(view.worldToCameraMatrix)) * clipPlaneWorldSpace;
        view.projectionMatrix = Camera.main.CalculateObliqueMatrix(clipPlaneCameraSpace);

        //Get the min and max points in the screen plane of the portal 
        /*Vector2 max = new Vector2(0f, 0f);
        Vector2 min = new Vector2(1f, 1f);
        Mesh mesh = link.display.GetComponent<MeshFilter>().mesh;
        points.Clear();
        for (int x = 1; x <= 2; x++)
            for (int y = 1; y <= 2; y++)
                for (int z = 1; z <= 2; z++)
                {
                    
                    Vector3 point = link.display.transform.position 
                        + link.display.transform.rotation * new Vector3(mesh.bounds.extents.x * Mathf.Pow(-1, x) * link.display.transform.lossyScale.x, 
                        mesh.bounds.extents.y * Mathf.Pow(-1, y) * link.display.transform.lossyScale.y, 
                        mesh.bounds.extents.z * Mathf.Pow(-1, z) * link.display.transform.lossyScale.z);
                    points.Add(point);
                    Vector2 viewPt = Camera.main.WorldToViewportPoint(point);
                    
                    if (viewPt.x < min.x) min.x = Mathf.Clamp01(viewPt.x);
                    if (viewPt.x > max.x) max.x = Mathf.Clamp01(viewPt.x);
                    if (viewPt.y < min.y) min.y = Mathf.Clamp01(viewPt.y);
                    if (viewPt.y > max.y) max.y = Mathf.Clamp01(viewPt.y);
                }
        print(min.x + "|" + min.y + "|" + ((max.x - min.x) / (max.y - min.y)));
        view.usePhysicalProperties = true;
        view.focalLength = view.nearClipPlane = Mathf.Abs(view.transform.localPosition.y * transform.localScale.y);
        view.lensShift = new Vector2(view.transform.localPosition.x * transform.localScale.x / -view.sensorSize.x, view.transform.localPosition.z * transform.localScale.z / view.sensorSize.y);
        center = (min + max) / 2;
        view.rect = new Rect(min.x, min.y, max.x-min.x, max.y-min.y);
        view.sensorSize = new Vector2(Camera.main.sensorSize.x * (max.x - min.x), Camera.main.sensorSize.y * (max.y - min.y));
        view.lensShift = new Vector2(center.x  -.5f, center.y - .5f);
        view.aspect = (max.x - min.x) / (max.y - min.y);
        view.focalLength = Camera.main.focalLength;*/
        AntiClip();

        /*foreach(KeyValuePair<Transform, TempTransform> t in lastTransform)
        {
            MoveAndRotateToOtherPortal(t.Key);
        }*/

        link.display.enabled = false;
        view.Render();
        link.display.enabled = true;

        /*foreach (KeyValuePair<Transform, TempTransform> t in lastTransform)
        {
            link.MoveAndRotateToOtherPortal(t.Key);
        }*/

        display.GetPropertyBlock(portalMaterialBlock);
        portalMaterialBlock.SetTexture("_PortalTexture", rendTexture);
        display.SetPropertyBlock(portalMaterialBlock);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!teleporting.ContainsKey(other.transform)) teleporting.Add(other.transform, other.transform.position);
        MakeStaticVisualCopyOfLineage(other.transform);
    }

    private void MakeStaticVisualCopyOfLineage(Transform t)
    {
        if (!visualCopies.ContainsKey(t) && t.GetComponent<Renderer>())
        {
            GameObject o = new GameObject();
            o.transform.rotation = t.rotation;
            o.transform.position = t.position;
            o.transform.localScale = t.localScale;
            CopyComponent<MeshFilter>(t.GetComponent<MeshFilter>(), o);
            CopyComponent<Renderer>(t.GetComponent<Renderer>(), o);
            visualCopies.Add(t, o.transform);
            MoveAndRotateToOtherPortal(o.transform);
        }
        foreach (Transform child in t)
        {
            MakeStaticVisualCopyOfLineage(child);
        }
    }

    private void RemoveCopies(Transform t)
    {
        if (visualCopies.ContainsKey(t))
        {
            Destroy(visualCopies[t].gameObject);
            visualCopies.Remove(t);
        }
        foreach (Transform child in t)
        {
            RemoveCopies(child);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (KeyValuePair<Transform, Vector3> obj in teleporting)
        {
            Gizmos.DrawRay(obj.Key.position, Vector3.Project(obj.Key.position - transform.position, transform.forward).normalized);
            Gizmos.DrawRay(obj.Value, Vector3.Project(obj.Value - transform.position, transform.forward).normalized);
        }
        
    }

    private void Update()
    {
        if (!VisibleFromCamera(display, Camera.main) || Vector3.Distance(transform.position, Camera.main.transform.position) > 100) return;
        List<Transform> toRemove = new List<Transform>();
        // Check to see if an object root should be teleported to the other portal
        foreach(KeyValuePair<Transform, Vector3> obj in teleporting)
        {
            Vector3 startDir = Vector3.Project(obj.Value - transform.position, transform.forward);
            Vector3 currentDir = Vector3.Project(obj.Key.position - transform.position, transform.forward);
            if (Vector3.Dot(startDir, currentDir) < 0)// && Vector3.Distance(transform.position, obj.Key.position) < 10)
            {
                Debug.Log("Teleporting "+obj.Key.name+". Distance from portal: "+Vector3.Distance(transform.position, obj.Key.position));
                MoveAndRotateToOtherPortal(obj.Key);
                toRemove.Add(obj.Key);
                //RemoveCopies(obj.Key);
            }
        }
        foreach(Transform t in toRemove)
        {
            teleporting.Remove(t);
        }
        // Update the positions of all renderers
        foreach (KeyValuePair<Transform, Transform> obj in visualCopies)
        {
            obj.Value.position = obj.Key.position;
            obj.Value.rotation = obj.Key.rotation;
            obj.Value.localScale = obj.Key.localScale;
            MoveAndRotateToOtherPortal(obj.Value);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (teleporting.ContainsKey(other.transform))
        {
            //MoveAndRotateToOtherPortal(other.transform);
            teleporting.Remove(other.transform);
        }
        RemoveCopies(other.transform);
    }

    private void AntiClip()
    {
        display.transform.localScale = new Vector3(display.transform.localScale.x, display.transform.localScale.y, 0);//(mainCameraNearPlaneDims.x * Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle(Camera.main.transform.forward, transform.forward)));
        // Make it move away from the player by half the z scale
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        RaycastHit hit;
        if(Physics.Raycast(view.ViewportPointToRay(center), out hit))
            Gizmos.DrawSphere(hit.point, .5f);
        foreach (Vector3 pt in points) Gizmos.DrawSphere(pt, .2f);
    }*/
    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, link.transform.rotation, Vector3.one);

        //Gizmos.matrix = view.projectionMatrix;
        //Gizmos.DrawFrustum(view.transform.position, view.fieldOfView, view.farClipPlane, view.nearClipPlane, view.aspect);
        
        Gizmos.DrawWireCube(link.transform.position + link.transform.forward * Mathf.Min(link.transform.localScale.x, link.transform.localScale.y), 
            new Vector3(link.transform.localScale.x, link.transform.localScale.y, Mathf.Max(link.transform.localScale.x, link.transform.localScale.y)));
    }*/

    public void MoveAndRotateToOtherPortal(Transform t)
    {
        t.position = link.transform.TransformPoint(transform.InverseTransformPoint(t.position));
        t.rotation = link.transform.rotation * Quaternion.Inverse(transform.rotation) * t.rotation;
        /*float theta = Mathf.PI;
        Matrix4x4 rotate = new Matrix4x4(new Vector4(Mathf.Cos(theta), 0, Mathf.Sin(theta), 0), new Vector4(0, 1, 0, 0), new Vector4(-Mathf.Sin(theta), 0, Mathf.Cos(theta), 0), new Vector4(0, 0, 0, 1));
        Matrix4x4 m = transform.localToWorldMatrix * rotate * link.transform.worldToLocalMatrix * Camera.main.transform.localToWorldMatrix;
        t.SetPositionAndRotation(m.GetColumn(3), m.rotation);*/
    }

    private T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        var dst = destination.GetComponent(type) as T;
        if (!dst) dst = destination.AddComponent(type) as T;
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(dst, field.GetValue(original));
        }
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }
        return dst as T;
    }

    private bool IsInside(Collider c, Vector3 point)
    {
        Vector3 closest = c.ClosestPoint(point);
        return closest == point;
    }

    private bool VisibleFromCamera(Renderer r, Camera c)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(c);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, r.bounds);
    }
}
