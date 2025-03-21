using System;
using System.Linq;
using UnityEngine;


public class CrystalController : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3[] InputPoints;
    [SerializeField] private Vector3[] OutputPoints;
    [SerializeField] private bool[]    ActiveInputs;
    private bool isSender = false;
    [SerializeField] private bool AnyInputValid = false;
    public float Rotation;
    
    private GameObject[] LastHits;
    public bool isLaserOn = false;
    private LineRenderer lineRenderer;
    private Vector3[] linePoints;  

    private void Awake()
    {
        if (InputPoints.Length == 0)
        {
            InputPoints = new Vector3[1];
            InputPoints[0] = new Vector3(0, 0, 0);
            isSender = true;
        }
        ActiveInputs = new bool[InputPoints.Length];
        Array.Fill(ActiveInputs, isSender);
        
        LastHits = new GameObject[InputPoints.Length];
        Array.Fill(LastHits, null);
        
        lineRenderer = GetComponent<LineRenderer>();
        linePoints = new Vector3[OutputPoints.Length*3];
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
            linePoints[OutputPointIndex] = hit.point;
            linePoints[OutputPointIndex+1] = transform.TransformPoint(OutputPoints[OutputPointIndex]);
            linePoints[OutputPointIndex+2] = transform.TransformPoint(OutputPoints[OutputPointIndex]);
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
        else
        {
            linePoints[OutputPointIndex] = transform.TransformPoint(OutputPoints[OutputPointIndex]);
            linePoints[OutputPointIndex+1] = transform.TransformPoint(OutputPoints[OutputPointIndex]);
            linePoints[OutputPointIndex+2] = transform.TransformPoint(OutputPoints[OutputPointIndex]);
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
        // Crystal Breaking logic
        for (int i = 0; i < OutputPoints.Length; i++)
        {
            if (hitPoint == transform.TransformPoint(OutputPoints[i]))
            {
                // print("DESTROY: hitPoint/InputPoint" + hitPoint + " / " + transform.TransformPoint(OutputPoints[i]));
                if (ActiveInputs.All(x => x))
                {
                    DestroyCrystal();
                }
            }
        }
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
    }

    private void DestroyCrystal()
    {
       Destroy(transform.parent.gameObject); 
    }

    private void UpdateLineRenderer(bool turnOff = false)
    {
        if (turnOff)
        {
            for (int i = 0; i < linePoints.Length; i++)
            {
                linePoints[i] = Vector3.one;
            }
        }
        lineRenderer.positionCount = linePoints.Length;
        lineRenderer.SetPositions(linePoints);
    }

    private void CrystalLogic()
    {
        bool shootFlag = false;
        if (AnyInputValid)
        {
            shootFlag = ActiveInputs.Any(x => x);
        }
        else
        {
            shootFlag = ActiveInputs.All(x => x);
        }
        
        if (shootFlag)
        {
            isLaserOn = true;
            for (int i = 0; i < OutputPoints.Length; i++)
            {
                SendLaser(i, true);
            }
            UpdateLineRenderer();
        }
        else if (isLaserOn)
        {
            isLaserOn = false;
            for (int i = 0; i < OutputPoints.Length; i++)
            {
                SendLaser(i, false);
            }
            UpdateLineRenderer(true);
        }
    }

    [ContextMenu("Interact")]
    void IInteractable.Interact()
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