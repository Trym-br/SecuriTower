using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;


public class CrystalController : MonoBehaviour
{
    [SerializeField] private Vector3[] InputPoints;
    [SerializeField] private bool[]    ActiveInputPoints;
    [SerializeField] private Vector3[] OutputPoints;
    public float Rotation;

    private void SendLaser(Vector3 OutputPoint, bool isON)
    {
        Vector3 origin = transform.TransformPoint(OutputPoint);
        Vector3 dir = origin - transform.position;
        // print("Laser emitting in dir: " + dir);
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            dir,
            Mathf.Infinity
        );
        if (hit)
        {
            // print("Hit: " + hit.collider.name);
            Debug.DrawLine(origin, hit.point, Color.green);
            if (hit.collider.CompareTag("Crystal"))
            {
               hit.collider.gameObject.GetComponent<CrystalController>().OnLaserHitPoint(hit.point);
            }
        }
        Debug.DrawRay(origin, dir, Color.red);
    }

    public void OnLaserHitDir(Vector3 direction)
    {
        for (int i = 0; i < InputPoints.Length; i++)
        {
            Vector3 origin = transform.TransformPoint(InputPoints[i]);
            Vector3 dir = origin - transform.position;
        }
    }
    
    public void OnLaserHitPoint(Vector3 hitPoint)
    {
        for (int i = 0; i < InputPoints.Length; i++)
        {
            print("hitPoint/InputPoint: " + hitPoint + " / "  + InputPoints[i]);
            if (hitPoint == transform.TransformPoint(InputPoints[i]))
            {
                ActiveInputPoints[i] = true;
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < OutputPoints.Length; i++)
        {
            SendLaser(OutputPoints[i], true);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var Point in InputPoints)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawCube(transform.TransformPoint(Point), new Vector3(0.3f, 0.3f, 0.3f)); 
        }
        foreach (var Point in OutputPoints)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(transform.TransformPoint(Point), new Vector3(0.3f, 0.3f, 0.3f)); 
        }
    }
}
