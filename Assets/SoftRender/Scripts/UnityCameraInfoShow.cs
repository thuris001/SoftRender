using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityCameraInfoShow : MonoBehaviour
{

    public Vector4 NDCPos = new Vector4(1,1,1,1);

    public Vector3 worldPos;

    public Vector3 cameraPos;

    public Vector3 ClipPos;

    public Vector3 InverseWorldPos;

    public Vector4 InverseCameraPos;
    
    public Vector4 InverseWorldPos_w;

    private Camera myCamera;
    // Update is called once per frame
    void Update()
    {
        if (myCamera == null)
        {
            myCamera = this.GetComponent<Camera>();
        }

        if (myCamera != null)
        {
            cameraPos =  myCamera.worldToCameraMatrix * worldPos;
            
            ClipPos =  myCamera.projectionMatrix * worldPos;
            
            InverseCameraPos = myCamera.projectionMatrix.inverse * NDCPos;

            Matrix4x4 worldToCamera_ = myCamera.worldToCameraMatrix;
            
            Matrix4x4 CameraToWorld_ = myCamera.cameraToWorldMatrix;
            
            InverseWorldPos_w = myCamera.cameraToWorldMatrix * InverseCameraPos;

            InverseWorldPos.x = InverseWorldPos_w.x / InverseWorldPos_w.w;
            InverseWorldPos.y = InverseWorldPos_w.y / InverseWorldPos_w.w; 
            InverseWorldPos.z = InverseWorldPos_w.z / InverseWorldPos_w.w;
        }
        

    }
}
