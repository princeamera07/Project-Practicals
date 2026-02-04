using UnityEngine;
using UnityEngine.UI.Extensions;
using System.Collections.Generic;

public class WireManager_UI : MonoBehaviour
{
    public static WireManager_UI Instance;

    [Header("Settings")]
    public GameObject wirePrefab;
    public RectTransform wireParent;
    
    [Header("Rope Settings")]
    [Range(5, 50)] public int resolution = 20; // How "smooth" the curve is
    [Range(0, 100)] public float sagAmount = 40f; // How much the wire droops

    private UILineRenderer currentWire;
    private WirePoint currentStartPoint;

    void Awake() => Instance = this;

    // Helper: Converts World Position to Local Canvas Position
    private Vector2 GetLocalPosition(RectTransform target)
    {
        return wireParent.InverseTransformPoint(target.position);
    }

    public void StartWire(WirePoint startPoint)
    {
        if (currentWire != null) return; // Only 1 wire at a time

        currentStartPoint = startPoint;

        GameObject wireObj = Instantiate(wirePrefab, wireParent);
        wireObj.transform.localPosition = Vector3.zero;
        wireObj.transform.localScale = Vector3.one;

        currentWire = wireObj.GetComponent<UILineRenderer>();
        
        // Initial Draw
        UpdateRope(GetLocalPosition(startPoint.rectTransform), GetLocalPosition(startPoint.rectTransform));
    }

    void Update()
    {
        if (currentWire != null)
        {
            // Get Mouse Position in Local Space
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(wireParent, Input.mousePosition, null, out localMousePos);
            
            // Get Start Position
            Vector2 startPos = GetLocalPosition(currentStartPoint.rectTransform);

            // Draw the curve from Start Hole to Mouse
            UpdateRope(startPos, localMousePos);

            // Cancel with Right Click or if released in empty space
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(0))
            {
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
            // Snap to the target hole
            Vector2 startPos = GetLocalPosition(currentStartPoint.rectTransform);
            Vector2 endPos = GetLocalPosition(endPoint.rectTransform);
            
            UpdateRope(startPos, endPos);

            // Seal the wire
            currentWire = null;
            currentStartPoint = null;
        }
    }

    private void DestroyWire()
    {
        if (currentWire != null) Destroy(currentWire.gameObject);
        currentWire = null;
        currentStartPoint = null;
    }

    // THE MAGIC: Generates the curved points
    private void UpdateRope(Vector2 p0, Vector2 p2)
    {
        if (currentWire == null) return;

        // 1. Calculate the Middle Control Point (P1)
        // We find the center, then drop it down by 'sagAmount' to create gravity
        Vector2 midPoint = (p0 + p2) / 2f;
        
        // Make sag depend on distance (longer wires sag more) causes cleaner look
        float distance = Vector2.Distance(p0, p2);
        Vector2 p1 = midPoint + new Vector2(0, -Mathf.Abs(sagAmount + (distance * 0.1f)));

        // 2. Generate points along the curve
        Vector2[] points = new Vector2[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1); // t goes from 0 to 1
            points[i] = CalculateBezierPoint(t, p0, p1, p2);
        }

        // 3. Apply to UI Line Renderer
        currentWire.Points = points;
        currentWire.SetAllDirty();
    }

    // Standard Quadratic Bezier Formula
    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector2 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return p;
    }
}