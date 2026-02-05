using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using UnityEngine.EventSystems;

public class LabController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform coarseKnob;    
    public RectTransform fineKnob;      
    public TextMeshProUGUI voltmeterText;
    public TextMeshProUGUI ammeterText;
    
    [Header("Power Switch")]
    public Image switchImage;           
    public Sprite switchOnPicture;      
    public Sprite switchOffPicture;     
    public bool isPowerOn = false;     

    [Header("Visual Fixes")]
    public float fineKnobStartingOffset = 0f; 

    [Header("Rotation Limits")]
    public float coarseMaxAngle = 360f; 
    public float fineMaxAngle = 180f;   

    [Header("Source Settings")]
    public float coarseMaxVoltage = 10f; 
    public float fineMaxVoltage = 1.0f;  

    [Header("Circuit Physics (Omega ETB-68)")]
    public float seriesResistance = 10f; 
    public float diodeThreshold = 0.6f;  
    public float diodeInternalR = 0.5f; 

    private float currentCoarseAngle = 0f;
    private float currentFineAngle = 0f;
    private RectTransform currentKnobDragging = null;
    private Vector2 lastMousePosition;

    void Start()
    {
        if (fineKnob != null) fineKnob.localEulerAngles = new Vector3(0, 0, fineKnobStartingOffset);
        UpdateSwitchVisuals();
        ClearScreens();
    }

    void Update()
    {
        HandleInput();
        
        if (isPowerOn)
        {
            CalculateForwardBias();
        }
        else
        {
            ClearScreens();
        }
    }

    public void TogglePower()
    {
        isPowerOn = !isPowerOn; 
        UpdateSwitchVisuals();  
    }

    void CalculateForwardBias()
    {
        // --- 1. SAFETY CHECK: RED WIRES? ---
        if (WireManager_UI.Instance.HasBadWires())
        {
            if (voltmeterText != null) voltmeterText.text = "Err";
            if (ammeterText != null) ammeterText.text = "Err";
            return; // Stop physics!
        }

        // --- 2. Normal Physics ---
        float coarsePercent = currentCoarseAngle / coarseMaxAngle;
        float finePercent = currentFineAngle / fineMaxAngle;
        float sourceVoltage = (coarsePercent * coarseMaxVoltage) + (finePercent * fineMaxVoltage);

        float displayedVolts = 0f;
        float displayedAmps = 0f;

        if (sourceVoltage <= diodeThreshold)
        {
            // Below Knee: Diode is Open. 
            // Voltmeter sees Source, Ammeter sees 0.
            displayedVolts = sourceVoltage;
            displayedAmps = 0f;
        }
        else
        {
            // Forward Bias: Diode is Closed.
            // I = (V_s - V_d) / R
            float totalResistance = seriesResistance + diodeInternalR;
            float current = (sourceVoltage - diodeThreshold) / totalResistance;
            
            // Voltmeter across diode (Knee + Drop)
            float diodeVoltageDrop = diodeThreshold + (current * diodeInternalR);

            displayedVolts = diodeVoltageDrop;
            displayedAmps = current;
        }

        UpdateScreens(displayedVolts, displayedAmps);
    }

    void UpdateScreens(float volts, float amps)
    {
        if (voltmeterText != null)
            voltmeterText.text = volts.ToString("F2"); 
            
        if (ammeterText != null)
        {
            float displaymA = amps * 1000f; 
            ammeterText.text = displaymA.ToString("F2"); 
        }
    }
    
    void ClearScreens()
    {
        if (voltmeterText != null) voltmeterText.text = "";
        if (ammeterText != null) ammeterText.text = "";
    }

    void UpdateSwitchVisuals()
    {
        if (switchImage != null && switchOnPicture != null && switchOffPicture != null)
        {
            switchImage.sprite = isPowerOn ? switchOnPicture : switchOffPicture;
        }
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
        if (Input.GetMouseButtonUp(0)) currentKnobDragging = null;

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
}