using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

using UnityEditor;

public class Wfr_WorldPos : MonoBehaviour
{
    [Header("模型空间坐标 动态修改可在WfrM 对应空间中查看结果值")]
    public Vector3 Sub_ModelPos;
    [Header("SubModelPos 在世界空间中的坐标值  ")]
    public Vector4 WfrM_WorldPos;
     
    public Vector3 WfrM_WorldPos_V3;
    [Space(10)]
    [Header("SubModelPos 在视觉空间(相机空间)中的坐标值  ")]
    public Vector4 WfrM_ViewSpacePos;
    
    public Vector3 WfrM_ViewSpacePos_V3;

    [Space(10)]
    [Header("SubModelPos 在裁剪空间中的坐标值  ")]
    public Vector4 WfrM_ClipSpacePos;
    
    public Vector3 WfrM_ClipSpacePos_V3;
 
    [Space(10)]
    [Header("SubModelPos NDC坐标值(ClipPos过齐次除法的结果)  ")]
    public Vector4 WfrM_NDCPos;
    
    [Space(10)]
    [Header("SubModelPos 屏幕空间坐标   ")]
    public Vector3 WfrM_ScreenPos;

    
    
    public bool IsInCameraFrustum = false;
    
    
    [Space(10)]
    [SerializeField]
    private EntityMatrixComponent _MatrixCom;
    
    
    [Space(30)]
  
    public Vector3 UnityM_WorldPos;
    [Header("用于 使用自己的矩阵将 UnityM_WorldPos 转换到 模型空间")]
    public Vector3 WfrM_WorldPosToSubModel;
    
    [Header("用于 使用Unity矩阵 将submodelpos 转换到 世界坐标")]
    public Transform SubModel_Trans;
  
    [Header("用于  显示 SubModel_Trans 在Unity矩阵变换的世界坐标")]
    public Vector3 SubTrans_WorldSpacePos;
     
    
    void Start()
    {
        if (_MatrixCom != null)
        {
            _MatrixCom.UpdateMatrix(this.transform,true);
        } 
     
    }
 
  
    
    // Update is called once per frame
    void Update()
    {
        if (SubModel_Trans != null)
        {
            SubModel_Trans.localPosition = Sub_ModelPos;
            SubTrans_WorldSpacePos = SubModel_Trans.position;
        }

        
        if (_MatrixCom != null)
        {
            _MatrixCom.UpdateMatrix(this.transform);
            
            WfrM_WorldPos = Wfr_Math.TransPointByMatrix_V3_2_V4(_MatrixCom.MainMatrix,Sub_ModelPos);

            WfrM_WorldPos_V3 = WfrM_WorldPos;
            
            WfrM_WorldPosToSubModel = Wfr_Math.TransPointByMatrix(_MatrixCom.MainMatrix_Inverse,UnityM_WorldPos);
            
            WfrM_ViewSpacePos = Wfr_Math.TransPointByMatrix_V3_2_V4(Wfr_Camera.Instance.GetWorldToViewMatrix(),WfrM_WorldPos);
 
            WfrM_ClipSpacePos = Wfr_Camera.Instance.GetClipSpacePos(WfrM_ViewSpacePos);
            
            WfrM_ClipSpacePos_V3 = WfrM_ClipSpacePos;
            
            IsInCameraFrustum =Wfr_Camera_FrustumMatrix.IsInFrustum(WfrM_ClipSpacePos); // 判断的是裁剪空间的数据
            
            WfrM_ViewSpacePos_V3 = WfrM_ViewSpacePos;

            WfrM_NDCPos = Wfr_Math.Homogeneous_Division(WfrM_ClipSpacePos);

            WfrM_ScreenPos = Wfr_Math.GetScreenPos(WfrM_ClipSpacePos,new Vector2(Wfr_Camera.Instance.WidthPixel,Wfr_Camera.Instance.HeightPixel));
            // 将外部坐标转换为自己坐标 
        } 
         
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsInCameraFrustum ? Color.green : Color.red;
        Gizmos.DrawSphere(this.transform.position,this.transform.localScale.x);
    }
}

[CustomEditor(typeof(Wfr_WorldPos))]
public class WorldPos_Inspector:UnityEditor.Editor
{
    
    private bool _Show_ClipPos;
    
    private bool _Show_NDCPos;
    
    private bool _Show_CameraPos;
    
