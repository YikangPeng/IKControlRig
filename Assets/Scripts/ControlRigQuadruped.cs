using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ControlRigQuadruped : MonoBehaviour
{

    [Header("身体位置")]
    public Transform main;

    //[Header("内半径")]
    //public float nearradius;
    [Header("重心半径")]
    public float farradius;

    [Header("足半径")]
    public float footradius;

    [Header("脚移动时间")]
    public float footspeed = 0.02f;

    //public float stride = 0.4f;

    private Vector3 move = Vector3.zero;

    private LayerMask groundmask;

    [System.Serializable]
    public class IKGroup
    {
        public Transform Controller;
        public Transform Target;
        public Transform Hit;
        public float radius;
    }

    //所有的脚的List
    public List<IKGroup> IKGroups = new List<IKGroup>();
    private List<Vector3> lasttargetpos ;//上一帧的位置
    private List<float> moveweight;//每个脚在移动中的轨迹位置权重

    private Vector3 mainlocalpos;
    private Quaternion mainlocalrotation;

    private Vector3 vel;

    // Start is called before the first frame update
    void Start()
    {
        //碰撞检测的层
        groundmask = 1 << LayerMask.NameToLayer("Ground");

        lasttargetpos = new List<Vector3>();
        moveweight = new List<float>();

        for (int i  = 0; i < IKGroups.Count; i++)
        {
            moveweight.Add(1.0f);
            lasttargetpos.Add(Vector3.zero);
        }

        mainlocalpos = transform.localPosition;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        move = GetComponent<camera_move>().move;        

        //Debug.Log(move.normalized);
        
        //修正脚
        FixIK();

        //修正重心
        FixMain();
    }


    //修正脚
    void FixIK()
    {
        //每个脚循环计算
        for (int i = 0; i < IKGroups.Count; i++)
        {
            

            int movefootnumber = 0;

            //统计在地面上的脚的数量
            for (int j = 0; j < IKGroups.Count; j++)
            {
                if (moveweight[j] >= 1.0f)
                {
                    movefootnumber += 1;
                }                    

            }

            //Debug.Log(movefootnumber);


            //计算落点
            RaycastHit footHit;
            RaycastHit targetHit;
            bool footGroud = Physics.Raycast(IKGroups[i].Hit.position, Vector3.down, out footHit, 0.7f, groundmask);

            Vector3 hitpos;            

            //记录一个判定的位置，是脚的目标落点，如果没有那么就是射线发射点下方            
            if (footGroud)
            {
                hitpos = footHit.point;
            }
            else
            {
                hitpos = IKGroups[i].Hit.position;
                hitpos.y = hitpos.y - 0.5f;
            }

            float dis = Vector3.Distance(hitpos, IKGroups[i].Target.position);

            //当到这个位置的距离超出设置的距离时，并且在地面的脚够多的不至于一只脚站立，那么就需要迈腿
            if ((dis > IKGroups[i].radius) && (movefootnumber > IKGroups.Count / 2))
            {
                //计算迈腿的位置
                //IKGroups[i].Target.position = hitpos + move.normalized * footradius;
                Vector3 targetpos = hitpos + move.normalized * IKGroups[i].radius + new Vector3(0.0f, 0.5f, 0.0f);
                bool hitGroud = Physics.Raycast(targetpos, Vector3.down, out targetHit, 0.7f, groundmask);

                
                if (hitGroud)
                {
                    targetpos = targetHit.point;
                    lasttargetpos[i] = IKGroups[i].Target.position;
                    IKGroups[i].Target.position = targetpos;
                    moveweight[i] = 0.0f;
                    
                }

                
            }


            //计算路径
            Vector3 movedir = IKGroups[i].Target.position - lasttargetpos[i];
            Vector3 right = Vector3.Cross(Vector3.up, movedir.normalized);
            Vector3 moveup = Vector3.Cross(movedir.normalized, right);

            IKGroups[i].Controller.position = Vector3.Lerp(lasttargetpos[i], IKGroups[i].Target.position, moveweight[i]);
            IKGroups[i].Controller.position += moveup * Mathf.Sin(moveweight[i] * Mathf.PI) * 0.1f;

            moveweight[i] += footspeed;
            moveweight[i] = Mathf.Clamp01(moveweight[i]);

            //Debug.Log(dis);
        }
    }


    void FixMain()
    {
        //根据每个角的高度和旋转，修正身体的高度旋转
        
        float gravity_Y = 0;
        float rotate_Z = 0;
        float rotate_X = 0;
        
        for (int i = 0; i < IKGroups.Count; i++)
        {
            Vector3 objpos = transform.InverseTransformPoint(IKGroups[i].Controller.position);

            gravity_Y += objpos.y;
            rotate_Z += (objpos.y / objpos.x);
            rotate_X += (objpos.y / objpos.z);
        }

        main.localPosition =Vector3.SmoothDamp(main.localPosition, mainlocalpos + new Vector3(0.0f, gravity_Y * 0.4f , 0.0f) , ref vel , 0.1f);

        //main.rotation = Quaternion.AngleAxis(rotate_Z, main.transform.forward) * main.rotation;

    }

    void OnDrawGizmos()
    {
        //Handles.color = Color.red;        
        //Handles.DrawWireArc(transform.position, Vector3.up, Vector3.right, 360, nearradius);

        Handles.color = Color.green;
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.right, 360, farradius);

        for (int i = 0; i < IKGroups.Count; i++)
        {
            Handles.color = Color.blue;
            Vector3 pos = IKGroups[i].Hit.position;
            pos.y -= 0.5f;
            Handles.DrawWireArc(pos, Vector3.up, Vector3.right, 360, IKGroups[i].radius);
        }
    }


}
