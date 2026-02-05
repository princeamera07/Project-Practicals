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
    
    [Header("Power Switch Images")]
    public Image switchImage;           
    public Sprite switchOnPicture;      
    public Sprite switchOffPicture;     
    
    public bool isPowerOn = false;     // Starts OFF

    [Header("Visual Fixes")]
    public float fineKnobStartingOffset = 0f; 

    [Header("Rotation Limits")]
    public float coarseMaxAngle = 360f; 
    public float fineMaxAngle = 180f;   

    [Header("Voltage Settings")]
    public float coarseMaxVoltage = 10f; 
    public float fineMaxVoltage = 1.0f;  

    [Header("Circuit Physics (Omega ETB-68)")]
    public float seriesResistance = 10f; 
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
        
        // Initialize Visuals
        UpdateSwitchVisuals();
        
        // Force screens to update immediately
        if (isPowerOn) CalculateCircuit();
        else ClearScreens();
    }

    void Update()
    {
        HandleInput();
        
        if (isPowerOn)
        {
            CalculateCircuit();
        }
        else
        {
            // If Power is OFF, make screens invisible
            ClearScreens();
        }
    }

    public void TogglePower()
    {
        isPowerOn = !isPowerOn; 
        UpdateSwitchVisuals();  
    }

    void UpdateSwitchVisuals()
    {
        if (switchImage != null && switchOnPicture != null && switchOffPicture != null)
        {
            switchImage.sprite = isPowerOn ? switchOnPicture : switchOffPicture;
        }
    }

    // NEW HELPER: Makes text empty
    void ClearScreens()
    {
        if (voltmeterText != null) voltmeterText.text = "";
        if (ammeterText != null) ammeterText.text = "";
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

        UpdateScreens(totalVoltage, currentAmps);
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
} 