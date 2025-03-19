using System;
using System.Linq;
using UnityEngine;


public class CrystalController : MonoBehaviour
{
    [SerializeField] private Vector3[] InputPoints;
    [SerializeField] private Vector3[] OutputPoints;
    [SerializeField] private bool[]    ActiveInputs;
    [SerializeField] private bool isSender = false;
    public float Rotation;
    
    public bool isLaserOn = false;
    private GameObject[] LastHits;

    private void Awake()
    {
        ActiveInputs = new bool[InputPoints.Length];
        Array.Fill(ActiveInputs, isSender);
        
        LastHits = new GameObject[InputPoints.Length];
        Array.Fill(LastHits, null);
    }

    private void FixedUpdate()
    {
        CrystalLogic();
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
            Debug.DrawLine(origin, hit.point, Color.green);
            // print("Hit: " + hit.collider.name);
            
            
            // print("Hit: " + hit.collider.name + " / " + LastHits[OutputPointIndex]);
            // If hit crystal, enable it
            if (hit.collider.CompareTag("Crystal"))
            { 
                // print("");
                hit.collider.gameObject.GetComponentInChildren<CrystalController>().OnLaserHitPoint(hit.point, isOn);
                LastHits[OutputPointIndex] = hit.collider.gameObject;
            }
        }
        // If lost LOS on crystal, disable it
        if (LastHits[OutputPointIndex] != null && (!hit || hit.collider.gameObject != LastHits[OutputPointIndex]))
        {
            // print("UMBRELLA CASE");
            LastHits[OutputPointIndex].GetComponentInChildren<CrystalController>().OnLaserHitPoint(hit.point, false, true);
            LastHits[OutputPointIndex] = null;
        }
        Debug.DrawRay(origin, dir, Color.red);
    }
    
    public void OnLaserHitPoint(Vector3 hitPoint, bool isOn, bool forceTrue = false)
    {
        // print("hitPoint/InputPoint" + hitPoint + " / " + transform.TransformPoint(InputPoints[0]));
        for (int i = 0; i < InputPoints.Length; i++)
        {
            if (hitPoint == transform.TransformPoint(InputPoints[i]) || forceTrue)
            {
                ActiveInputs[i] = isOn;
            }
        }
        // Cut Down on Delay
        CrystalLogic();
        
        // Breaking logic
        for (int i = 0; i < OutputPoints.Length; i++)
        {
            if (hitPoint == transform.TransformPoint(OutputPoints[i]) || forceTrue)
            {
                DestroyCrystal();
            }
        }
        
    }

    private void DestroyCrystal()
    {
        
    }

    private void CrystalLogic()
    {
        bool shootFlag = ActiveInputs.All(x=> x);
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
            isLaserOn = false;
            for (int i = 0; i < OutputPoints.Length; i++)
            {
                SendLaser(i, false);
            }
        }
    }

    [ContextMenu("Interact")]
    private void Interact()
    {
        //Rotate Crystal
        Array.Fill(ActiveInputs, isSender);
        transform.Rotate(Vector3.forward, 45);
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