using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_move : MonoBehaviour
{

    public Camera m_Camera;
    public float speed = 0.02f;

    private CharacterController m_CharacterController;

    public Vector3 move = Vector3.zero;
    

    // Start is called before the first frame update
    void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 forward = Vector3.ProjectOnPlane(m_Camera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(m_Camera.transform.right, Vector3.up).normalized;

        move = (v * forward + h * right).normalized * speed * Time.deltaTime;

        //transform.position += move;
        m_CharacterController.Move(move + Vector3.down * 0.02f);

        
        

    }
}
