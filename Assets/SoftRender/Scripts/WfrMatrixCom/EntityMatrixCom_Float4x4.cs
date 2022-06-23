using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public partial class EntityMatrixComponent : MonoBehaviour
{
    [SerializeField]
    private float4x4 _Matrix_Main_4x4;
    [SerializeField]
    private float4x4 Matrix_Main_Inverse_4x4;
    
    private float4x4 Matrix_Main_Inverse_4x4_Cal;

    
    private float4x4 _Matrix_Main_4x4_NoShow;
    
    [SerializeField]
    private float4x4 Matrix_Main_Inverse_4x4_OnShowCal;
    /// <summary>
    /// 由4X4 计算出来的 逆矩阵 
    /// </summary>
    private float[,] _Float4x4Cal_MainMatrixInverse;
    
    /// <summary>
    /// 由float4x4 计算出来的矩阵 
    /// </summary>
    public float[,] MainMatrixInverse_FloatCal
    {
        get
        {
            return _Float4x4Cal_MainMatrixInverse;
        }
    }
    
    /// <summary>
    /// 因为 Unity Inspector 上 float4X4 呈现的 是将行竖着显示,因此 float4x4 只用于 显示 .实际使用时要转换一次
    /// </summary>
    /// <param name="Matrix_"></param>
    /// <returns></returns>
    public float[,] GetFloat2DArrary(float4x4 Matrix_)
    {
        float[,] reMatrix_ = new float[4, 4];

        for (int row = 0; row < 4; row++)
        {
            for (int col_ = 0; col_ < 4; col_++)
            {
                reMatrix_[row, col_] = Matrix_[col_][row];
            }
        }

        return reMatrix_;
    }
    
    public float[,] CopyInfoFromFloat4X4(float4x4 Matrix_)
    {
        float[,] reMatrix_ = new float[4, 4];

        for (int row = 0; row < 4; row++)
        {
            for (int col_ = 0; col_ < 4; col_++)
            {
                reMatrix_[row, col_] = Matrix_[row][col_];
            }
        }

        return reMatrix_;
    }

    private void CalFloat4X4MainMatrix_Inverse()
    { 
        Matrix_Main_Inverse_4x4 = Unity.Mathematics.math.inverse(_Matrix_Main_4x4);

        _Float4x4Cal_MainMatrixInverse = GetFloat2DArrary(Matrix_Main_Inverse_4x4);
        
        Matrix_Main_Inverse_4x4_OnShowCal  = Unity.Mathematics.math.inverse(_Matrix_Main_4x4_NoShow);

        _Float4x4Cal_MainMatrixInverse = CopyInfoFromFloat4X4(Matrix_Main_Inverse_4x4_OnShowCal);
    }
}
