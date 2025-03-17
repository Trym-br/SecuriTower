using UnityEngine;

public class CrystalSender : MonoBehaviour
{
    [SerializeField] private Vector3[] OutputPoints;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool isON;
    
    [Header("Debug")]
    [SerializeField] private float minDistance = 1f;
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position + OutputPoints[0],
            Vector2.right,
            Mathf.Infinity
        );
        print("Found an object - distance: " + hit.distance);
        if (isON)
        {
            Debug.DrawLine(transform.position + OutputPoints[0], hit.point, Color.green);
        }

        if (hit.rigidbody.gameObject.CompareTag("Crystal"))
        {
            // hit.rigidbody.gameObject.GetComponent<CrystalController>().OnLaserInteract(Vector2.left, isON);
        }
        Debug.DrawRay(transform.position + OutputPoints[0], Vector3.right, Color.red);
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position + OutputPoints[0], new Vector3(0.3f, 0.3f, 0.3f)); 
    }
}
