using UnityEngine;

public class DragController : MonoBehaviour
{
    private string otherLayer = "Boundary";
    private MeshRenderer[] bounds;
    private void Start()
    {
        transform.localRotation = MulMgr.CharRot();
        if (MulMgr.GetPl() == 1)
        {
            otherLayer += "Blue";
            bounds = GameMgr.GetBlueBounds();
        }
        else
        {
            otherLayer += "Red";
            bounds = GameMgr.GetRedBounds();
        }

        foreach (MeshRenderer mesh in bounds)
        {
            if (mesh == null) continue;
            mesh.enabled = true; 
        }    
    }
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("WalkAble")))
        {
            Vector3 groundPosition = hit.point;
            groundPosition.y = -1f;

            float snapValue = GameMgr.GetSnap();
            groundPosition.x = SnapToClosest(groundPosition.x, snapValue);
            groundPosition.z = SnapToClosest(groundPosition.z, snapValue);  // Assuming you want to snap on z-axis as well

            Collider[] hitColliders = Physics.OverlapSphere(groundPosition, 0.5f, LayerMask.GetMask(otherLayer, "Boundary"));
            if (hitColliders.Length != 0)
            {
                Vector3 lastValidPosition = transform.position;

                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("SideBounds"))
                    {
                        groundPosition.x = lastValidPosition.x;
                    }
                    else if (hitCollider.CompareTag("VerBounds"))
                    {
                        groundPosition.z = lastValidPosition.z;
                    }
                }
            }

            transform.position = groundPosition;
        }
    }
    public static float SnapToClosest(float position, float snapValue)
    {
        float lowerSnapPoint = Mathf.Floor(position / snapValue) * snapValue;
        float higherSnapPoint = Mathf.Ceil(position / snapValue) * snapValue;

        float lowerDistance = Mathf.Abs(position - lowerSnapPoint);
        float higherDistance = Mathf.Abs(position - higherSnapPoint);

        return lowerDistance < higherDistance ? lowerSnapPoint : higherSnapPoint;
    }
    private void OnDestroy()
    {
        foreach (MeshRenderer mesh in bounds)
        {
            if (mesh == null) continue;
            mesh.enabled = false; 
        }        
    }
}