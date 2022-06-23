using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Wfr_Math  
{
    /// <summary>
    /// 将 XX度转换为 三角函数公式计算时需要输入的值
    /// 比如 30度 转换为
    /// PI 精度可以自己优化
    /// 之后再复刻一下 三角形插值的算法. 
    /// </summary>
    public static float NumToAngleValue(float value_)
    {
        // Mathf.Deg2Rad 其实unity 有这个算法 应该是pi/180f 的常量值.
        
        float anglevalue_ = value_ * (Mathf.PI / 180f);

        return anglevalue_;
    }
 
    public static float GetNumCot(float value_)
    {
        float anglevalue_ = NumToAngleValue(value_);

        float cot_ = 1/Mathf.Tan(anglevalue_);
        
        return cot_;
    }
    
    
    /// <summary>
    /// 保留 float 指定位数的小数点.
    /// </summary>
    /// <param name="value_"></param>
    /// <param name="PrecisionValue"></param>
    /// <returns></returns>
    public static float Keep_PrecisionPoint(float value_,int PrecisionValue = 0)
    {
        float reFloat_ = value_;

        float scale_ = 1;
        for (int i = 0; i < PrecisionValue; i++)
        {
            scale_ *= 10;
        }
        
        Int32 scaleend_ = (Int32)(System.Math.Round(value_ * scale_));

        reFloat_ = scaleend_ / scale_;
        
        return reFloat_;
    }

    public static float UnityLikePrecision_MatrixFloat(float value_)
    {
        return Keep_PrecisionPoint(value_,4);
    }
    
    
       public static Vector4 TransPointByMatrix(float[,] Matrix_,Vector3 point)
    { 
        float[] startPoint_  = new float[4]{point.x ,point.y  ,point.z ,1};
        Vector4 rePoint_ = new Vector4();
        for (int i = 0; i < 4; i++)
        {
            float handlevalue_ =0;
            for (int j = 0; j < 4; j++)
            {
                handlevalue_ +=   startPoint_[j] * Matrix_[i, j];
            }
            handlevalue_ = Wfr_Math.UnityLikePrecision_MatrixFloat(handlevalue_);
            rePoint_[i] = handlevalue_;// 做一次数据累加
        }
         
        return rePoint_;
        
    }
    
    public static Vector4 TransPointByMatrix(float[,] Matrix_,Vector4 point_)
    {
   
        Vector4 rePoint_ = new Vector4(); 
        for (int i = 0; i < 4; i++)
        {
            float handlevalue_ =0;
            for (int j = 0; j < 4; j++)
            {
                handlevalue_ +=   point_[j] * Matrix_[i, j];
            }
            handlevalue_ = Wfr_Math.UnityLikePrecision_MatrixFloat(handlevalue_);
            rePoint_[i] = handlevalue_;// 做一次数据累加
        } 
        
        return rePoint_;
        
    }
    
    public static Vector4 TransPointByMatrix_V3_2_V4(float[,] Matrix_,Vector3 point_)
    {
        Vector4 point_v4_ = new Vector4(point_.x, point_.y, point_.z, 1);
        Vector4 rePoint_ = new Vector4(); 
        for (int i = 0; i < 4; i++)
        {
            float handlevalue_ =0;
            for (int j = 0; j < 4; j++)
            {
                handlevalue_ +=   point_v4_[j] * Matrix_[i, j];
            }
            handlevalue_ = Wfr_Math.UnityLikePrecision_MatrixFloat(handlevalue_);
            rePoint_[i] = handlevalue_;// 做一次数据累加
        } 
        
        return rePoint_;
        
    }

    public static Vector4 Homogeneous_Division(Vector4 clippos_)
    {
        Vector4 reV3_ = Vector4.zero;

        for (int i = 0; i < 3; i++)
        {
            reV3_[i] = clippos_[i] / clippos_.w; // w的值要留着..
        }

        reV3_.w = clippos_.w;// w 不能除.不然 还原要走专用算法
        return reV3_;
    }
    
    public static Vector4 Homogeneous_Division_Inverse(Vector4 ndcpos_)
    {
        Vector4 reV3_ = Vector3.zero;

        for (int i = 0; i < 3; i++)
        {
            reV3_[i] = ndcpos_[i] * ndcpos_.w; // 但符号肯定无了吧?
        }

        reV3_.w = ndcpos_.w;
        
        return reV3_;
    }
    

    public static Vector4 GetScreenPos(Vector4 clippos_,Vector2 screenPixel_)
    {
        Vector4 reV3_ = Vector4.zero;

        reV3_.x = clippos_.x * screenPixel_.x / (2 * clippos_.w) + screenPixel_.x / 2;
        
        reV3_.y = clippos_.y * screenPixel_.y / (2 * clippos_.w) + screenPixel_.y / 2;

        reV3_.z = clippos_.z / clippos_.w;
        
        return reV3_;
    }
}

public struct WorldRect
{
  
    private Vector3 _CenterPos;

    private Vector2 _RectSize;

    public Vector3 CenterPos
    {
        get
        {
            return _CenterPos;
        }
    }
    
    public Vector3 RectSize
    {
        get
        {
            return _RectSize;
        }
    } 
 
    public Vector3 LeftBottom;
    
    public Vector3 RightTop;
    
    public Vector3 LeftTop;
    
    public Vector3 RightBottom;
    
    public WorldRect(Vector3 CenterPos_,Vector2 RectSize_)
    {  
        _CenterPos = CenterPos_;
        _RectSize = RectSize_; 
        LeftBottom =   RightTop =   LeftTop =   RightBottom = Vector3.zero;
        Update(_CenterPos,_RectSize);
    }

    public void Update(Vector3 CenterPos_,Vector2 RectSize_)
    {
        _CenterPos = CenterPos_;
        _RectSize = RectSize_;

        // LeftBottom = new Vector3(CenterPos.x - RectSize.x, CenterPos.y - RectSize.y, CenterPos.z);
        // RightTop = new Vector3(CenterPos.x + RectSize.x, CenterPos.y + RectSize.y, CenterPos.z);
        // LeftTop = new Vector3(CenterPos.x - RectSize.x, CenterPos.y + RectSize.y, CenterPos.z);
        // RightBottom = new Vector3(CenterPos.x + RectSize.x, CenterPos.y - RectSize.y, CenterPos.z);
        
        // 让其处于 子坐标系
        LeftBottom = new Vector3( - RectSize.x,   - RectSize.y, CenterPos.z);
        RightTop = new Vector3(  RectSize.x,   RectSize.y, CenterPos.z);
        LeftTop = new Vector3( - RectSize.x,   RectSize.y, CenterPos.z);
        RightBottom = new Vector3(  RectSize.x,  - RectSize.y, CenterPos.z);
    }
    
  
    public Vector3 GetLeftBottom()
    {
        return new Vector3(CenterPos.x - RectSize.x, RectSize.y - RectSize.y, CenterPos.z);
    }
    
    public Vector3 GetRightTop()
    {
        return new Vector3(CenterPos.x + RectSize.x, RectSize.y + RectSize.y, CenterPos.z);
    }
    
    public Vector3 GetLeftTop()
    {
        return new Vector3(CenterPos.x - RectSize.x, RectSize.y + RectSize.y, CenterPos.z);
    }
    
    public Vector3 GetRightBottom()
    {
        return new Vector3(CenterPos.x + RectSize.x, RectSize.y - RectSize.y, CenterPos.z);
    }
    
}