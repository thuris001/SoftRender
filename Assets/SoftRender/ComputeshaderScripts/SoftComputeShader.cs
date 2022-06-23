using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// computeshader 的逻辑执行
/// 参考 https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/sm5-attributes-numthreads
/// 结论
/// 1 Computeshader  CPU端分配了 调用dispacth 传入并行组数量 的 Computeshader 实例 并为这些实例分配了索引 用于划分处理数据区间
/// 2 Computeshader  内部分配了  Computeshader numthreads 个 执行主函数  并为执行主函数划分的处理数据区间
/// 
/// </summary>
public class SoftComputeShader : MonoBehaviour
{
    
    [Header("Dispacthh Computeshader 时传入的数 (调用此CS线程组的次数)")]
    public Vector3Int DispatchCSRunNum= new Vector3Int(2,1,1);     // 因 当前 移动端 基本只能支持 shader 4.X 所以 Z 的值 为1 

    
    
    [Header("Computeshader 线程分布 (代表此CS内部执行的状态) ")]
    public Vector3Int ComputeShaderNumThread = new Vector3Int(16,16,1);     // 因 当前 移动端 基本只能支持 shader 4.X 所以 Z 的值 为1 
    
  
    // 对于一定数量的处理数据, computeshader 执行组 执行线程能正好容纳 computebuffer 或 rendertexture 传递的数据. 此时是刚刚好的.
    // 如果执行线程 与 线程组 合起来未能覆盖 执行数据,那么执行不完全 
    // 如果执行线程 与 线程组 合起来超过了之星数据, 那么执行有浪费
    
    // Start is called before the first frame update
    void Start()
    {
        // int runnum_ = ComputeShaderRunGroupNum.x * ComputeShaderRunThread.x
        //                                          * ComputeShaderRunGroupNum.y * ComputeShaderRunThread.y
        //                                          * ComputeShaderRunGroupNum.z * ComputeShaderRunThread.z;
 
       
    }

    [ContextMenu("RunSoftComputeShader")]
    private void InitAndRunOnceTime()
    {
        InitCSInfo();
 
        DispatchCS(DispatchCSRunNum.x,DispatchCSRunNum.y,DispatchCSRunNum.z);
         
      
    }
    
    private void InitCSInfo()
    {

        #region 创建一个恰好等于执行次数的数值. 用于验证线性执行是否正确

        int allZ_ = DispatchCSRunNum.z * ComputeShaderNumThread.z;
        int allY_ = DispatchCSRunNum.y * ComputeShaderNumThread.y;
        int allX_ = DispatchCSRunNum.x * ComputeShaderNumThread.x;

        int runnum_ = allX_ * allY_ * allZ_; 
        
        computebuffer = new Vector4[runnum_];
        int index_ = 0;
        for (int z_ = 0; z_ < allZ_; ++z_)
        {  
            for (int y_ = 0; y_ < allY_; ++y_)
            { 
                
                for (int x_ = 0; x_ < allX_; ++x_)
                { 
                    computebuffer[index_] = new Vector4(index_,x_,y_,z_);
                    ++index_;
                }
            } 
            
        } 

        #endregion
        
       
    }
    
    

    // Update is called once per frame
    void Update()
    {
       // DispatchCS(DispatchCSRunGroupNum.x,DispatchCSRunGroupNum.y,DispatchCSRunGroupNum.z);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#endif
    }

