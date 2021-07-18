using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroSequence : MonoBehaviour
{
    [Header("General")]
    public float timeBetweenSlides;

    [Header("Start Menu")]
    public Color[] colors;
    public float startBlinkSpeed;
    public Text startText;
    public GameObject startMenu;

    [Header("Loading Screen")]
    public int loadingBarWidth;
    public float loadingBarDelay;
    public Text loadingBar;
    public GameObject LoadingScreen;

    [Header("Misc References")]
    public GameObject[] pregameSlides;
    public GameObject afterIntro;
    public Selectable firstSelectable;
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
        sfx.Glitch();

        //pregame slides
        for (int p = 0; p < pregameSlides.Length; p++)
        {
            //enable slide
            pregameSlides[Mathf.Clamp(p - 1, 0, pregameSlides.Length - 1)].SetActive(false);
            pregameSlides[p].SetActive(true);

            //wait
            yield return new WaitForSeconds(timeBetweenSlides);
            sfx.Glitch();
        }

        //start menu
        pregameSlides[pregameSlides.Length - 1].SetActive(false);
        startMenu.SetActive(true);

        //wait until start is pressed
        bool lastToggle = true;
        while (!(input.aDown || input.bDown || input.startDown || UnityEngine.InputSystem.Mouse.current.leftButton.isPressed))
        {
            if(lastToggle != Time.time % startBlinkSpeed / startBlinkSpeed > 0.5f)
            {
                lastToggle = Time.time % startBlinkSpeed / startBlinkSpeed > 0.5f;
                startText.color = colors[Random.Range(0, colors.Length)];
            }
            startText.gameObject.SetActive(Time.time % startBlinkSpeed / startBlinkSpeed > 0.5f);

            yield return new WaitForEndOfFrame();
        }
        sfx.Glitch();

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
        firstSelectable.Select();
        NotificationSounds.instance.PlaySound(NotificationSound.Good);

        yield return null;
    }
}
