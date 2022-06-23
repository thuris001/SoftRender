using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 模拟shader 过程
/// </summary>
public class Wfr_ShaderSimulate : MonoBehaviour
{
    /// <summary>
    /// 到时应该扩充一个三角形. 或者立方体,让其进行渲染处理.
    /// 最后 以坐标形式,去映射到一个 屏幕范围上,作为软光栅. 但剔除那一段暂时还不好实现,需要重新关联出路.
    /// </summary>
    public VertexData MyVertex;
    
    public DrawCallData ObjData;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CpuSetData(DrawCallData ObjData_)
    {
        ObjData = ObjData_;
    }
    
    public struct Appdata
    {
        public Vector3 Position;
        
    }
    
    public struct V2S
    {
        public Vector3 ClipPosition;
    }
    // 理论上在 片段着色器阶段, 其收到的值,是clip 处理之后的值? 
    public V2S Vert(Appdata data_)
    {
        V2S o_ = new V2S();

        o_.ClipPosition = WfrShaderLib.CalculateClipPos(data_.Position,ObjData);

        return o_;
    }

    public Vector4 Frag(V2S input_)
    {
        return Vector4.zero;// 等于呈现黑色
    }

    public void GpuHandle()
    {
        Appdata data_ = new Appdata();
        data_.Position = MyVertex.position;

        V2S v_ = Vert(data_);
    }
}
[Serializable]
public class VertexData
{
    public Vector3 position;
}


public class DrawCallData
{
    public float[,] ObjWorldMatrix;  
}

public class GPUFrameData
{
    public float[,] ViewMatrix;
    
    public float[,] ClipMatrix; 
    
    public float[,] ViewMatrix_Inverse;
    
    public float[,] ClipMatrix_Inverse; 
}