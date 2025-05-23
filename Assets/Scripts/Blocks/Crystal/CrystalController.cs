using System;
// using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using UnityEditor.Rendering;
using UnityEngine;

public class CrystalController : MonoBehaviour, IInteractable, IResetable
{
    [SerializeField] private int Rotation = 0;
    [SerializeField] private int[] InputPoints;
    [SerializeField] private int[] OutputPoints;
    [SerializeField] private bool[]    ActiveInputs;
    [SerializeField] private bool[]    ActiveOutputs;
    [SerializeField] private bool[]    ActivePoints;
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
    private BoxCollider2D boxCollider2D;
    
    public SpriteRenderer spriteRendererCrystal;
    public SpriteRenderer spriteRendererBase;
    public SpriteRenderer spriteRendererWhole;
    private bool laserHumming = false;
    private int hummingId = 0;
        
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
    [SerializeField] private Sprite[] BaseSprites;
    [SerializeField] private Sprite[] CrystalSprites;
    [SerializeField] private Sprite[] WholeSprites;
    

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

    private bool Awoken = false;
    public void AwakeWrapper()
    {
        if (Awoken) { return; }
        else { Awoken = true;}
        if (InputPoints.Length == 0)
        {
            isSender = true;
            if (!isRotateableSet)
            {
                isRotateable = false;
            }
        }
        ActivePoints = new bool[ValidPositions.Length];
        Array.Fill(ActiveInputs, isSender);
            
        ActiveInputs = new bool[InputPoints.Length];
        Array.Fill(ActiveInputs, isSender);
        
        LastHits = new GameObject[OutputPoints.Length];
        Array.Fill(LastHits, null);
        
        lineRenderer = GetComponent<LineRenderer>();
        Lasers = new LineRenderer[OutputPoints.Length];
        linePoints = new Vector3[OutputPoints.Length*2];
        
        // spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        UpdateSprite();
    }
    
    private void Awake()
    {
        AwakeWrapper();
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

    // private void Update()
    // {
    //     // UpdateCrystalLaserHitbox();
    // }

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
        bool flag = false;
        Vector3 OutputPoint = ValidPositions[OutputPoints[OutputPointIndex]];
        Vector3 origin = transform.position + OutputPoint;
        Vector3 dir = origin - transform.position;
        Vector3 CorrectedOutputPoint = this.GetComponent<PolygonCollider2D>().bounds.ClosestPoint(transform.position + ValidPositions[OutputPoints[OutputPointIndex]]);
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin,
            dir,
            Mathf.Infinity,
            // doesn't work without 1 <<, FUCKING STUPID https://discussions.unity.com/t/raycast2d-not-working-with-layermask/132481
            1 << LayerMask.NameToLayer("Laser")
        );
        // RaycastHit2D hit = hits.FirstOrDefault(h => h.collider.gameObject != this.gameObject);
        RaycastHit2D hit = default;
        foreach (RaycastHit2D iteratorHit in hits.Where(h => h.collider.gameObject != this.gameObject))
        {
            // print(this.name + ": hitting " + iteratorHit.collider.gameObject.name);
            if (iteratorHit)
            {
                Debug.DrawLine(origin, iteratorHit.point, Color.green);
                linePoints[OutputPointIndex * 2 + 1] = iteratorHit.point;
                // print("hit: " + hit.collider.gameObject.name + " / " + hit.point);
                if (iteratorHit.collider.CompareTag("Crystal"))
                {
                    // print("hitP / SB: " + hit.point + " / " + hit.collider.GetComponent<PolygonCollider2D>().ClosestPoint(hit.point));
                    flag = iteratorHit.collider.gameObject.GetComponentInChildren<CrystalController>()
                        .OnLaserHitPoint(iteratorHit.point, dir, isOn, forceTrue);
                    if (flag)
                    {
                        linePoints[OutputPointIndex * 2 + 1] = GetCrystalConnectionPoint(iteratorHit, dir);
                        hit = iteratorHit;
                        break;
                    }
                    else
                    {
                        linePoints[OutputPointIndex * 2 + 1] = CorrectedOutputPoint + dir * 30;
                        continue;
                    }
                    // print("the same: " + linePoints[OutputPointIndex + 1]);
                }
            }
            hit = iteratorHit;
            break;
        }
        // print($"{this.name}: hit {hit}");
        if (hit == default(RaycastHit2D))
        {
            linePoints[OutputPointIndex * 2 + 1] = CorrectedOutputPoint + dir * 30;
            // print("no hits for shits");
        }
        linePoints[OutputPointIndex*2] = CorrectedOutputPoint;
        // If lost LOS on crystal, disable it
        // TODO needs to be checked before assigning LastHits aswell
        if (LastHits[OutputPointIndex] != null && LastHits[OutputPointIndex].CompareTag("Crystal") && (!hit || hit.collider.gameObject != LastHits[OutputPointIndex] || !flag)) {
            // print(this.name + "disconnected: " + LastHits[OutputPointIndex].name);
            LastHits[OutputPointIndex].GetComponentInChildren<CrystalController>().OnLaserHitPoint(hit.point, dir, false, true);
            // TODO make this disable the input and otherwise never reset it
            Array.Fill(ActivePoints, isSender);
            LastHits[OutputPointIndex] = null;
        }

