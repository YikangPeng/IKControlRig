using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion;

public class ControlRig : MonoBehaviour
{

    public Transform RightHandBone;//右手骨骼
    public Transform Collider_RightHand;//右手用于射线检测的起点
    public Transform Effector_RightHand;//右手IK手腕控制点
    public Transform TargetPoint;//手IK目标点
    public Transform SmoothTarget;//debug平滑目标点
    
    public float SearchLength = 0.3f;//离墙多少距离以内会扶上去    

    public RootMotion.FinalIK.ArmIK HandIK;

    private bool hitWall = false;
    private bool AttachWall = false;//是否扶在墙上
    private bool LeaveWall = false;
    private RaycastHit WallHit;//墙面碰撞检测

    private Vector3 vel;
    private Vector3 lasttarget;
    private float ikweight = 0.0f;

    private LayerMask wallmask;


    // Start is called before the first frame update
    void Start()
    {
        SmoothTarget.position = RightHandBone.position;
        Effector_RightHand.position = RightHandBone.position;
        wallmask = 1 << LayerMask.NameToLayer("Wall");
    }

    // Update is called once per frame
    void Update()
    {
        
        
        
    }

    
    private void LateUpdate()
    {
        if (AttachWall)
        {
            Vector3 targetobjspace = TargetPoint.localPosition;
            if (targetobjspace.z < -0.3f)
            {
                AttachWall = false;
                LeaveWall = true;
                lasttarget = TargetPoint.position;
            }
        }

        if (!AttachWall)
        {
            hitWall = Physics.Raycast(Collider_RightHand.position, transform.right, out WallHit, SearchLength, wallmask);
        }

        if (hitWall)
        {
            
            Vector3 HandY = WallHit.normal.normalized;
            Vector3 HandZ = Vector3.Cross(Vector3.up ,HandY);
            Vector3 HandX = Vector3.Cross(HandY, HandZ);
            HandZ = HandZ - HandX * TargetPoint.localPosition.z * WallHit.distance * 2.0f;
            HandZ = HandZ.normalized;
            //TargetPoint.rotation = Quaternion.LookRotation(HandZ, HandY);
            Effector_RightHand.rotation = Quaternion.LookRotation(HandZ, HandY);

            TargetPoint.position = WallHit.point + HandY * 0.025f;

            AttachWall = true;
        }
        else
        {
            TargetPoint.position = RightHandBone.position;
            LeaveWall = false;
        }

        SetWeight();


        SmoothTarget.position = Vector3.SmoothDamp(SmoothTarget.position, TargetPoint.position,ref vel, 0.15f);
        

        Vector3 characenter = Collider_RightHand.position - 0.5f * transform.forward;
        Vector3 smoothdir = SmoothTarget.position - characenter;
        float smoothlength = smoothdir.magnitude;
        smoothdir = smoothdir.normalized;
        RaycastHit smoothpoint;
        bool Smoothhit = Physics.Raycast(characenter, smoothdir, out smoothpoint, smoothlength - 0.05f , wallmask);

        Vector3 FixSmoothPosition = SmoothTarget.position;
        
        if (LeaveWall)
        {
            

            float bias = 1 - Vector3.Distance(SmoothTarget.position, TargetPoint.position) / Vector3.Distance(TargetPoint.position, lasttarget);
            Debug.Log(bias);
            bias = Mathf.Clamp01(bias);
            bias = Mathf.Sin(bias * Mathf.PI);
            
            //Debug.Log(smoothlength +"and"+ smoothpoint.distance);

            if (Smoothhit)
            {
                
                FixSmoothPosition = smoothpoint.point - smoothdir * 0.02f;
            }

            FixSmoothPosition = FixSmoothPosition - smoothdir * 0.1f * bias * 0.4f;
        }
                

        Effector_RightHand.position = FixSmoothPosition;
        
        
    }

    
    void SetWeight()
    {
        if (AttachWall)
        {
            ikweight += 0.1f;
        }
        else
        {
            ikweight -= 0.1f;
        }

        ikweight = Mathf.Clamp01(ikweight);
        
        HandIK.solver.IKRotationWeight = ikweight;
    }
}
