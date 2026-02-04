using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class LabController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform coarseKnob;    
    public RectTransform fineKnob;      
    public TextMeshProUGUI voltmeterText;
    public TextMeshProUGUI ammeterText;

    [Header("Visual Fixes")]
    public float fineKnobStartingOffset = 0f; 

    [Header("Rotation Limits")]
    public float coarseMaxAngle = 360f; 
    public float fineMaxAngle = 180f;   

    [Header("Voltage Settings")]
    public float coarseMaxVoltage = 10f; 
    public float fineMaxVoltage = 1.0f;  

    [Header("Circuit Physics")]
    public float seriesResistance = 100f;
    public float diodeCutIn = 0.7f;

    private float currentCoarseAngle = 0f;
    private float currentFineAngle = 0f;
    
    private RectTransform currentKnobDragging = null;
    private Vector2 lastMousePosition;

    void Start()
    {
        if (fineKnob != null)
        {
            fineKnob.localEulerAngles = new Vector3(0, 0, fineKnobStartingOffset);
        }
    }

    void Update()
    {
        HandleInput();
        CalculateCircuit();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;

            if (RectTransformUtility.RectangleContainsScreenPoint(coarseKnob, Input.mousePosition))
                currentKnobDragging = coarseKnob;
            else if (RectTransformUtility.RectangleContainsScreenPoint(fineKnob, Input.mousePosition))
                currentKnobDragging = fineKnob;
        }

        if (Input.GetMouseButtonUp(0))
        {
            currentKnobDragging = null;
        }

        if (currentKnobDragging != null)
        {
            float mouseDelta = Input.mousePosition.x - lastMousePosition.x;
            lastMousePosition = Input.mousePosition;

            if (currentKnobDragging == coarseKnob)
            {
                currentCoarseAngle += mouseDelta * 2f; 
                currentCoarseAngle = Mathf.Clamp(currentCoarseAngle, 0f, coarseMaxAngle);
                coarseKnob.localEulerAngles = new Vector3(0, 0, -currentCoarseAngle);
            }
            else if (currentKnobDragging == fineKnob)
            {
                currentFineAngle += mouseDelta * 2f;
                currentFineAngle = Mathf.Clamp(currentFineAngle, 0f, fineMaxAngle);
                
                float visualAngle = -currentFineAngle + fineKnobStartingOffset;
                fineKnob.localEulerAngles = new Vector3(0, 0, visualAngle);
            }
        }
    }

    void CalculateCircuit()
    {
        float coarsePercent = currentCoarseAngle / coarseMaxAngle;
        float finePercent = currentFineAngle / fineMaxAngle;

        float totalVoltage = (coarsePercent * coarseMaxVoltage) + (finePercent * fineMaxVoltage);

        float currentAmps = 0f;
        if (totalVoltage > diodeCutIn)
        {
            currentAmps = (totalVoltage - diodeCutIn) / seriesResistance;
        }

        // --- UPDATED: BOTH SHOW 0.00 ---
        if (voltmeterText != null)
            voltmeterText.text = totalVoltage.ToString("F2"); // E.g. "5.00"
            
        if (ammeterText != null)
        {
            float displaymA = currentAmps * 1000f; 
            ammeterText.text = displaymA.ToString("F2"); // CHANGED FROM F1 TO F2
        }
    }
}