        if (hit) {
            LastHits[OutputPointIndex] = hit.collider.gameObject;
        }
        Debug.DrawRay(origin, dir, Color.red);
    }

    Vector3 GetCrystalConnectionPoint(RaycastHit2D Hit, Vector3 dir)
    {
        // return Hit.collider.GetComponent<PolygonCollider2D>().ClosestPoint(Hit.point);
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            Hit.point,
            dir,
            1, 
            // doesn't work without 1 <<, FUCKING STUPID https://discussions.unity.com/t/raycast2d-not-working-with-layermask/132481
            1 << LayerMask.NameToLayer("Laser")
        );
        // RaycastHit2D finalhit = hits.FirstOrDefault(h => h.collider.gameObject == Hit.collider.gameObject && h.collider is PolygonCollider2D);
        RaycastHit2D finalhit = hits.FirstOrDefault(h => h.collider is PolygonCollider2D);
        return finalhit.point;
    }
    
    // Laser hit Detection
    // Returns if it hit a crystal correctly and should connect a laser to it
    public bool OnLaserHitPoint(Vector3 hitPoint, Vector3 hitDir, bool isOn, bool forceTrue = false)
    {
        bool flag = false;
        // normal hit detection
        for (int i = 0; i < InputPoints.Length; i++)
        {
            if ((hitPoint == transform.position + ValidPositions[InputPoints[i]] && Mathf.Abs(Vector3.Dot(ValidPositions[InputPoints[i]], hitDir)) > 0.1f) || forceTrue)
            {
                ActiveInputs[i] = isOn;
                ActivePoints[InputPoints[i]] = isOn;
                flag =  true;
            }
        }

        // visually if it hits output
        for (int i = 0; i < OutputPoints.Length; i++)
        {
            // print($"{this.name}: {Vector3.Dot(ValidPositions[OutputPoints[i]], hitDir)}");
            if ((hitPoint == transform.position + ValidPositions[OutputPoints[i]] && Mathf.Abs(Vector3.Dot(ValidPositions[OutputPoints[i]], hitDir)) > 0.1f))
            {
                flag = true;
            }
        }
        if (isPointOnLine(transform.position, hitPoint, hitDir))
        {
            flag = true;
        }
        
        return flag;
    }

    public void CheckForOff()
    {
        bool flag = ActiveInputs.All(x => x == false);
        // print($"{this.name}: flag is {flag}");
        if (!isSender && flag)
        {
            UpdateLineRenderer(true);
        }
    }

    bool isPointOnLine(Vector3 r, Vector3 p, Vector3 v)
    {
        return (p.x - r.x) * v.y - (p.y - r.y) * v.x == 0;
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

        if (!laserHumming && isLaserOn)
        {
            hummingId = FMODController.PlaySoundFrom(FMODController.Sound.SFX_LaserHum, this.gameObject);
            laserHumming = true;
        }
        else if (laserHumming && !isLaserOn)
        {
            FMODController.StopSound(hummingId);
            laserHumming = false;
        }
        // print($"{this.name}: laserHumming: {laserHumming}");
    }

    public void UpdateCrystalLaserHitbox()
    {
        // transform.position = transform.GetChild(0).position;
        // transform.GetChild(0).position = transform.position;
        // boxCollider2D.enabled = true;
        // polygonCollider2D.enabled = true;
        // print("SHMOVING THE BOX");
    }
    public void OnMoveStart()
    {
        // boxCollider2D.enabled = false;
        // polygonCollider2D.enabled = false;
        // transform.position = transform.GetChild(0).position;
        // transform.GetChild(0).position = transform.position;
        // print("SHMOVING THE BOX");
    }

    // Rotate on Interact
    [ContextMenu("Interact")]
    void IInteractable.Interact()
    {
        if (!isRotateable) { return; }
        
        int[] CurrentlyActiveInputs = ActivePoints
            .Select((value, index) => new { value, index })  // Capture value and index
            .Where(x => x.value)  // Filter for true values
            .Select(x => x.index) // Select only the indices
            .ToArray();
        
        // Rotate points
        for (int i = 0; i < InputPoints.Length; i++)
        {
            InputPoints[i] = ( InputPoints[i] + 1) % 8;
        }
        for (int i = 0; i < OutputPoints.Length; i++)
        {
            OutputPoints[i] = (OutputPoints[i] + 1) % 8;
        }
        
        // Reduces input delay by reapplying known active input points
        Array.Fill(ActiveInputs, isSender);
        foreach (var point in CurrentlyActiveInputs)
        {
            int index = Array.IndexOf(InputPoints, point);
            if (index > -1)
            {
                ActiveInputs[index] = true;
            }
        }
        
        // Update Sprite and Lasers
        UpdateSprite();
        CrystalLogic();

        Rotation += 1;
		FMODController.PlaySound(FMODController.Sound.SFX_CrystalRotate);
    }

    void IResetable.Reset()
    {
		AwakeWrapper();

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

#if UNITY_EDITOR
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
#endif

    private void UpdateSprite()
    {
        if (OutputPoints.Length > 0)
        {
            spriteRendererCrystal.sprite = CrystalSprites[OutputPoints[0]];
            spriteRendererBase.sprite = BaseSprites[OutputPoints[0]];
            spriteRendererWhole.sprite = WholeSprites[OutputPoints[0]];
        }
        UpdatePhysicsShape();
    }
    private void UpdatePhysicsShape()
    {
        // for (int i = 0; i < polygonCollider2D.pathCount; i++) polygonCollider2D.SetPath(i, (Vector2[])null);
        polygonCollider2D.pathCount = spriteRendererCrystal.sprite.GetPhysicsShapeCount();
        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < polygonCollider2D.pathCount; i++) {
            path.Clear();
            spriteRendererCrystal.sprite.GetPhysicsShape(i, path);
            polygonCollider2D.SetPath(i, path.ToArray());
        }
    }

	void OnDisable() {
		if (laserHumming) {
			if (FMODController.instance != null) {
				FMODController.StopSound(hummingId);
			}

			laserHumming = false;
			hummingId = 0;
		}
	}
}
