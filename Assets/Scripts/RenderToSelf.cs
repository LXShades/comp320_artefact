using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderToSelf : MonoBehaviour
{
    Camera myCamera;

    RenderTexture myRenderTexture;

    // Start is called before the first frame update
    void Start()
    {
        GameObject cameraObject = new GameObject("ImpostorCamera", typeof(Camera));

        cameraObject.transform.position = transform.position - cameraObject.transform.forward * 5.0f;

        myCamera = cameraObject.GetComponent<Camera>();

        myRenderTexture = new RenderTexture(256, 256, 16);

        myCamera.clearFlags = CameraClearFlags.SolidColor;
        myCamera.backgroundColor = Color.red;
        myCamera.targetTexture = myRenderTexture;
        myCamera.Render();
        myCamera.targetTexture = null;
        myCamera.enabled = false;

        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = myRenderTexture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
