using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 targetPos;
    public float speed;

    private void Start()
    {
        if (!target)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        } 
    }

    private void Update()
    {
       // transform.position = new Vector3(target.position.x, target.position.y, transform.position.z); 
       targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
       if (speed == -1)
       {
           transform.position = targetPos;
       }
       else
       {
           transform.position = Vector3.Lerp(transform.position, targetPos, speed * Time.deltaTime);
       }
    }
}


// public Transform Trans;
//
// public float CamMoveSpeed = 5f;
//
// private void Start()
// {
//     _cam = Trans.position;
// }
//
// private void Update()
// {
//     if (Input.GetMouseButton(0))
//     {
//         float xinput = Input.GetAxisRaw("Mouse X");
//         float zinput = Input.GetAxisRaw("Mouse Y");
//
//         float x = Trans.position.x;
//         float z = Trans.position.z;
//         float y = Trans.position.y;
//
//         _cam = new Vector3(x + xinput, y, z + zinput);            
//     }
//
//     Trans.position = Vector3.Lerp(Trans.position, _cam, CamMoveSpeed * Time.deltaTime);
// }