using UnityEngine;
using UnityEngine.UI;

public class EraserButton : MonoBehaviour
{
    private Button myButton;
    private Image myImage;

    void Awake()
    {
        myButton = GetComponent<Button>();
        myImage = GetComponent<Image>();
        myButton.onClick.AddListener(ToggleMode);
    }

    void ToggleMode()
    {
        WireManager_UI.Instance.ToggleEraser();

        if (WireManager_UI.Instance.isEraserOn)
            myImage.color = Color.red; 
        else
            myImage.color = Color.white; 
    }
}