    /// <summary>
    /// 此段等价于  ComputeShader cs_;
    ///  cs_.Dispatch(DispatchCSRunGroupNum.x,DispatchCSRunGroupNum.y,DispatchCSRunGroupNum.z);
    /// 为方便理解, CSharp 代表调用的 dispatach 定义为CPU的调用数目 
    /// </summary>
    /// <param name="XMax_"></param>
    /// <param name="YMax_"></param>
    /// <param name="ZMax_"></param>
    public void DispatchCS(int XMax_, int YMax_,int ZMax_)
    {
        Vector3Int instanceGroupID_ =  Vector3Int.zero;
        Vector3Int parallelRunInfo_  = new Vector3Int(XMax_,YMax_,ZMax_);

        int CSNum_ = XMax_ * YMax_ * ZMax_;
        int CSMainFuncNum_ = ComputeShaderNumThread.x * ComputeShaderNumThread.y * ComputeShaderNumThread.z;
        // 本次执行 将并行处理 {} 个Computeshader. 每个Computeshader 并行执行 {}个 MainFunc;
        
        Debug.LogError($"本次执行  将并行处理 {CSNum_} 个Computeshader. 每个Computeshader 并行执行 {CSMainFuncNum_}个 MainFunc  " +
                       $"一次Dispatch 同时并行 {CSNum_*CSMainFuncNum_} 个运行 " );
        
        for (int z_ = 0; z_ < ZMax_; z_++)
        {
            instanceGroupID_.z = z_;
             
            for (int y_ = 0; y_ < YMax_; y_++)
            { 
                instanceGroupID_.y = y_;
                
                for (int x_ = 0; x_ < XMax_; x_++)
                {
                    instanceGroupID_.x = x_;

                    _Computeshader_Instance = new ComputeShader_Soft(ComputeShaderNumThread); // 实际情况,computeshader是并行执行这一段 
                    _Computeshader_Instance.computebuffer = computebuffer;
                    _Computeshader_Instance.texture = texture;
                    _Computeshader_Instance.OnDraw = DrawMesh;
                    _Computeshader_Instance.CS_InstanceNum = parallelRunInfo_ ;
                    _Computeshader_Instance.Run(instanceGroupID_.x,instanceGroupID_.y,instanceGroupID_.z);
                }
            } 
            
        }  
    }

 
    
    public Vector4[] computebuffer;
    
    public Vector2[][] texture  ;


    private ComputeShader_Soft _Computeshader_Instance;


    public Mesh DrawMeshs;

    public Material mat;
    
    public void DrawMesh(Vector4 pos_)
    {
        Matrix4x4 matrix_ = Matrix4x4.identity;

        matrix_.m03 = pos_.y;
        matrix_.m13 = pos_.z;
        matrix_.m23 = pos_.w;
        
        Graphics.DrawMesh(DrawMeshs,matrix_,mat,0,Camera.main,0);
    }
}

/// <summary>
/// 
/// Computeshader 的 理解上有两层
/// Computeshader 内部的 并行执行数量, 可视为分配了numthreads个数量的执行函数 由其并行执行.
/// </summary>
public class ComputeShader_Soft
{
    public Vector3Int numthreads = new Vector3Int(16, 16, 1); // 因 当前 移动端 基本只能支持 shader 4.X 所以 Z 的值 为1 
    /// <summary>
    /// 本线程组的 线程组ID; 代表本CS实例在线程组中的索引    此v3数据的  x y z 的计量值需要通过 CS_InstanceNum 对应值来确认. 
    /// </summary>
    public Vector3Int SV_GroupID;
    
    public Vector4[] computebuffer;
    
    public Vector2[][] texture  ;

    public Action<Vector4> OnDraw;
    /// <summary>
    /// 创建CS的实例数量
    /// 可认为CPU创建的CS线程组的最大容量 
    /// </summary>
    public Vector3Int CS_InstanceNum;

    /// <summary>
    /// 一个CS并行主执行函数 数量   可视为CS负责处理的区块范围大小 
    /// </summary>
    private int _per_cs_allThreadNum;
    
    
    /// <summary>
    /// 本CS 实例 处理数据的开始索引, 
    /// </summary>
    private int _CS_HandleData_StartIndex;
    
    public ComputeShader_Soft(Vector3Int numthreads_)
    {
        numthreads = numthreads_;
        
        _per_cs_allThreadNum = numthreads.z * numthreads.y * numthreads.x; // 乘以 每个CS 负责处理的区块范围大小  最终得到结果
    }

 
    
