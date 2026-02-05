using UnityEngine;
using UnityEngine.UI.Extensions; 
using System.Collections.Generic;

public class WireManager_UI : MonoBehaviour
{
    public static WireManager_UI Instance;

    [Header("Settings")]
    public GameObject wirePrefab;
    public RectTransform wireParent;
    
    [Header("Magnet Feature")]
    public float snapRadius = 45f; // How close (pixels) to snap
    
    [Header("Simulator Tools")]
    public bool isEraserOn = false; 
    public float deleteRadius = 25f; 

    [Header("Rope Physics")]
    [Range(5, 50)] public int resolution = 20; 
    [Range(0, 100)] public float sagAmount = 40f; 

    [Header("Validation Colors")]
    public Color normalColor = Color.black;
    public Color errorColor = Color.red;

    private UILineRenderer currentWire;
    private WirePoint currentStartPoint;
    
    // Lists for optimization
    private List<UILineRenderer> allWires = new List<UILineRenderer>();
    private WirePoint[] allHoles; // Cache all holes for snapping

    void Awake() 
    {
        Instance = this;
        // FIND ALL HOLES ONCE AT START (Efficiency)
        allHoles = FindObjectsOfType<WirePoint>();
    }

    private Vector2 GetLocalPosition(RectTransform target)
    {
        return wireParent.InverseTransformPoint(target.position);
    }

    public void StartWire(WirePoint startPoint)
    {
        if (isEraserOn) return; 
        if (currentWire != null) return; 

        currentStartPoint = startPoint;

        GameObject wireObj = Instantiate(wirePrefab, wireParent);
        wireObj.transform.localPosition = Vector3.zero;
        wireObj.transform.localScale = Vector3.one;

        currentWire = wireObj.GetComponent<UILineRenderer>();
        currentWire.color = normalColor;
        currentWire.raycastTarget = false; 

        Vector2 pos = GetLocalPosition(startPoint.rectTransform);
        UpdateRope(pos, pos);
    }

    void Update()
    {
        // 1. Eraser Logic
        CheckForEraserClick();

        // 2. Drawing Logic
        if (currentWire != null)
        {
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(wireParent, Input.mousePosition, null, out localMousePos);
            Vector2 startPos = GetLocalPosition(currentStartPoint.rectTransform);

            UpdateRope(startPos, localMousePos);

            // --- MOUSE RELEASED: TRY TO SNAP ---
            if (Input.GetMouseButtonUp(0))
            {
                // Try to find a hole near the mouse
                WirePoint nearbyHole = FindClosestHole(localMousePos);

                if (nearbyHole != null)
                {
                    // Success! We found a hole nearby
                    EndWire(nearbyHole);
                }
                else
                {
                    // Too far from any hole, delete it
                    DestroyWire();
                }
            }
            
            // Cancel with Right Click
            if (Input.GetMouseButtonDown(1))
            {
                DestroyWire();
            }
        }
    }

    // --- NEW: MAGNET LOGIC ---
    WirePoint FindClosestHole(Vector2 mouseLocalPos)
    {
        WirePoint bestHole = null;
        float closestDist = snapRadius; // Start with max range

        foreach (var hole in allHoles)
        {
            if (hole == null) continue;
            if (hole == currentStartPoint) continue; // Don't snap to self

            // Convert hole position to the same local space as mouse
            Vector2 holeLocalPos = GetLocalPosition(hole.rectTransform);
            
            float dist = Vector2.Distance(mouseLocalPos, holeLocalPos);

            if (dist < closestDist)
            {
                closestDist = dist;
                bestHole = hole;
            }
        }
        return bestHole;
    }

    public void EndWire(WirePoint endPoint)
    {
        if (currentWire == null) return;

        if (endPoint == currentStartPoint)
        {
            DestroyWire();
        }
        else
        {
            Vector2 startPos = GetLocalPosition(currentStartPoint.rectTransform);
            Vector2 endPos = GetLocalPosition(endPoint.rectTransform);
            UpdateRope(startPos, endPos);

            if (CheckDualConnection(currentStartPoint, endPoint))
                currentWire.color = normalColor;
            else
                currentWire.color = errorColor;

            allWires.Add(currentWire);

            currentWire.SetAllDirty();
            currentWire = null;
            currentStartPoint = null;
        }
    }

    // ... [Eraser Logic] ...
    private void CheckForEraserClick()
    {
        if (!isEraserOn || !Input.GetMouseButtonDown(0)) return;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(wireParent, Input.mousePosition, null, out localMousePos);

        UILineRenderer wireToDelete = null;
        float closestDist = deleteRadius; 

        foreach (var wire in allWires)
        {
            if (wire == null) continue;
            if (IsPointNearWire(wire, localMousePos, deleteRadius))
            {
                wireToDelete = wire;
                break; 
            }
        }

        if (wireToDelete != null)
        {
            allWires.Remove(wireToDelete);
            Destroy(wireToDelete.gameObject);
        }
    }

    private bool IsPointNearWire(UILineRenderer wire, Vector2 point, float radius)
    {
        if (wire.Points == null) return false;
        for (int i = 0; i < wire.Points.Length - 1; i++)
        {
            float dist = HandleUtility_DistancePointLine(point, wire.Points[i], wire.Points[i+1]);
            if (dist < radius) return true;
        }
        return false;
    }

    float HandleUtility_DistancePointLine(Vector2 p, Vector2 a, Vector2 b) {
        Vector2 pa = p - a, ba = b - a;
        float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
        return (pa - ba * h).magnitude;
    }

    // ... [Connection Checks] ...
    private bool CheckDualConnection(WirePoint start, WirePoint end)
    {
        if (IsMatch(start.pointID_1, start.targetID_1, end.pointID_1, end.targetID_1)) return true;
        if (IsMatch(start.pointID_1, start.targetID_1, end.pointID_2, end.targetID_2)) return true;
        if (IsMatch(start.pointID_2, start.targetID_2, end.pointID_1, end.targetID_1)) return true;
        if (IsMatch(start.pointID_2, start.targetID_2, end.pointID_2, end.targetID_2)) return true;
        return false;
    }

    private bool IsMatch(string pID, string tID, string other_pID, string other_tID)
    {
        if (string.IsNullOrEmpty(pID) && string.IsNullOrEmpty(other_pID)) return false;
        if (!string.IsNullOrEmpty(tID) && other_pID != tID) return false;
        if (!string.IsNullOrEmpty(other_tID) && pID != other_tID) return false;
        return true;
    }

    private void DestroyWire()
    {
        if (currentWire != null) Destroy(currentWire.gameObject);
        currentWire = null;
        currentStartPoint = null;
    }

    private void UpdateRope(Vector2 p0, Vector2 p2)
    {
        if (currentWire == null) return;
        Vector2 midPoint = (p0 + p2) / 2f;
        float distance = Vector2.Distance(p0, p2);
        Vector2 p1 = midPoint + new Vector2(0, -Mathf.Abs(sagAmount + (distance * 0.1f)));
        Vector2[] points = new Vector2[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            points[i] = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        }
        currentWire.Points = points;
        currentWire.SetAllDirty();
    }
    
    public void ToggleEraser() { isEraserOn = !isEraserOn; }
    
    public void ClearAllWires()
    {
        foreach(var wire in allWires) { if(wire != null) Destroy(wire.gameObject); }
        allWires.Clear();
    }
}