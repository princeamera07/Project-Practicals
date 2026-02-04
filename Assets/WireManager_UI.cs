using UnityEngine;
using UnityEngine.UI.Extensions; 
using System.Collections.Generic;

public class WireManager_UI : MonoBehaviour
{
    public static WireManager_UI Instance;

    [Header("Settings")]
    public GameObject wirePrefab;
    public RectTransform wireParent;
    
    [Header("Simulator Tools")]
    public bool isEraserOn = false; 
    public float deleteRadius = 25f; // How close you need to click (in pixels)

    [Header("Rope Physics")]
    [Range(5, 50)] public int resolution = 20; 
    [Range(0, 100)] public float sagAmount = 40f; 

    [Header("Validation Colors")]
    public Color normalColor = Color.black;
    public Color errorColor = Color.red;

    private UILineRenderer currentWire;
    private WirePoint currentStartPoint;
    
    // List to keep track of all active wires so we can check them for deletion
    private List<UILineRenderer> allWires = new List<UILineRenderer>();

    void Awake() => Instance = this;

    public void ToggleEraser()
    {
        isEraserOn = !isEraserOn;
        Debug.Log("Eraser Mode: " + isEraserOn);
    }

    private Vector2 GetLocalPosition(RectTransform target)
    {
        return wireParent.InverseTransformPoint(target.position);
    }

    // --- CLICK CHECKER (Put this in Update) ---
    private void CheckForEraserClick()
    {
        // Only run if Eraser is ON and Mouse is Clicked
        if (!isEraserOn || !Input.GetMouseButtonDown(0)) return;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(wireParent, Input.mousePosition, null, out localMousePos);

        UILineRenderer wireToDelete = null;
        float closestDist = deleteRadius; // Start with the max range

        // Check every wire to see if we clicked near it
        foreach (var wire in allWires)
        {
            if (wire == null) continue;

            if (IsPointNearWire(wire, localMousePos, deleteRadius))
            {
                wireToDelete = wire;
                break; // Found one! Stop looking.
            }
        }

        if (wireToDelete != null)
        {
            allWires.Remove(wireToDelete);
            Destroy(wireToDelete.gameObject);
            Debug.Log("Wire Deleted via Eraser!");
        }
    }

    // Math helper: Checks if mouse is near any part of the curved wire
    private bool IsPointNearWire(UILineRenderer wire, Vector2 point, float radius)
    {
        if (wire.Points == null) return false;

        for (int i = 0; i < wire.Points.Length - 1; i++)
        {
            Vector2 p1 = wire.Points[i];
            Vector2 p2 = wire.Points[i+1];
            
            // Check distance from point to the line segment p1-p2
            float dist = HandleUtility_DistancePointLine(point, p1, p2);
            if (dist < radius) return true;
        }
        return false;
    }

    // Standard Math function to get distance from point to line segment
    float HandleUtility_DistancePointLine(Vector2 p, Vector2 a, Vector2 b) {
        Vector2 pa = p - a, ba = b - a;
        float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
        return (pa - ba * h).magnitude;
    }

    public void StartWire(WirePoint startPoint)
    {
        if (isEraserOn) return; // Don't draw in eraser mode
        if (currentWire != null) return; 

        currentStartPoint = startPoint;

        GameObject wireObj = Instantiate(wirePrefab, wireParent);
        wireObj.transform.localPosition = Vector3.zero;
        wireObj.transform.localScale = Vector3.one;

        currentWire = wireObj.GetComponent<UILineRenderer>();
        currentWire.color = normalColor;
        currentWire.raycastTarget = false; // We don't need raycasts anymore!

        Vector2 pos = GetLocalPosition(startPoint.rectTransform);
        UpdateRope(pos, pos);
    }

    void Update()
    {
        // 1. Check for Deletion
        CheckForEraserClick();

        // 2. Handle Drawing
        if (currentWire != null)
        {
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(wireParent, Input.mousePosition, null, out localMousePos);
            Vector2 startPos = GetLocalPosition(currentStartPoint.rectTransform);

            UpdateRope(startPos, localMousePos);

            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(0))
            {
                // If we are drawing and click empty space -> Cancel
                // (Unless we just clicked a hole, which is handled by WirePoint)
                // We add a tiny delay or check if we are over a UI element usually, 
                // but your current logic relies on EndWire being called first.
                // Let's rely on EndWire vs Update race. 
                // A safe way: if button UP and we are NOT hovering a hole (WirePoint handles the success case).
                
                // For now, keep your simple cancel:
                 DestroyWire();
            }
        }
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

            // Add to our list so we can erase it later
            allWires.Add(currentWire);

            currentWire.SetAllDirty();
            currentWire = null;
            currentStartPoint = null;
        }
    }

    // ... [Keep CheckDualConnection and IsMatch exactly as they were] ...
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
}