    /// <summary>
    /// GX GY GZ 代表第几组 Computeshader 执行.
    /// </summary>
    /// <param name="Gx_"></param>
    /// <param name="Gy_"></param>
    /// <param name="Gz_"></param>
    public void Run(int Gx_, int Gy_,int Gz_)
    {
        SV_GroupID = new Vector3Int(Gx_, Gy_, Gz_);
        
        Vector3Int SV_GroupThreadID_ = Vector3Int.zero;
        
        Vector3Int SV_DispatchThreadID_ = Vector3Int.zero; 
        
        ///  本 CS   已执行了子线程的索引起始值.  
        Vector3Int DispatchThreadID_Base =  new Vector3Int(SV_GroupID.x * numthreads.x,
            SV_GroupID.y * numthreads.y,
            SV_GroupID.z * numthreads.z);

        //  本 CS实例的索引值,  三维数组计算 转换 为线性列表 
        int cs_instance_index_ =
            SV_GroupID.z *  CS_InstanceNum.x * CS_InstanceNum.y +
            SV_GroupID.y * CS_InstanceNum.x +   
            SV_GroupID.x  ;
        // 以上 为已执行了多少线程组 (或本理解为CS的线程组ID)
         
        
        _CS_HandleData_StartIndex  = cs_instance_index_ * _per_cs_allThreadNum; //  可得到本次处理数据的开始索引

        int SV_GroupIndex_ = 0;// 当前线程组  已执行线程的数量 (组内数量)
        
       
        
        for (int z = 0; z < numthreads.z; ++z)
        {
            SV_GroupThreadID_.z = z;
            
        
            for (int y_ = 0; y_ < numthreads.y; ++y_)
            { 
                SV_GroupThreadID_.y = y_;
                
                for (int x_ = 0; x_ < numthreads.x; ++x_)
                {
                    SV_GroupThreadID_.x = x_;

                    // 三维计数器 的计算结果 等价于 不断累计计数
                    SV_GroupIndex_ = SV_GroupThreadID_.z * numthreads.x * numthreads.y
                                     + SV_GroupThreadID_.y * numthreads.x
                                     + SV_GroupThreadID_.x;
                 
                    
                    // 当前要执行线程的索引.
                    SV_DispatchThreadID_ = DispatchThreadID_Base + SV_GroupThreadID_; 
                    
                    ComputeShaderKernel_MainFunc(SV_DispatchThreadID_,SV_GroupID,SV_GroupIndex_);
                }
            } 
            
        }   
    }


    public void ComputeShaderKernel_MainFunc(Vector3Int DispatchThreadID_,Vector3Int GroupID_,int GroupIndex_)
    {
        int  realindex_ =  DispatchThreadID_.z *  numthreads.x * numthreads.y * (GroupID_.x * GroupID_.y) +
                DispatchThreadID_.y * numthreads.x  + CS_InstanceNum.x
                         + DispatchThreadID_.x ;
        
        // Computeshader 中可使用这种方式. 
        int realindex_m1_ =  ((GroupID_.z *  CS_InstanceNum.x * CS_InstanceNum.y)+ (  GroupID_.y *  CS_InstanceNum.x) +
                             GroupID_.x) * _per_cs_allThreadNum +  
                             GroupIndex_ ;
        
        int realindex_m2_ = GroupIndex_ + _CS_HandleData_StartIndex;// 之前cs 调用过次数 加上本次 cs main之前调用次数,得到当前执行次数
        // 所以使用这个数值可以得到准确的已执行.
 
        if (OnDraw != null)
        {
            OnDraw(computebuffer[realindex_m2_]);
        }
        Debug.LogWarningFormat($"输出vector {computebuffer[realindex_m1_]}     [ realindex_ :{realindex_m1_} ] by  Y:{ DispatchThreadID_.y} X:{DispatchThreadID_.x }  _CS_HandleData_StartIndex : {_CS_HandleData_StartIndex}");
        // 执行数组 
    }
    
  
     
}

