using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;


public class CrystalController1 : MonoBehaviour
{
    [SerializeField] private Vector3[] InputPoints;
    [SerializeField] private Vector3[] OutputPoints;
    [SerializeField] private GameObject[] OutputLastHit;
    // [SerializeField] private int[] OutputLastHit;
    // [SerializeField] private Func<bool>[] crystalConditionals;
    public Func<bool>[] crystalConditionals;
    [SerializeField] private bool[] ActiveInputs;
    [SerializeField] private bool[] ActiveOutputs;
    
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        ActiveInputs = new bool[InputPoints.Length];
        ActiveOutputs = new bool[OutputPoints.Length];
        OutputLastHit = new GameObject[OutputPoints.Length];
        Array.Fill(ActiveInputs, false);
        Array.Fill(ActiveOutputs, false);
        crystalConditionals = new Func<bool>[]{};
    }

    public void OnLaserInteract(Vector3 Direction, bool Attach)
    {
        // if hit.point is in input, then it activates the input
        
        // for (int i = 0; i < InputPoints.Length; i++)
        // {
        //     print("hP / IP | " + hitPoint + " / " + transform.TransformPoint(InputPoints[i]) );
        //     if (hitPoint == transform.TransformPoint(InputPoints[i]))
        //     {
        //         ActiveInputs[i] = Attach;
        //     }
        // }
        // Handle Inputs
        int Index = Array.IndexOf(InputPoints, -Direction);
        // print("Direction: " + Direction/2 + " / " + InputPoints[0] + " | " + Index);
        if (Index != -1)
        {
            ActiveInputs[Index] = Attach;
        }
    }

    private void CrystalLogic()
    {
        bool clearFlag = true;
        foreach (var input in ActiveInputs)
        {
            if (!input)
            {
                clearFlag = false;
            }
        }

        if (clearFlag)
        {
            ActiveOutputs[0] = true;
            for (int i = 0; i < OutputPoints.Length; i++)
            {
                // print(i + " outputPoint: " + OutputPoints[i]);
                SendLaser(OutputPoints[i], true);
            }
        }
        else if(ActiveOutputs[0])
        {
            for (int i = 0; i < OutputPoints.Length; i++)
            {
                // print(i + " outputPoint: " + OutputPoints[i]);
                SendLaser(OutputPoints[i], false);
                // OnLaserInteract(OutputPoints[i],false);
                ActiveOutputs[i] = false; 
            }
        }
    }

    private void SendLaser(Vector3 OutputPoint, bool isON)
    {
        Vector3 origin = transform.TransformPoint(OutputPoint);
        Vector3 dir = origin - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            dir,
            Mathf.Infinity
        );
        if (hit.distance > 0)
        {
            // print("Found a " + hit.rigidbody.gameObject.name + "/ distance: " + hit.distance);
            // print("Laser blocked?: " + OutputLastHit[Array.IndexOf(OutputPoints, OutputPoint)] != null + " / " + hit.rigidbody.gameObject != OutputLastHit[Array.IndexOf(OutputPoints, OutputPoint)]);
            if (OutputLastHit[Array.IndexOf(OutputPoints, OutputPoint)] != null && hit.rigidbody.gameObject != OutputLastHit[Array.IndexOf(OutputPoints, OutputPoint)])
            {
                OutputLastHit[Array.IndexOf(OutputPoints, OutputPoint)].GetComponent<CrystalController1>().OnLaserInteract(hit.point, false);
            }
            Debug.DrawLine(origin, hit.point, Color.green);
            if (hit.rigidbody.gameObject.CompareTag("Crystal"))
            {
                hit.rigidbody.gameObject.GetComponent<CrystalController1>().OnLaserInteract(OutputPoint, isON);
                OutputLastHit[Array.IndexOf(OutputPoints, OutputPoint)] = hit.rigidbody.gameObject;
            }
        }
        else if (OutputLastHit[Array.IndexOf(OutputPoints, OutputPoint)] != null)
        {
            OutputLastHit[Array.IndexOf(OutputPoints, OutputPoint)].GetComponent<CrystalController1>().OnLaserInteract(hit.point, false);
        }
        Debug.DrawRay(origin, dir, Color.red);
        // hit.rigidbody.gameObject.GetComponent<CrystalController>().OnLaserInteract(Vector2.left, isON);
    }

    private void FixedUpdate()
    {
       CrystalLogic(); 
    }

    // Debug Draw input and output points
    private void OnDrawGizmos()
    {
        foreach (var Point in InputPoints)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            // Gizmos.DrawCube(transform.position + (Point - transform.up), new Vector3(0.3f, 0.3f, 0.3f)); 
            // Gizmos.DrawCube(transform.position + (Point - transform.up), new Vector3(0.3f, 0.3f, 0.3f)); 
            Gizmos.DrawCube(transform.TransformPoint(Point), new Vector3(0.3f, 0.3f, 0.3f)); 
        }
        foreach (var Point in OutputPoints)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(transform.TransformPoint(Point), new Vector3(0.3f, 0.3f, 0.3f)); 
        }
    }
}
