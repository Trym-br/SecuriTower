using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;


public class CrystalController : MonoBehaviour
{
    [SerializeField] private Vector3[] InputPoints;
    [SerializeField] private Vector3[] OutputPoints;
    [SerializeField] private bool[]    ActiveInputs;
    [SerializeField] private bool isSender = false;
    public float Rotation;
    
    private bool isLaserOn = true;
    private GameObject[] LastHits;

    private void Awake()
    {
        ActiveInputs = new bool[InputPoints.Length];
        Array.Fill(ActiveInputs, isSender);
        
        LastHits = new GameObject[InputPoints.Length];
        Array.Fill(LastHits, null);
    }

    private void SendLaser(int OutputPointIndex, bool isOn)
    {
        Vector3 OutputPoint = OutputPoints[OutputPointIndex];
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
            
            // If lost LOS on crystal, disable it
            
            print("Hit: " + hit.collider.name + " / " + LastHits[OutputPointIndex]);
            if (LastHits[OutputPointIndex] != null && hit.collider.gameObject != LastHits[OutputPointIndex])
            {
                print("UMBRELLA CASE");
                LastHits[OutputPointIndex].GetComponentInChildren<CrystalController>().OnLaserHitPoint(hit.point, false, true);
                LastHits[OutputPointIndex] = null;
            }
            // If hit crystal, enable it
            if (hit.collider.CompareTag("Crystal"))
            {
               hit.collider.gameObject.GetComponentInChildren<CrystalController>().OnLaserHitPoint(hit.point, isOn);
               LastHits[OutputPointIndex] = hit.collider.gameObject;
            }
            
            Debug.DrawLine(origin, hit.point, Color.green);
        }
        Debug.DrawRay(origin, dir, Color.red);
    }
    
    public void OnLaserHitPoint(Vector3 hitPoint, bool isOn, bool forceTrue = false)
    {
        for (int i = 0; i < InputPoints.Length; i++)
        {
            if (hitPoint == transform.TransformPoint(InputPoints[i]) || forceTrue)
            {
                ActiveInputs[i] = isOn;
            }
        }
        // Cut Down on Delay
        CrystalLogic();
    }

    private void CrystalLogic()
    {
        bool shootFlag = ActiveInputs.All(x=>x);
        if (shootFlag)
        {
            isLaserOn = true;
            for (int i = 0; i < OutputPoints.Length; i++)
            {
                SendLaser(i, true);
            }
        }
        else if (isLaserOn)
        {
            for (int i = 0; i < OutputPoints.Length; i++)
            {
                SendLaser(i, false);
            }
            isLaserOn = false;
        }
    }
    private void FixedUpdate()
    {
        CrystalLogic();
    }

    private void OnDrawGizmos()
    {
        // Drawing input & output Points
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