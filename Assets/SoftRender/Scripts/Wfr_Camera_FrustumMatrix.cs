using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.Mathematics;
using UnityEngine;
/// <summary>
/// 相机视椎体裁剪变换
/// </summary>
public class Wfr_Camera_FrustumMatrix : MonoBehaviour
{
    // 得到裁剪矩阵 

    public float near_size;

    public float near_z;

    public float far_z;
    
    public float[,] FrustumMatrixl = new float[4,4];
    
    public float[,] FrustumMatrixl_Inverse = new float[4,4];

    public Wfr_Camera MainCamera;

    public float aspect;
    /// <summary>
    /// 在inspector 上显示比较好.
    /// Matrix4x4 在insptector 上显示不佳.显示成数组.
    /// </summary>
    [SerializeField]
    private float4x4 FrustumMatrix_4x4;
    [SerializeField]
    private float4x4 FrustumMatrix_4x4_Inverse;
    
    public float3x4 FrustumNearPostion;
    
    public float3x4 FrustumFarPostion;

    public Transform FrustumNearPoint;
    
    public Transform FrustumFarPoint;
    public void UpdateNearPosition(Vector3 p1,Vector3 p2,Vector3 p3,Vector3 p4)
    {
        FrustumNearPostion = new float3x4(p1, p2, p3, p4);

        if (FrustumNearPoint != null)
        {
            FrustumNearPoint.transform.position = p1;
        }
    
    }
    
    public void UpdateFarPosition(Vector3 p1,Vector3 p2,Vector3 p3,Vector3 p4)
    {
        FrustumFarPostion = new float3x4(p1, p2, p3, p4);

        if (FrustumFarPoint != null)
        {
            FrustumFarPoint.transform.position = p3;
        }
    }
    
    private void Awake()
    {
       
       
    }

    /// <summary>
    /// 视野. 就是 求tan 与 cos. 即近裁面的宽度.一半与 近裁面 与 相机零点距离, 进行计算得到
    ///  高宽比
    ///  近裁面 远裁面距离. 与近裁面 远裁面差.
    /// 相机的矩阵 只要 视野视椎体的固定值不变化, 那么不需要变化矩阵 
    /// </summary>
    public void CalculateMatrix()
    {
       // 三角函数 cot
   
       float cot_ = Wfr_Math.UnityLikePrecision_MatrixFloat(cot_value);

       float MV_1_ = cot_ / aspect;

       float MV_2_ = cot_;

       float MV_3_ = -((far_z + near_z) / (far_z - near_z));
       
       float MV_4_ = -2*((near_z * far_z) / (far_z - near_z));

       MV_1_= Wfr_Math.UnityLikePrecision_MatrixFloat(MV_1_ );
       MV_3_ = Wfr_Math.UnityLikePrecision_MatrixFloat(MV_3_ );
       MV_4_ = Wfr_Math.UnityLikePrecision_MatrixFloat(MV_4_  );

       FrustumMatrixl = new float[4, 4]
       {
           {MV_1_,0    ,0    ,0    },
           {0    ,MV_2_,0    ,0    },
           {0    ,0    ,MV_3_,MV_4_},
           {0    ,0    ,-1   ,0    } // 因为第四列的 第四行三列 为 -1 所以在与 x y z 1 矩阵相乘后  结果的 w 会为 -Z.  因为第四行 三列  会作用到z 上. 
       };

       for (int i = 0; i <4; i++)
       {
           for (int j = 0; j <4; j++)
           {
               FrustumMatrix_4x4[i][j] = FrustumMatrixl[j, i]; //  因float4x4 的inspector 呈现, 行被竖着显示, 所以这里互换一下.
               // 例子, 如果是 4x3 会发现  显示的inspector 一行只有3个, 一列反而有4个, 但存入的确实是一个float4. 
               
           } 
       }

       FrustumMatrix_4x4_Inverse = math.inverse(FrustumMatrix_4x4);
        
       for (int i = 0; i <4; i++)
       {
           for (int j = 0; j <4; j++)
           {
               FrustumMatrixl_Inverse[i,j] = Wfr_Math.UnityLikePrecision_MatrixFloat(FrustumMatrix_4x4_Inverse[j][i]); //  因float4x4 的inspector 呈现, 行被竖着显示, 所以这里互换一下.
               // 例子, 如果是 4x3 会发现  显示的inspector 一行只有3个, 一列反而有4个, 但存入的确实是一个float4. 
               
           } 
       }
    }
    
    float cot_value ;
    private float FovAngleValue;
    
    public void UpdateByCamera(Wfr_Camera MainCamera_ ,bool force_ = false)
    {
        
        MainCamera = MainCamera_; 
        bool ischange_ = force_;

        ischange_ = ischange_ ||(FovAngleValue != MainCamera_.GetFovAngle() || near_z != MainCamera.NearZ 
                                                                              || far_z != MainCamera.FarZ||
                                                                              aspect != (MainCamera.Aspect_Height / MainCamera.Aspect_Width));
        FovAngleValue = MainCamera_.GetFovAngle();
        float fovangle_ = FovAngleValue / 2f;
         
        cot_value = 1/math.tan(fovangle_);
    
        near_size = MainCamera.ViewSizeHeight / 2;// 一半的viewsize. 作为 视椎体的参数. near_size
        near_z = MainCamera.NearZ;
        far_z = MainCamera.FarZ;

        aspect = MainCamera.Aspect_Width / MainCamera.Aspect_Height;

        if (ischange_)
        {
            CalculateMatrix();
        }
    
    }

    /// <summary>
    /// 根据传递进入的 裁剪空间坐标,判断其是否为 在视椎体之中.
    /// </summary>
    /// <param name="frustumpoint_"></param>
    /// <returns></returns>
    public static bool IsInFrustum(Vector4 frustumpoint_)
    {
        bool reBoo_ = true; 

        for (int i = 0; i < 3; i++)
        {
            reBoo_ = reBoo_ &&(  -frustumpoint_[3] <= frustumpoint_[i] && frustumpoint_[i] <= frustumpoint_[3]); // 三个坐标系都必须小于此值 
        }

        return reBoo_;
    }
    
    public bool IsInCameraFrustum(Vector4 point_)
    {
        Vector4 frustumpoint_ = Wfr_Math.TransPointByMatrix(FrustumMatrixl, point_);

        bool reBoo_ = true; 

        for (int i = 0; i < 3; i++)
        {
            reBoo_ = reBoo_ &&(  -frustumpoint_[3] <= frustumpoint_[i] && frustumpoint_[i] <= frustumpoint_[3]); // 三个坐标系都必须小于此值 
        }

        return reBoo_;
    }
    
    
    public bool IsInCameraFrustum(Vector3 point_)
    {
        Vector4 frustumpoint_ = Wfr_Math.TransPointByMatrix_V3_2_V4(FrustumMatrixl, point_);

        bool reBoo_ = true; 

        for (int i = 0; i < 3; i++)
        {
            reBoo_ = reBoo_ &&(  -frustumpoint_[3] <= frustumpoint_[i] && frustumpoint_[i] <= frustumpoint_[3]); // 三个坐标系都必须小于此值 
        }

        return reBoo_;
    }
    
    
}
