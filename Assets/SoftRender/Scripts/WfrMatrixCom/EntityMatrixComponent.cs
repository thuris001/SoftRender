using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public partial class EntityMatrixComponent : MonoBehaviour
{
    /// <summary>
    /// 是否为右手坐标系
    /// </summary>
    public bool isRightCoordinate = false;
    
    private   float[,] _Matrix_RightCoordinate  = new float[4, 4]
    {   {1 ,0,0,0},
        {0,1 ,0,0},
        {0,0,-1,0},
        {0,0, 0, 1},
    };
    /// <summary>
    /// 本单位的 位移矩阵 
    /// </summary>
    private   float[,] _Matrix_Trans = new float[4, 4];
    /// <summary>
    /// 本单位的 大小矩阵 
    /// </summary>
    private   float[,] _Matrix_Scale = new float[4, 4];
    /// <summary>
    /// 本单位的 旋转矩阵 
    /// </summary>
    private   float[,] _Matrix_Rotation = new float[4, 4];
    /// <summary>
    /// 综合矩阵, 可进行 大小,旋转 平移的变换.
    /// </summary>
    private   float[,] _Matrix_Main = new float[4, 4];
    
    /// <summary>
    /// 主矩阵的逆置矩阵 用于逆转换.  比如对于相机来说,其实是把物体坐标转换到它的局部坐标?
    /// 逆矩阵应该是  把主矩阵反过来一遍吧?  
    /// </summary>
    private   float[,] _Matrix_Main_Inverse = new float[4, 4];

    /// <summary>
    /// 本单位的 位移矩阵 
    /// </summary>
    private   float[,] _Matrix_Trans_Inverse = new float[4, 4];
    /// <summary>
    /// 本单位的 大小矩阵 
    /// </summary>
    private   float[,] _Matrix_Scale_Inverse  = new float[4, 4];
    /// <summary>
    /// 本单位的 旋转矩阵 
    /// </summary>
    private   float[,] _Matrix_Rotation_Inverse  = new float[4, 4];
    
     
    public float[,] MainMatrix
    {
        get
        {
            return _Matrix_Main;
        }
    }
    
    public float[,] MainMatrix_Inverse
    {
        get
        {
            return _Matrix_Main_Inverse;
        }
    }
    
    
    //记录上次的值, 用于减少计算
    private Vector3 _LastPosition;
    
    private Vector3 _LastRotation;
    
    private Vector3 _LastScale;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
 

    public void UpdateMatrix(Transform transform_, bool force_ = false)
    {
        UpdateMatrix(transform_.position,
            transform_.localScale,
            transform_.rotation.eulerAngles,
            force_);
    }
    
    public void UpdateMatrix(Vector3 position_,Vector3 scale_,Vector3 rotation_,bool force_ = false)
    { 

        bool tran_change_ = _LastPosition != position_ || force_;
        if (tran_change_)
        {
            // 平移矩阵
            CalculateTrans_Matrix(position_);
            
            _LastPosition = position_;
        }
  
        bool scale_change_ = _LastScale != scale_|| force_;
        if (scale_change_)
        {
            // 缩放矩阵
            CalculateScale_Matrix(scale_);
            
            _LastScale = scale_;

        }

        bool rotation_change_ = _LastRotation != rotation_|| force_;
        if (rotation_change_)
        { 
            bool  calboo_  = CalculateRotation_Matrix(rotation_, force_);
            if (calboo_)
            {
                _Matrix_Rotation = MergeRotationMatrix(_WorldMatrix_Rotation_Y,_WorldMatrix_Rotation_X,_WorldMatrix_Rotation_Z);    MergeRotationMatrix();
                
                _Matrix_Rotation_Inverse = MergeRotationMatrix(_WorldMatrix_Rotation_Y_Inverse,_WorldMatrix_Rotation_X_Inverse,_WorldMatrix_Rotation_Z_Inverse);    MergeRotationMatrix();
            }
            
            _LastRotation = rotation_;
        }
  
        if (rotation_change_ || scale_change_ || tran_change_)
        {
            CalMainMatrix();
            CalMainMatrixInverse();
            CalFloat4X4MainMatrix_Inverse();
        }
          
    }
    
    /// <summary>
    /// 计算主矩阵, 本单元坐标系下的物体 变换到 本单元所在坐标系(0点坐标系下的 坐标)
    /// 整体从左到右 顺序是准确的 位移 x 角度 X大小.   这个顺序.   如果有右坐标系修正 则右坐标系修正  要最先处理. 
    /// </summary>
    private void CalMainMatrix()
    {// 按精要的 逻辑, 是先进行scale 变换 再进行 rotation 变换, 再进行 位移 trans 变换,  
        //  i是 行  j 是列.  A X B 即 A矩阵的行 需要 与 B矩阵的列相同, 才能  A X B  . 前矩阵是 从一行逐个取数据 , 后矩阵是从一列 逐个取数据 
        
        // 所以 前矩阵 要从 i 开启驱动, 而后矩阵 从 列数据驱动 
        float handlevalue_ = 0;
        float[,] mul_matrix_ = new float[4, 4];
        for (int i_row_ = 0; i_row_ < 4; i_row_++)
        {  
            for (int j_col_ = 0; j_col_ < 4; j_col_++)
            {  
                handlevalue_ = 0;
                for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的   q 作为驱动者 
                {
                    handlevalue_ +=   _Matrix_Rotation[i_row_, q] *_Matrix_Scale[q, j_col_]   ; // 矩阵还在右, 但顺序是 先进行右边的矩阵乘法. 矩阵计算必须这样处理,  即 i q 对 rotation, q j 对 scale 
                    //  
                }

                mul_matrix_[i_row_, j_col_] = handlevalue_;
            }
             
        }
       float[,] Matrix_temp_ = new float[4, 4]; 
       _Matrix_Main_4x4 = new float4x4();
       _Matrix_Main_4x4_NoShow = new float4x4();
        for (int i = 0; i < 4; i++)
        {  
            for (int j = 0; j < 4; j++)
            {  
                handlevalue_ = 0;
                for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的   q 作为驱动者 
                {
                    handlevalue_ +=  _Matrix_Trans[i, q] * mul_matrix_[q, j]  ;
                }

                handlevalue_ = Wfr_Math.UnityLikePrecision_MatrixFloat(handlevalue_);
                
                Matrix_temp_[i, j] = handlevalue_;
                _Matrix_Main_4x4[j][i] = handlevalue_; // 因为float4x4 的显示排列呈现  是将float4 竖着显示的. 所以把i与j 互换一下.
                _Matrix_Main_4x4_NoShow[i][j] = handlevalue_;
            } 
        }

       

        if (isRightCoordinate)
        {
            _Matrix_Main = new float[4, 4];
            _Matrix_Main_4x4 = new float4x4();
            _Matrix_Main_4x4_NoShow = new float4x4();
            for (int i = 0; i < 4; i++)
            {  
                for (int j = 0; j < 4; j++)
                {  
                    handlevalue_ = 0;
                    for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的   q 作为驱动者 
                    {
                        handlevalue_ +=    Matrix_temp_[i, q]  *  _Matrix_RightCoordinate[q, j]  ; // 正矩阵是 右坐标系位于右,  而逆矩阵 则右矩阵是位于 左
                    }
                    handlevalue_ = Wfr_Math.UnityLikePrecision_MatrixFloat(handlevalue_);
                    _Matrix_Main[i, j] = handlevalue_;
                    _Matrix_Main_4x4[j][i] = handlevalue_;
                    _Matrix_Main_4x4_NoShow[i][j] = handlevalue_;
                } 
            }

            Matrix_temp_[1, 1] = Matrix_temp_[1,1];
        }
        else
        { 
            _Matrix_Main = Matrix_temp_;
        }

 

    }
    
    
    /// <summary>
    /// 计算逆矩阵,让外部物体,到自己的坐标系下 将自己的偏移逆转回其他地方,
    /// 逆矩阵的顺序. 大小 X 旋转 X 位移.这样的顺序.   最终组合结果,因为满足结合律,所以可以先算 大小与旋转, 再与 位移计算  而右坐标系修正因为在正计算时 是最先处理的,所以这里要最后处理,即放在左侧.
    /// </summary>
    private void CalMainMatrixInverse()
    {// 精要 上的方法, 是 逆着来, 但其实按矩阵聚合的思路, 继续从scale 组合 rotation 再组合 trans的逻辑肯定是 适配所有的.  因为这里的计算是每个矩阵数值都是按相反变化过去的,使被变换的数据变换到子空间,
        // 如原本是旋转30度, 则逆矩阵的旋转矩阵 就是 旋转-30度.  移动也都是与正矩阵相加为0的. 
        float handlevalue_ = 0;
        float[,] mul_matrix_ = new float[4, 4];
        for (int i = 0; i < 4; i++)
        {  
            for (int j = 0; j < 4; j++)
            {  
                handlevalue_ = 0;
                for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的   q 作为驱动者 
                {
                    handlevalue_ +=  _Matrix_Scale_Inverse[i, q] * _Matrix_Rotation_Inverse[q, j]  ; // 矩阵还在右, 但顺序是 先进行右边的矩阵乘法.
                }

                mul_matrix_[i, j] = handlevalue_;
            }
             
        }
        float[,] Matrix_temp_ = new float[4, 4];  
        for (int i = 0; i < 4; i++)
        {  
            
            for (int j = 0; j < 4; j++)
            {  
                handlevalue_ = 0;
                for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的   q 作为驱动者 
                {
                    handlevalue_ +=   mul_matrix_[i, q] * _Matrix_Trans_Inverse[q, j] ;
                }

                handlevalue_ = Wfr_Math.UnityLikePrecision_MatrixFloat(handlevalue_);
                Matrix_temp_[i, j] = handlevalue_; 
            } 
        }
        
        if (isRightCoordinate)
        {
            _Matrix_Main_Inverse = new float[4, 4]; 
            for (int i = 0; i < 4; i++)
            {  
                for (int j = 0; j < 4; j++)
                {  
                    handlevalue_ = 0;
                    for (int q = 0; q < 4; q++) // i 为 行 乘 的    j 为 列 乘的   q 作为驱动者 
                    {
                        handlevalue_ +=  _Matrix_RightCoordinate[i, q] * Matrix_temp_[q, j]  ;
                    }
                    handlevalue_ = Wfr_Math.UnityLikePrecision_MatrixFloat(handlevalue_);
                    _Matrix_Main_Inverse[i, j] = handlevalue_;
                   
                } 
            }
        }
        else
        { 
            _Matrix_Main_Inverse = Matrix_temp_; 
        }
        
    }
    
    
    /// <summary>
    /// 计算平移的矩阵 
    /// </summary>
    private void CalculateTrans_Matrix(Vector3 trans_ )
    { 
        // 平移矩阵
        _Matrix_Trans = new float[,]
        {   {1,0,0,trans_.x},
            {0,1,0,trans_.y},
            {0,0,1,trans_.z},
            {0,0,0,1},
        };
        
        _Matrix_Trans_Inverse= new float[,]
        {   {1,0,0,-trans_.x},
            {0,1,0,-trans_.y},
            {0,0,1,-trans_.z},
            {0,0,0,1},
        };
    }
    
    // 缩放矩阵
    private void CalculateScale_Matrix(Vector3 Scale_ )
    { 
        _Matrix_Scale = new float[,]
        {   {Scale_.x,0,0,0},
            {0,Scale_.y,0,0},
            {0,0,Scale_.z,0},
            {0,0,0,1},
        };
        
        _Matrix_Scale_Inverse = new float[,]
        {   {1/Scale_.x,0,0,0},
            {0,1/Scale_.y,0,0},
            {0,0,1/Scale_.z,0},
            {0,0,0,1},
        };
    }
    
     
     
 
}
