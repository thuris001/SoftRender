using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EntityMatrixComponent : MonoBehaviour
{
    #region  旋转计算

    private float[,] _WorldMatrix_Rotation_Z = new float[4, 4];
    
    private float[,] _WorldMatrix_Rotation_X = new float[4, 4];
    
    private float[,] _WorldMatrix_Rotation_Y = new float[4, 4];
    
    private float[,] _WorldMatrix_Rotation_Z_Inverse = new float[4, 4];
    
    private float[,] _WorldMatrix_Rotation_X_Inverse = new float[4, 4];
    
    private float[,] _WorldMatrix_Rotation_Y_Inverse = new float[4, 4];
    
    public Vector2 GetEulerAngleCosSin(float rotation_)
    {
        float Rotation_angle_ = Wfr_Math.NumToAngleValue((rotation_ % 180));
        float cos_ = Mathf.Cos(Rotation_angle_);
        float sin_ = Mathf.Sin(Rotation_angle_);

        return new Vector2(cos_,sin_);
    }

      private bool CalculateRotation_Matrix(Vector3 NewAngle_,bool force_ = false)
    {
      
        // 世界变换旋转的 其实都是个体自己的旋转.. 缩放也是个体自己的缩放
    
        #region  用各个轴 sin cos 值 填充各个轴的旋转矩阵

        bool calcultaZ_ = NewAngle_.z != _LastRotation.z || force_;

        Vector2 calculateCosSin_;
        Vector2 calculateCosSin_inverse_;
        
        if (calcultaZ_)
        {
            calculateCosSin_ = GetEulerAngleCosSin(NewAngle_.z );
            
       
                
            float cos_z = calculateCosSin_.x;
            float sin_z = calculateCosSin_.y;
            
            _WorldMatrix_Rotation_Z = new float[,]
            {   {cos_z,-sin_z, 0,0},
                {sin_z, cos_z, 0,0},
                {0    ,     0, 1,0},
                {0    ,     0, 0,1},
            };
            
            calculateCosSin_inverse_ = GetEulerAngleCosSin(-NewAngle_.z );
            cos_z = calculateCosSin_inverse_.x;
            sin_z = calculateCosSin_inverse_.y;
            
            _WorldMatrix_Rotation_Z_Inverse = new float[,]
            {   {cos_z,-sin_z, 0,0},
                {sin_z, cos_z, 0,0},
                {0    ,     0, 1,0},
                {0    ,     0, 0,1},
            };
        }
   
        bool calcultaX_ = NewAngle_.x != _LastRotation.x || force_;

        if (calcultaX_)
        {
            calculateCosSin_ = GetEulerAngleCosSin(NewAngle_.x );
            float cos_x = calculateCosSin_.x;
            float sin_x = calculateCosSin_.y;
            
            _WorldMatrix_Rotation_X = new float[,]
            {   {1,0    ,  0   ,0},
                {0,cos_x,-sin_x,0},
                {0,sin_x,cos_x ,0},
                {0,    0,    0 ,1},
            };
            
            calculateCosSin_inverse_ = GetEulerAngleCosSin(-NewAngle_.x );
            cos_x = calculateCosSin_inverse_.x;
            sin_x = calculateCosSin_inverse_.y;
            
            _WorldMatrix_Rotation_X_Inverse = new float[,]
            {   {1,0    ,  0   ,0},
                {0,cos_x,-sin_x,0},
                {0,sin_x,cos_x ,0},
                {0,    0,    0 ,1},
            };
            
        }
        bool calcultaY_ = NewAngle_.y != _LastRotation.y || force_;

        if (calcultaY_)
        { 
            calculateCosSin_ = GetEulerAngleCosSin(NewAngle_.y );
            float cos_y = calculateCosSin_.x;
            float sin_y = calculateCosSin_.y;
            
            _WorldMatrix_Rotation_Y = new float[,]
            {
                { cos_y,   0, sin_y, 0 },
                { 0,       1,     0, 0 },
                { -sin_y,  0, cos_y, 0 },
                { 0,       0,     0, 1 },
            };
            calculateCosSin_inverse_ = GetEulerAngleCosSin(-NewAngle_.y );
            
            cos_y = calculateCosSin_inverse_.x;
            sin_y = calculateCosSin_inverse_.y;
            
            _WorldMatrix_Rotation_Y_Inverse = new float[,]
            {
                { cos_y,   0, sin_y, 0 },
                { 0,       1,     0, 0 },
                { -sin_y,  0, cos_y, 0 },
                { 0,       0,     0, 1 },
            };
        }

        #endregion

        bool reBoo = calcultaZ_ || calcultaX_ || calcultaY_; 
        return reBoo;
    }

    private float[,] MergeRotationMatrix()
    {
        // 按 精要中的提示, 矩阵变换是 zxy , 但实际执行 顺序应当是  y  x 先先执行,然后再执行 与 z的合并, 
        // 实际中 发现也确实如此 否则 按 zx 先结合, 再与y结合, 只适用于 单轴旋转,不能适配多轴同时旋转,    所以需要用 y x z的旋转顺序.
        
        #if true 
        float handlevalue_1 = 0;
        
        float[,] mul_matrix_m2_yx_ = new float[4, 4];
        for (int i = 0; i < 4; i++)
        {  
            for (int j = 0; j < 4; j++)
            {  
                handlevalue_1 = 0;
                for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的  q 作为驱动者 
                {
                    handlevalue_1 +=  _WorldMatrix_Rotation_Y[i, q] * _WorldMatrix_Rotation_X[q, j]  ;
                }

                mul_matrix_m2_yx_[i, j] = handlevalue_1;
            }
             
        }
        float[,] mul_matrix_m2_yxz_ = new float[4, 4];
        for (int i = 0; i < 4; i++)
        {  
            for (int j = 0; j < 4; j++)
            {
                handlevalue_1 = 0;
                for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的  q 作为驱动者 
                {
                    handlevalue_1 +=  mul_matrix_m2_yx_[i, q] * _WorldMatrix_Rotation_Z[q, j]  ;
                }

                mul_matrix_m2_yxz_[i, j] = handlevalue_1;
            }
             
        }

        return mul_matrix_m2_yxz_;
     
        #else

        #region Mode1  //  z x y 的方式 只适用于 同时只有单轴的旋转,
           

            float handlevalue_ = 0;
        
            float[,] mul_matrix_m2_zx_ = new float[4, 4];
            for (int i = 0; i < 4; i++)
            {  
                for (int j = 0; j < 4; j++)
                {  
                    handlevalue_ = 0;
                    for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的   l 作为驱动者 
                    {
                        handlevalue_ +=  _WorldMatrix_Rotation_Z[i, q] * _WorldMatrix_Rotation_X[q, j]  ;
                    }

                    mul_matrix_m2_zx_[i, j] = handlevalue_;
                }
                 
            }
            float[,] mul_matrix_m2_zxy_ = new float[4, 4];
            for (int i = 0; i < 4; i++)
            {  
                for (int j = 0; j < 4; j++)
                {
                    handlevalue_ = 0;
                    for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的   l 作为驱动者 
                    {
                        handlevalue_ +=  mul_matrix_m2_zx_[i, q] * _WorldMatrix_Rotation_Y[q, j]  ;
                    }

                    mul_matrix_m2_zxy_[i, j] = handlevalue_;
                }
                 
            }

            return mul_matrix_m2_zxy_;

        #endregion
        #endif
         
     
    }
    
    
     private float[,] MergeRotationMatrix(float[,] M_Y_,float[,] M_X_,float[,] M_Z_)
    {
        // 按 精要中的提示, 矩阵变换是 zxy , 但实际执行 顺序应当是  y  x 先先执行,然后再执行 与 z的合并, 
        // 实际中 发现也确实如此 否则 按 zx 先结合, 再与y结合, 只适用于 单轴旋转,不能适配多轴同时旋转,    所以需要用 y x z的旋转顺序.
         
        float handlevalue_1 = 0;
        
        float[,] mul_matrix_m2_yx_ = new float[4, 4];
        for (int i = 0; i < 4; i++)
        {  
            for (int j = 0; j < 4; j++)
            {  
                handlevalue_1 = 0;
                for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的  q 作为驱动者 
                {
                    handlevalue_1 +=  M_Y_[i, q] * M_X_[q, j]  ;
                }

                mul_matrix_m2_yx_[i, j] = handlevalue_1;
            }
             
        }
        float[,] mul_matrix_m2_yxz_ = new float[4, 4];
        for (int i = 0; i < 4; i++)
        {  
            for (int j = 0; j < 4; j++)
            {
                handlevalue_1 = 0;
                for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的  q 作为驱动者 
                {
                    handlevalue_1 +=  mul_matrix_m2_yx_[i, q] * M_Z_[q, j]  ;
                }

                mul_matrix_m2_yxz_[i, j] = handlevalue_1;
            }
             
        }

        return mul_matrix_m2_yxz_;
      
     
    }
    
    #endregion
}
