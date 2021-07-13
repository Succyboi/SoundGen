using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroSequence : MonoBehaviour
{
    [Header("General")]
    public float timeBetweenSlides;

    [Header("Start Menu")]
    public float startBlinkSpeed;
    public GameObject startText;
    public GameObject startMenu;

    [Header("Loading Screen")]
    public int loadingBarWidth;
    public float loadingBarDelay;
    public Text loadingBar;
    public GameObject LoadingScreen;

    [Header("Misc References")]
    public GameObject[] pregameSlides;
    public GameObject afterIntro;
    public LTROController input;
    public SubSfxGen sfx;

    private void Start()
    {
        //before everything
        afterIntro.SetActive(false);

        //start routine
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        //wait before startup
        yield return new WaitForSeconds(timeBetweenSlides);
        sfx.GlitchPluck();

        //pregame slides
        for (int p = 0; p < pregameSlides.Length; p++)
        {
            //enable slide
            pregameSlides[Mathf.Clamp(p - 1, 0, pregameSlides.Length - 1)].SetActive(false);
            pregameSlides[p].SetActive(true);

            //wait
            yield return new WaitForSeconds(timeBetweenSlides);
            sfx.GlitchPluck();
        }

        //start menu
        pregameSlides[pregameSlides.Length - 1].SetActive(false);
        startMenu.SetActive(true);

        //wait until start is pressed
        while (!(input.aDown || input.bDown || input.startDown))
        {
            startText.SetActive(Time.time % startBlinkSpeed / startBlinkSpeed > 0.5f);

            yield return new WaitForEndOfFrame();
        }
        sfx.GlitchPluck();

        //loading screen
        startMenu.SetActive(false);
        LoadingScreen.SetActive(true);

        for(int x = 0; x < loadingBarWidth; x++)
        {
            loadingBar.text = "[";

            for(int l = 0; l < loadingBarWidth; l++)
            {
                loadingBar.text += x >= l ? "#" : "_";
            }

            loadingBar.text += "]";

            yield return new WaitForSeconds(loadingBarDelay);
        }

        //finally enter the application
        LoadingScreen.SetActive(false);
        afterIntro.SetActive(true);

        yield return null;
    }
}
