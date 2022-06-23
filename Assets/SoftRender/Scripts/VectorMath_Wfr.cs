using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class VectorMath_Wfr : MonoBehaviour
{

    public Vector3 A = Vector3.forward;
    
    public Vector3 B = Vector3.left;
    
    public Vector3 _A = Vector3.zero;
    
    public Vector3 _B= Vector3.zero;

    public Vector3 NewX;

    public float Size = 10f;
    
    // Start is called before the first frame update
    void Start()
    {
        VectorCross(A,B);
    }

    private void Update()
    {
        VectorCross(A,B);
    }

    private Ray RayA_;
    private Ray RayB_;
    
    private Ray RayX_;
    public  void VectorCross(Vector3 A_,Vector3 B_)
    {
        if (_A == A_ && _B == B_)
        {
            return;
        }

        _A = A_;
        _B = B_;

        Vector3 zero_pos_ = transform.position;
        
        RayA_ = new Ray(zero_pos_, _A);
        
        RayB_ = new Ray(zero_pos_, _B);
        
 
        
        Vector3 NewX_ = new Vector3();

        for (int i = 0; i < 3; i++)
        {
            NewX_[i] = A_[(i + 1) % 3] * B_[(i + 2) % 3] - A_[(i + 2) % 3] * B_[(i + 1) % 3];
        }

        RayX_ = new Ray(zero_pos_, NewX);
        
        NewX = NewX_;
        Debug.Log($"A{A_} Cross B{B_} =  {NewX_}");
    }

    private void OnDrawGizmos()
    {
      
        
        Gizmos.color = Color.green;

       
        Gizmos.DrawRay(RayA_);
        
         
        Gizmos.DrawRay(RayB_);
        
        Gizmos.color = Color.blue;
        
        Gizmos.DrawRay(RayX_);

        Vector3 position_ = this.transform.position;

        Vector3 b_pos_ = position_ + B * Size;
        
        Vector3 a_pos_ = position_ + A * Size;
        
        Vector3 x_pos_ = position_ + NewX * Size;
        
        DrawRect(position_,b_pos_);
    }

    /// <summary>
    /// 如果画 正方形, 需要知道 他们的变化值在哪. 
    /// </summary>
    /// <param name="leftBootom"></param>
    /// <param name="RightTop"></param>
    public static void DrawRect(Vector3 leftBootom,Vector3 RightTop)
    {
        Vector3 leftTop_ = new Vector3(leftBootom.x,RightTop.y,RightTop.z);
        
        Vector3 RightNottom_ = new Vector3(RightTop.x,leftBootom.y,leftBootom.z);
        
        Gizmos.DrawLine(leftBootom,leftTop_);
        
        Gizmos.DrawLine(leftBootom,RightNottom_);
        
        Gizmos.DrawLine(RightTop,leftTop_);
        
        Gizmos.DrawLine(RightTop,RightNottom_);
    }


}
