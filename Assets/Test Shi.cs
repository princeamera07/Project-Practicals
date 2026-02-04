using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TestShi : MonoBehaviour
{
    public TextMeshProUGUI comingSoonText;
    public float comingSoonMoveUp = 100f;
    public float comingSoonDuration = 1f;
    private Vector3 comingSoonStartPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GOStartMenu()
    {
        SceneManager.LoadScene("Start");
    }
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
