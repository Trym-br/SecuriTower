using System;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class CrystalController : MonoBehaviour, IInteractable, IResetable
{
    [SerializeField] private int Rotation = 0;
    [SerializeField] private int[] InputPoints;
    [SerializeField] private int[] OutputPoints;
    [SerializeField] private bool[]    ActiveInputs;
    
    [SerializeField] private bool isSender = false;
    [SerializeField] private bool AnyInputValid = false;
    [SerializeField] private bool isRotateable = true; // Default value
    [SerializeField, HideInInspector] private bool isRotateableSet = false; // Tracks if modified
    public bool IsRotateable {
        get => isRotateable;
        set { if (!isRotateableSet) isRotateableSet = true; isRotateable = value; }
    }

    public bool isLaserOn = false;
    private GameObject[] LastHits;
    private LineRenderer lineRenderer;
    private Vector3[] linePoints;  
    
    private Vector3[] ValidPositions = new Vector3[]
    {
        new Vector3(0f, 0.5f),
        new Vector3(0.5f, 0.5f),
        new Vector3(0.5f, 0f),
        new Vector3(0.5f, -0.5f),
        new Vector3(0, -0.5f),
        new Vector3(-0.5f, -0.5f),
        new Vector3(-0.5f, 0f),
        new Vector3(-0.5f, 0.5f),
    };

    private void OnValidate()
    {
        for (int i = 0; i < InputPoints.Length; i++)
        {
            InputPoints[i] = Mathf.Clamp(InputPoints[i], 0, 7);
        }

        for (int i = 0; i < OutputPoints.Length; i++)
        {
            OutputPoints[i] = Mathf.Clamp(OutputPoints[i], 0, 7);
        }
    }
    
    private void Awake()
    {
        if (InputPoints.Length == 0)
        {
            isSender = true;
            if (!isRotateableSet)
            {
                isRotateable = false;
            }
        }
        ActiveInputs = new bool[InputPoints.Length];
        Array.Fill(ActiveInputs, isSender);
        
        LastHits = new GameObject[OutputPoints.Length];
        Array.Fill(LastHits, null);
        
        lineRenderer = GetComponent<LineRenderer>();
        linePoints = new Vector3[OutputPoints.Length*3];
    }

    private void FixedUpdate()
    {
        CrystalLogic();
    }
    private void SendLaser(int OutputPointIndex, bool isOn, bool forceTrue = false)
    {
        Vector3 OutputPoint = ValidPositions[OutputPoints[OutputPointIndex]];
        Vector3 origin = transform.position + OutputPoint;
        Vector3 dir = origin - transform.position;
        // print("Laser emitting in dir: " + dir + " / on layer: " + LayerMask.NameToLayer("Laser"));
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            dir,
            Mathf.Infinity,
            // doesn't work without 1 <<, FUCKING STUPID https://discussions.unity.com/t/raycast2d-not-working-with-layermask/132481
            1 << LayerMask.NameToLayer("Laser")
        );
        // RaycastHit2D[] hits = Physics2D.RaycastAll(
        //     origin,
        //     dir,
        //     Mathf.Infinity,
        //     // doesn't work without 1 <<, FUCKING STUPID https://discussions.unity.com/t/raycast2d-not-working-with-layermask/132481
        //     1 << LayerMask.NameToLayer("Laser")
        // );
        // RaycastHit2D hit = hits.Skip(1).Where(hit => hit.collider.tag == "Crystal").FirstOrDefault();
        
        if (hit) {
            Debug.DrawLine(origin, hit.point, Color.green);
            linePoints[OutputPointIndex] = hit.point;
            if (hit.collider.CompareTag("Crystal"))
            { 
                hit.collider.gameObject.GetComponentInChildren<CrystalController>().OnLaserHitPoint(hit.point, isOn, forceTrue);
            }
        }
        else {
            linePoints[OutputPointIndex] = origin + dir*30;
        }
        linePoints[OutputPointIndex+1] = transform.position + ValidPositions[OutputPoints[OutputPointIndex]];
        linePoints[OutputPointIndex + 2] = transform.position + ValidPositions[OutputPoints[OutputPointIndex]];
        // If lost LOS on crystal, disable it
        // TODO needs to be checked before assigning LastHits aswell
        if (LastHits[OutputPointIndex] != null && LastHits[OutputPointIndex].CompareTag("Crystal") && (!hit || hit.collider.gameObject != LastHits[OutputPointIndex])) {
            // print("UMBRELLA CASE");
            // print("lasthit: " + LastHits[OutputPointIndex]);
            LastHits[OutputPointIndex].GetComponentInChildren<CrystalController>().OnLaserHitPoint(hit.point, false, true);
            LastHits[OutputPointIndex] = null;
        }

        if (hit) {
            LastHits[OutputPointIndex] = hit.collider.gameObject;
        }
        Debug.DrawRay(origin, dir, Color.red);
    }
    
    // Laser hit Detection
    public void OnLaserHitPoint(Vector3 hitPoint, bool isOn, bool forceTrue = false)
    {
        print("hitPoint/InputPoint" + hitPoint + " / " + (transform.position + ValidPositions[InputPoints[0]]) + " | " + (transform.position + ValidPositions[OutputPoints[0]]));
        // Crystal Breaking logic
        for (int i = 0; i < OutputPoints.Length; i++)
        {
            if (hitPoint == transform.position + ValidPositions[OutputPoints[i]])
            {
                print("DESTROY: hitPoint/InputPoint" + hitPoint + " / " + OutputPoints[i]);
                if (!isSender && isLaserOn)
                {
                    DestroyCrystal(isOn);
                }
            }
        }
        for (int i = 0; i < InputPoints.Length; i++)
        {
            // print(this.name + ": " + hitPoint + " / " + (transform.position + ValidPositions[InputPoints[i]]));
            if (hitPoint == transform.position + ValidPositions[InputPoints[i]] || forceTrue)
            {
                ActiveInputs[i] = isOn;
            }
        }
        // Cut Down on Delay
        // CrystalLogic();
    }

    private void DestroyCrystal(bool fromBeyondTheGrave = false)
    {
        print(this.name + ": is dying / " + fromBeyondTheGrave);
        if (!fromBeyondTheGrave)
        {
            SendLaser(0,false, true);
        }
       Destroy(transform.gameObject); 
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
        
        if (shootFlag || isSender)
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
        if (!isRotateable) { return; }
        
        // Rotate Crystal V2
        Array.Fill(ActiveInputs, isSender);
        CrystalLogic();
        for (int i = 0; i < InputPoints.Length; i++)
        {
            InputPoints[i] = ( InputPoints[i] + 1) % 8;
        }
        for (int i = 0; i < OutputPoints.Length; i++)
        {
            OutputPoints[i] = (OutputPoints[i] + 1) % 8;
        }
        Rotation += 1;
    }

    void IResetable.Reset()
    {
        // Resets Inputs && Rotation
        Array.Fill(ActiveInputs, isSender);
        for (int i = 0; i < InputPoints.Length; i++)
        {
            InputPoints[i] = (InputPoints[i] + 8 - Rotation % 8) % 8;
        }
        for (int i = 0; i < OutputPoints.Length; i++)
        {
            OutputPoints[i] = (OutputPoints[i] + 8 - Rotation % 8) % 8;
        }
        Rotation = 0;
    }

    private void OnDrawGizmos()
    {
        // Drawing input & output Points
        foreach (var Point in InputPoints)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawCube(transform.position + ValidPositions[Point], new Vector3(0.3f, 0.3f, 0.3f)); 
        }
        foreach (var Point in OutputPoints)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(transform.position + ValidPositions[Point], new Vector3(0.3f, 0.3f, 0.3f)); 
        }
    }
}