using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 提供一些 shader的操作方法 
/// </summary>
public class WfrShaderLib
{
    public static GPUFrameData ShaderData;
    

    public static Vector4 CalculateClipPos(Vector3 modelposition_,DrawCallData VertexdrwacallData_)
    {

        Vector4 worldpos_ =
            Wfr_Math.TransPointByMatrix_V3_2_V4(VertexdrwacallData_.ObjWorldMatrix, modelposition_);

        Vector4 viewpos_ = Wfr_Math.TransPointByMatrix(ShaderData.ViewMatrix, worldpos_);
        
        Vector4 clippos_ = Wfr_Math.TransPointByMatrix(ShaderData.ClipMatrix, viewpos_);
        
        return clippos_;
    }
    
    
}
