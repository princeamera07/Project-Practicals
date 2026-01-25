using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject startMenu;
    public GameObject optionsMenu;
    public GameObject chooseSubjectMenu;
    public GameObject choosePracticalMenu;

    void Start()
    {
        ShowStartMenu();
    }

    void DisableAll()
    {
        startMenu.SetActive(false);
        optionsMenu.SetActive(false);
        chooseSubjectMenu.SetActive(false);
        choosePracticalMenu.SetActive(false);
    }

    public void ShowStartMenu()
    {
        DisableAll();
        startMenu.SetActive(true);
    }

    public void ShowOptions()
    {
        DisableAll();
        optionsMenu.SetActive(true);
    }

    public void ShowChooseSubject()
    {
        DisableAll();
        chooseSubjectMenu.SetActive(true);
    }

    public void ShowChoosePractical()
    {
        DisableAll();
        choosePracticalMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}
