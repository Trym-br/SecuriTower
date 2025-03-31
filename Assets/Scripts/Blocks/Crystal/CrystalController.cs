using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class CrystalController : MonoBehaviour, IInteractable, IResetable
{
    [SerializeField] private int Rotation = 0;
    [SerializeField] private int[] InputPoints;
    [SerializeField] private int[] OutputPoints;
    [SerializeField] private bool[]    ActiveInputs;
    [SerializeField] private bool[]    ActiveOutputs;
    [SerializeField] private LineRenderer[] Lasers;
    
    [SerializeField] private bool isSender = false;
    [SerializeField] private bool AnyInputValid = false;
    [SerializeField] private bool isRotateable = true; // Default value
    [SerializeField, HideInInspector] private bool isRotateableSet = false; // Tracks if modified
    
    [SerializeField] private GameObject _laserPrefab;

    //[SerializeField] private float DestructionTime = 3f;
    //[SerializeField] private float DestructionTimer;
    //private bool isDying;
    private PolygonCollider2D polygonCollider2D;
    
    private SpriteRenderer spriteRenderer;
        
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
    private Vector3[] CardinalPositions = new Vector3[]
    {
        new Vector3(0f, 0.5f),
        new Vector3(0.5f, 0f),
        new Vector3(0, -0.5f),
        new Vector3(-0.5f, 0f),
    };
    [SerializeField] private Sprite[] Sprites;

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
        Lasers = new LineRenderer[OutputPoints.Length];
        linePoints = new Vector3[OutputPoints.Length*2];
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        
        UpdateSprite();
    }

    private void Start()
    {
       // create lasers 
       for (int i = 0; i < OutputPoints.Length; i++)
       {
           // GameObject gc = new GameObject("Laser " + i.ToString());
           // gc.transform.SetParent(this.transform);
           // LineRenderer lr = gc.AddComponent(typeof(LineRenderer)) as LineRenderer;
           GameObject gc = Instantiate(_laserPrefab, this.transform);
           Lasers[i] = gc.GetComponent<LineRenderer>();
       }
    }

	bool playedReceiverNoise;
    private void FixedUpdate()
    {
		if (OutputPoints.Length == 0) {
			bool isActive = ActiveInputs.Any(x => x);
			if (isActive) {
				if(!playedReceiverNoise) {
					FMODController.PlaySoundFrom(FMODController.Sound.SFX_LaserReceiver, gameObject);
					playedReceiverNoise = true;
				}
			} else {
				playedReceiverNoise = false;
			}
		}

        CrystalLogic();
    }
    private void SendLaser(int OutputPointIndex, bool isOn, bool forceTrue = false)
    {
        Vector3 OutputPoint = ValidPositions[OutputPoints[OutputPointIndex]];
        Vector3 origin = transform.position + OutputPoint;
        Vector3 dir = origin - transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin,
            dir,
            Mathf.Infinity,
            // doesn't work without 1 <<, FUCKING STUPID https://discussions.unity.com/t/raycast2d-not-working-with-layermask/132481
            1 << LayerMask.NameToLayer("Laser")
        );
        RaycastHit2D hit = hits.FirstOrDefault(h => h.collider.gameObject != this.gameObject);
        
        if (hit) {
            Debug.DrawLine(origin, hit.point, Color.green);
            linePoints[OutputPointIndex*2 + 1] = hit.point;
            // print("hit: " + hit.collider.gameObject.name + " / " + hit.point);
            if (hit.collider.CompareTag("Crystal"))
            {
                // print("hitP / SB: " + hit.point + " / " + hit.collider.GetComponent<PolygonCollider2D>().ClosestPoint(hit.point));
                if (hit.collider.gameObject.GetComponentInChildren<CrystalController>()
                    .OnLaserHitPoint(hit.point, dir, isOn, forceTrue))
                {
                    linePoints[OutputPointIndex*2 + 1] = hit.collider.GetComponent<PolygonCollider2D>().ClosestPoint(hit.point);
                } else {
                    linePoints[OutputPointIndex*2 + 1] = dir*30;
                }
                // print("the same: " + linePoints[OutputPointIndex + 1]);
            }
        }
        else {
            linePoints[OutputPointIndex*2 + 1] = dir*30;
        }
        Vector3 CorrectedOutputPoint = this.GetComponent<PolygonCollider2D>().bounds.ClosestPoint(transform.position + ValidPositions[OutputPoints[OutputPointIndex]]);
        linePoints[OutputPointIndex*2] = CorrectedOutputPoint;
        // If lost LOS on crystal, disable it
        // TODO needs to be checked before assigning LastHits aswell
        if (LastHits[OutputPointIndex] != null && LastHits[OutputPointIndex].CompareTag("Crystal") && (!hit || hit.collider.gameObject != LastHits[OutputPointIndex])) {
            LastHits[OutputPointIndex].GetComponentInChildren<CrystalController>().OnLaserHitPoint(hit.point, dir, false, true);
            LastHits[OutputPointIndex] = null;
        }

        if (hit) {
            LastHits[OutputPointIndex] = hit.collider.gameObject;
        }
        Debug.DrawRay(origin, dir, Color.red);
    }
    
    // Laser hit Detection
    // Returns if it hit a crystal correctly and should connect a laser to it
    public bool OnLaserHitPoint(Vector3 hitPoint, Vector3 hitDir, bool isOn, bool forceTrue = false)
    {
        // normal hit detection
        for (int i = 0; i < InputPoints.Length; i++)
        {
            // if (hitPoint == transform.position + ValidPositions[InputPoints[i]])
            // {
                // print(this.name + ": this/that/dir: " + ValidPositions[InputPoints[i]] + " / " + hitDir + " / " + Vector3.Dot(ValidPositions[InputPoints[i]], hitDir));
            // }

            if ((hitPoint == transform.position + ValidPositions[InputPoints[i]] && Vector3.Dot(ValidPositions[InputPoints[i]], hitDir) < 0) || forceTrue)
            {
                ActiveInputs[i] = isOn;
                return true;
            }
        }

        // visually if it hits output
        for (int i = 0; i < OutputPoints.Length; i++)
        {
            if ((hitPoint == transform.position + ValidPositions[OutputPoints[i]] && Vector3.Dot(ValidPositions[OutputPoints[i]], hitDir) == -0.25))
            {
                return true;
            }
        }
        return false;
    }
    
    private void UpdateLineRenderer(bool turnOff = false)
    {
        if (turnOff)
        {
            for (int i = 0; i < linePoints.Length; i++)
            {
                linePoints[i] = Vector3.zero;
            }
        }
        // FMODController.PlaySoundFrom(FMODController.Sound.SFX_LaserHum, this.gameObject);
        for (int i = 0; i < Lasers.Length; i++)
        {
            Lasers[i].positionCount = 2;
            Lasers[i].SetPosition(0, linePoints[i*2]);
            Lasers[i].SetPosition(1, linePoints[i*2+1]);
        }
        // lineRenderer.positionCount = linePoints.Length;
        // lineRenderer.SetPositions(linePoints);
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

        UpdateSprite();

        Rotation += 1;

		FMODController.PlaySound(FMODController.Sound.SFX_CrystalRotate);
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
        UpdateSprite();
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

    private void UpdateSprite()
    {
        if (OutputPoints.Length > 0)
        {
            spriteRenderer.sprite = Sprites[OutputPoints[0]];
        }
        UpdatePhysicsShape();
    }
    private void UpdatePhysicsShape()
    {
        // for (int i = 0; i < polygonCollider2D.pathCount; i++) polygonCollider2D.SetPath(i, (Vector2[])null);
        polygonCollider2D.pathCount = spriteRenderer.sprite.GetPhysicsShapeCount();

        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < polygonCollider2D.pathCount; i++) {
            path.Clear();
            spriteRenderer.sprite.GetPhysicsShape(i, path);
            polygonCollider2D.SetPath(i, path.ToArray());
        }
    }
}
