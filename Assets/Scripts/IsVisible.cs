using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsVisible : MonoBehaviour
{
    public Camera except;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Camera.main.gameObject.SetActive(!Camera.main.gameObject.activeSelf);
        /*foreach (Camera c in FindObjectsOfType<Camera>(true)) c.gameObject.SetActive(false);
        if (except) except.gameObject.SetActive(true);
        //Debug.Log(GetComponent<Renderer>().isVisible);
        foreach (Camera c in FindObjectsOfType<Camera>(true)) c.gameObject.SetActive(true);*/
    }

    private void OnBecameVisible()
    {
        Debug.Log("visible");
    }
}
