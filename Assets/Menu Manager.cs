using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject HowToMenu;
    public GameObject ToolsMenu;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnDisable()
    {
        HowToMenu.SetActive(false);  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
