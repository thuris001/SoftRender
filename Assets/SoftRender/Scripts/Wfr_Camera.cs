using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Wfr_Camera : MonoBehaviour
{
    private static Wfr_Camera _Instance;

    public static Wfr_Camera Instance
    {
        get
        {
            return _Instance;
        }
    }
    

    [Range(0,10)]
    public float NearZ;
    [Range(10,100)]
    public float FarZ; 
     
    [Range(1,10)]
    public float ViewSizeWidth;
    [Range(1,10)]
    public float ViewSizeHeight;
    [Range(200,1920)]
    public float WidthPixel = 400;

    public float HeightPixel
    {
        get
        {
            return WidthPixel * (Aspect_Height / Aspect_Width);
        }
    }
    
    // Start is called before the first frame update
    [Range(1,30)]
    public float Aspect_Width = 4;
    [Range(1,30)]
    public float Aspect_Height= 3;
    
    [Range(10,90)]
    public float FiledOfView = 30;


    [Range(0.05f, 2)] public float SphereR = 0.1f;
    
    [Space(10)]
    [Header("待反向计算的 NDC坐标")]
    public Vector3 NeedCal_NDCSpacePos_ = new  Vector3(-1,-1,-1);
    [Header("NDC 坐标计算出的 ViewSpace坐标")]
    public Vector4  CalFromNDCSpace_ViewPos_ = Vector4.zero;
    [Header("由NDC 坐标反向推算出的世界坐标")]
    public Vector3 CalFromNDCSpaceToWorldPos_ = Vector3.zero;
    [Header("由NDC 坐标反向推算出的世界坐标 未透除W")]
    public Vector4 CalFromNDCSpaceToWorldPos_W_ = Vector4.zero;

  
    [Header("由NDC 坐标反向推算出的世界坐标 unity 相机矩阵计算得到的坐标 ")]
    public Vector3 CalFromNDCSpaceToWorldPos_Unity = Vector3.zero;

    [Header("由NDC 坐标反向推算出的世界坐标 unity 相机矩阵计算得到的坐标 ")]
    public Vector4 CalFromNDCSpaceToWorldPos_Unity_W = Vector4.zero;
    private void Awake()
    {
        _Instance = this;
    }

    void Start()
    {
        InitRectPoint();

        if (_MatrixCom !=null)
        {
            _MatrixCom.UpdateMatrix(this.transform,true);
        }

        if (_ViewMatrixCom != null)
        {
            _ViewMatrixCom.UpdateMatrix(this.transform,true);
        }

        if (FrustumMatrix != null)
        {
            FrustumMatrix.UpdateByCamera(this,true);
        }
    }
    [SerializeField]
    public WorldRect NearRect;
    [SerializeField]
    public WorldRect FarRect;

    [SerializeField]
    private EntityMatrixComponent _MatrixCom;
    
    [SerializeField]
    private EntityMatrixComponent _ViewMatrixCom;

    [SerializeField]
    private Wfr_Camera_FrustumMatrix FrustumMatrix;


    public float GetFovAngle()
    {
        return Wfr_Math.NumToAngleValue(FiledOfView);
    }
    
    // 相机的几个点,肯定也都收到相机 矩阵影响. 朝向?
    private void InitRectPoint()
    {
        float angle_ = (GetFovAngle()/2);

        ViewSizeHeight = 2 * NearZ * Mathf.Tan(angle_);
        
        ViewSizeWidth = ViewSizeHeight * (Aspect_Width/Aspect_Height );
        
        float ViewSizeHeight_Far_ = 2 * FarZ * Mathf.Tan(angle_);
        
        float ViewSizeWidth_Far_ = ViewSizeHeight_Far_ * (Aspect_Width/Aspect_Height );
        
        NearRect = new WorldRect(GetModelCenter(NearZ), new Vector2(ViewSizeWidth,ViewSizeHeight));
        
        FarRect = new WorldRect(GetModelCenter(FarZ), new Vector2(ViewSizeWidth_Far_,ViewSizeHeight_Far_) );
    }
    
    public void UpdateRectPoint()
    { 
        float angle_ = ((FiledOfView ) *(Mathf.PI / 360)/2);

        ViewSizeHeight = 2 * NearZ * Mathf.Tan(angle_);
        
        ViewSizeWidth = ViewSizeHeight * (Aspect_Width/Aspect_Height );
        
        float ViewSizeHeight_Far_ = 2 * FarZ * Mathf.Tan(angle_);
        
        float ViewSizeWidth_Far_ = ViewSizeHeight_Far_ * (Aspect_Width/Aspect_Height );
        
        NearRect.Update(GetModelCenter(NearZ), new Vector2(ViewSizeWidth,ViewSizeHeight));
        
        FarRect.Update(GetModelCenter(FarZ), new Vector2(ViewSizeWidth_Far_,ViewSizeHeight_Far_));
    }

    public Vector3 GetRectCenter(float Z_)
    {
        Vector3 camerapoint = this.transform.position;
        
        Vector3 Center_ = new Vector3(camerapoint.x , camerapoint.y , camerapoint.z + Z_); // 需要转换到 Z 的逆矩阵.

        return Center_;  
    }
    
    public Vector3 GetModelCenter(float Z_)
    { 
        Vector3 Center_ = new Vector3(0 , 0 ,   Z_); // 需要转换到 Z 的逆矩阵.

        return Center_;  
    }
    
    // Update is called once per frame
    void Update()
    {
        if (_MatrixCom !=null)
        {
            _MatrixCom.UpdateMatrix(this.transform);
        }
        
        if (_ViewMatrixCom !=null)
        {
            _ViewMatrixCom.UpdateMatrix(this.transform);
        }

     
        if (FrustumMatrix != null)
        {
            FrustumMatrix.UpdateByCamera(this);

            CalFromNDCSpace_ViewPos_ =
                Wfr_Math.TransPointByMatrix(FrustumMatrix.FrustumMatrixl_Inverse,
                    NeedCal_NDCSpacePos_);
            // 这个是透除过的数, 逻辑上是有问题的..
            
            Vector4 clipPos_ =
                Wfr_Math.TransPointByMatrix(FrustumMatrix.FrustumMatrixl,
                    CalFromNDCSpace_ViewPos_);
            // 直接让camerapos 有 w 不为-1的结果 再转换反而接近原本效果了..不能理解.
            
   

            Matrix4x4 clip_to_view = Camera.main.projectionMatrix.inverse;

            Matrix4x4 viewToProject_ = Camera.main.cameraToWorldMatrix * clip_to_view;

            Matrix4x4.Translate(CalFromNDCSpace_ViewPos_);
            
            CalFromNDCSpaceToWorldPos_Unity_W =  viewToProject_ * new Vector4(NeedCal_NDCSpacePos_.x,NeedCal_NDCSpacePos_.y,NeedCal_NDCSpacePos_.z,1) ;
            
            CalFromNDCSpaceToWorldPos_Unity =  Wfr_Math.Homogeneous_Division(CalFromNDCSpaceToWorldPos_Unity_W);
            
            if (_ViewMatrixCom.MainMatrixInverse_FloatCal != null)
            {
                CalFromNDCSpaceToWorldPos_W_  =    Wfr_Math.TransPointByMatrix(GetViewToWorldMatrix(),
                    CalFromNDCSpace_ViewPos_);
                  //  new Vector4(camerapos_w_.x, camerapos_w_.y, camerapos_w_.z, 1));

                CalFromNDCSpaceToWorldPos_ = Wfr_Math.Homogeneous_Division(CalFromNDCSpaceToWorldPos_W_);
            }
          
            
        }
 
        UpdateRectPoint();

        UpdateFrustum();
        

    }

    private void UpdateFrustum()
    {
        Vector3 n_LeftBottom_ = GetWorldPos(NearRect.LeftBottom);
    
        Vector3 n_LeftTop_ = GetWorldPos(NearRect.LeftTop);
       
        Vector3 n_RightBottom_ = GetWorldPos(NearRect.RightBottom);
      
        Vector3 n_RightTop_ = GetWorldPos(NearRect.RightTop);
        
        Vector3 F_LeftBottom_ = GetWorldPos(FarRect.LeftBottom);
     
        Vector3 F_LeftTop_ = GetWorldPos(FarRect.LeftTop);
        
        Vector3 F_RightBottom_ = GetWorldPos(FarRect.RightBottom);
       
        Vector3 F_RightTop_ = GetWorldPos(FarRect.RightTop);

        if (FrustumMatrix != null)
        {
            FrustumMatrix.UpdateNearPosition(n_LeftBottom_,n_LeftTop_,n_RightTop_,n_RightBottom_);
            
            FrustumMatrix.UpdateFarPosition(F_LeftBottom_,F_LeftTop_,F_RightTop_,F_RightBottom_);
        }
    }
    

    private void OnDrawGizmos()
    {

        DrawFrustum();

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(CalFromNDCSpaceToWorldPos_,1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(CalFromNDCSpaceToWorldPos_W_,1f);
         
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(CalFromNDCSpaceToWorldPos_Unity,1f);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(CalFromNDCSpaceToWorldPos_Unity_W,1f);
        
    }

    private void DrawFrustum()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawFrustum(transform.position,FiledOfView,FarZ,NearZ,Aspect_Width/Aspect_Height);
        Gizmos.color = Color.blue;
        DrawWorlRect(NearRect);
        DrawWorlRect(FarRect);
        
        Vector3 n_LeftBottom_ = GetWorldPos(NearRect.LeftBottom);
    
        Vector3 n_LeftTop_ = GetWorldPos(NearRect.LeftTop);
       
        Vector3 n_RightBottom_ = GetWorldPos(NearRect.RightBottom);
      
        Vector3 n_RightTop_ = GetWorldPos(NearRect.RightTop);
        
        Vector3 F_LeftBottom_ = GetWorldPos(FarRect.LeftBottom);
     
        Vector3 F_LeftTop_ = GetWorldPos(FarRect.LeftTop);
        
        Vector3 F_RightBottom_ = GetWorldPos(FarRect.RightBottom);
       
        Vector3 F_RightTop_ = GetWorldPos(FarRect.RightTop);
        
        
        Gizmos.DrawLine(n_LeftBottom_,F_LeftBottom_);
        
        Gizmos.DrawLine(n_RightBottom_,F_RightBottom_);
        
        Gizmos.DrawLine(n_LeftTop_,F_LeftTop_);
        
        Gizmos.DrawLine(n_RightTop_,F_RightTop_);
        
        Gizmos.color = Color.white;
        
        Gizmos.DrawSphere(n_LeftBottom_,SphereR);
        Gizmos.DrawSphere(n_LeftTop_,SphereR);
        Gizmos.DrawSphere(n_RightBottom_,SphereR);
        Gizmos.DrawSphere(n_RightTop_,SphereR);
        
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(F_LeftBottom_,SphereR);
        Gizmos.DrawSphere(F_LeftTop_,SphereR);
        Gizmos.DrawSphere(F_RightBottom_,SphereR);
        Gizmos.DrawSphere(F_RightTop_,SphereR);
    }

    private void DrawWorlRect(WorldRect rect_)
    {
        Vector3 LeftBottom_ = GetWorldPos(rect_.LeftBottom);
        
        Vector3 LeftTop_ = GetWorldPos(rect_.LeftTop);
        
        Vector3 RightBottom_ = GetWorldPos(rect_.RightBottom);
        
        Vector3 RightTop_ = GetWorldPos(rect_.RightTop);
        
        
        // Gizmos.DrawLine(rect_.LeftBottom,rect_.LeftTop);
        //
        // Gizmos.DrawLine(rect_.LeftBottom,rect_.RightBottom);
        //
        // Gizmos.DrawLine(rect_.RightTop,rect_.LeftTop);
        //
        // Gizmos.DrawLine(rect_.RightBottom,rect_.RightTop);
        
        Gizmos.DrawLine(LeftBottom_,LeftTop_);
        
        Gizmos.DrawLine(LeftBottom_,RightBottom_);
        
        Gizmos.DrawLine(RightTop_,LeftTop_);
        
        Gizmos.DrawLine(RightBottom_,RightTop_);
    }

    private Vector3 GetWorldPos(Vector3 modelpos_)
    {
        Vector3 rePoint_ =  Wfr_Math.TransPointByMatrix(_MatrixCom.MainMatrix, modelpos_); // 因为那边的坐标确实是基础坐标,旋转就是相机自身的旋转了.

        return rePoint_;
    }
    
    // 组合一个 Matrix 数据 
    public float[,] GetWorldToViewMatrix()
    {
        return _ViewMatrixCom.MainMatrix_Inverse;// 转到相机空间, 其实就是 相机模型空间到世界空间的逆矩阵 但坐标系是右手坐标系
    }
    
    public float[,] GetViewToWorldMatrix()
    {
        return _ViewMatrixCom.MainMatrix;// 转到相机空间, 其实就是 相机模型空间到世界空间的逆矩阵 但坐标系是右手坐标系
    }
    
    public float[,] GetViewToProjectMatrix()
    {
        return FrustumMatrix.FrustumMatrixl;// 转到相机空间, 其实就是 相机模型空间到世界空间的逆矩阵 但坐标系是右手坐标系
    }
    
    public float[,] GetProjectToViewMatrix()
    {
        return FrustumMatrix.FrustumMatrixl_Inverse;// 转到相机空间, 其实就是 相机模型空间到世界空间的逆矩阵 但坐标系是右手坐标系
    }
    

    public bool IsInCamera(Vector4 point_)
    {
        if (FrustumMatrix != null)
        {
            return FrustumMatrix.IsInCameraFrustum(point_);
        }   
        
        return false;
    }

    /// <summary>
    /// 根据传入的 相机空间的坐标值,将其转换为 裁剪空间坐标
    /// </summary>
    /// <param name="point_"></param>
    /// <returns></returns>
    public Vector4 GetClipSpacePos(Vector4 point_)
    {
        Vector4 rePoint_ =  Wfr_Math.TransPointByMatrix(FrustumMatrix.FrustumMatrixl, point_);
        return rePoint_;
    }
    
}
 