    private bool _ShowCameraToWorldPos;
    
    
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying)
        {
            return;
        }
        
        Wfr_WorldPos target_  =target as Wfr_WorldPos;
        
        GUIDrawPosShow(target_.WfrM_WorldPos,"世界空间坐标 WorldPos"); 
        
        GUIDrawPosShow(target_.WfrM_ViewSpacePos,"相机空间坐标 ViewPos"); 
        _ShowCameraToWorldPos = GUILayout.Toggle(_ShowCameraToWorldPos,"是否显示相机空间坐标转换到世界坐标");
        
        if (_ShowCameraToWorldPos)
        { 
            GUILayout.Space(10);
            GUILayout.Label("由当前相机空间转换得到的世界坐标");   
            Vector4 valuepos_ =
                Wfr_Math.TransPointByMatrix(Wfr_Camera.Instance.GetViewToWorldMatrix(),  target_.WfrM_ViewSpacePos);
            GUIDrawPosShow(valuepos_,"相机空间转换至世界空间 worldPos:"); 
            
            GUILayout.Space(10);
        }
        
        GUIDrawPosShow(target_.WfrM_ClipSpacePos,"裁剪空间坐标 ClipPos"); 
        _Show_ClipPos = GUILayout.Toggle(_Show_ClipPos,"是否显示裁剪空间坐标转换到世界坐标");
        
        if (_Show_ClipPos)
        { 
            GUILayout.Space(10);
            GUILayout.Label("由当前裁剪空间转换得到的世界坐标");   
            Vector4 valuepos_ =
                Wfr_Math.TransPointByMatrix(Wfr_Camera.Instance.GetProjectToViewMatrix(),  target_.WfrM_ClipSpacePos);
            GUIDrawPosShow(valuepos_,"裁剪空间转换至相机空间 ViewPos "); 
            Vector4 valuepos_1_ =
                Wfr_Math.TransPointByMatrix(Wfr_Camera.Instance.GetViewToWorldMatrix(),  valuepos_);
            GUIDrawPosShow(valuepos_1_,"相机空间转换至世界空间  WorldPos "); 
            
            GUILayout.Space(10);
        }
        
        GUIDrawPosShow(target_.WfrM_NDCPos,"NDC空间坐标 NDCPos"); // ndc 肯定无法还原到 裁剪空间,得通过其他方式? 反正一般的还原肯定不行
        _Show_NDCPos = GUILayout.Toggle(_Show_NDCPos,"是否显示NDC坐标转换到世界坐标");
        
        if (_Show_NDCPos)
        { 
            GUILayout.Space(10);
            GUILayout.Label("由当前NDC空间转换得到的世界坐标");   
            
            Vector4 valuepos_0 =
                Wfr_Math.Homogeneous_Division_Inverse(target_.WfrM_NDCPos);
            GUIDrawPosShow(valuepos_0,"NDC 坐标 转换回 ClipPos "); 
            
            Vector4 valuepos_ =
                Wfr_Math.TransPointByMatrix(Wfr_Camera.Instance.GetProjectToViewMatrix(), valuepos_0);
            GUIDrawPosShow(valuepos_,"裁剪空间转换至相机空间 ViewPos "); 
            Vector4 valuepos_1_ =
                Wfr_Math.TransPointByMatrix(Wfr_Camera.Instance.GetViewToWorldMatrix(),  valuepos_);
            GUIDrawPosShow(valuepos_1_,"相机空间转换至世界空间  WorldPos "); 
            
            GUILayout.Space(10);
        }
        
        // ndc 算法2  通过ndc 推算出w. 然后再进行处理. 
    }
 

    public static void GUIDrawPosShow(Vector4 valuepos_,string title_ = null)
    { 
        EditorGUI.indentLevel++;
       // GUILayout.BeginHorizontal();  
       // GUILayout.Space(10);   
        GUILayout.Label($"{title_} : [{valuepos_.x} , {valuepos_.y} , {valuepos_.z} , {valuepos_.x}]");
            
      //  GUILayout.EndHorizontal();
        
        EditorGUI.indentLevel--;
    }
    
    public static void GUIDrawPosShow(Vector3 valuepos_,string title_ = null)
    {
        EditorGUI.indentLevel++;
        
       // GUILayout.BeginHorizontal();  
        
       // GUILayout.Space(10);   
        
        GUILayout.Label($"{title_} :[{valuepos_.x} , {valuepos_.y} , {valuepos_.z}  ] ]");
            
       // GUILayout.EndHorizontal();
        
        EditorGUI.indentLevel--;
    }
    
}