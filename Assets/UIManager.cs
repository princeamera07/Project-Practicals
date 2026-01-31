using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // Menus
    public GameObject startMenu;
    public GameObject optionsMenu;
    public GameObject chooseSubjectMenu;
    public GameObject choosePracticalMenu;

    // Coming Soon UI
    public TextMeshProUGUI comingSoonText;
    public float comingSoonMoveUp = 100f;
    public float comingSoonDuration = 1f;

    private Vector3 comingSoonStartPos;

    void Start()
    {
        ShowStartMenu();

        // Setup Coming Soon text
        if (comingSoonText != null)
        {
            comingSoonStartPos = comingSoonText.transform.position;
            comingSoonText.gameObject.SetActive(false);
        }
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

    // LEVEL BUTTON LOGIC
    public void OnLevelButtonClicked(int levelNumber)
    {
        if (levelNumber == 1)
        {
            SceneManager.LoadScene("Level1");
        }
        else if (levelNumber == 2)
        {
            SceneManager.LoadScene("Level2");
        }
        else
        {
            ShowComingSoon();
        }
    }

    // COMING SOON ANIMATION
    void ShowComingSoon()
    {
        if (comingSoonText == null) return;

        StopAllCoroutines();
        comingSoonText.gameObject.SetActive(true);
        comingSoonText.transform.position = comingSoonStartPos;
        StartCoroutine(ComingSoonAnimation());
    }

    IEnumerator ComingSoonAnimation()
    {
        float t = 0f;
        Color c = comingSoonText.color;
        c.a = 1f;
        comingSoonText.color = c;

        while (t < comingSoonDuration)
        {
            t += Time.deltaTime;
            float p = t / comingSoonDuration;

            comingSoonText.transform.position =
                comingSoonStartPos + Vector3.up * comingSoonMoveUp * p;

            c.a = 1f - p;
            comingSoonText.color = c;

            yield return null;
        }

        comingSoonText.gameObject.SetActive(false);
    }
}
