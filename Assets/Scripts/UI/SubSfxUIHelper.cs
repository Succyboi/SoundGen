using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubSfxUIHelper : MonoBehaviour
{
    public Text savedText;
    public float savedTextVisibilityTime;

    private LTROController input;
    private SubSfxGen sfx;

    private void Start()
    {
        input = FindObjectOfType<LTROController>();
        sfx = FindObjectOfType<SubSfxGen>();
    }

    private void Update()
    {
        //start to play sound
        if (input.bDown)
        {
            sfx.PlaySfx();
        }
    }

    public void Save()
    {
        StartCoroutine(SaveRoutine());
    }

    private IEnumerator SaveRoutine()
    {
        savedText.transform.parent.gameObject.SetActive(true);

        switch (Application.platform)
        {
            //desktop
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.OSXPlayer:
                //notification
                bool saveSuccessful = sfx.Save();
                savedText.text = saveSuccessful ? "Saved to downloads succesfully!" : "Could not save. Sorry...";
                NotificationSounds.instance.PlaySound(saveSuccessful ? NotificationSound.Good : NotificationSound.Bad);
                break;

            //web
            case RuntimePlatform.WebGLPlayer:
                //start download and wait for completion
                sfx.Download();
                yield return new WaitUntil(() => sfx.downloadSuccesful);
                savedText.text = "Saved to downloads succesfully!";
                NotificationSounds.instance.PlaySound(NotificationSound.Good);
                break;

            //default to false
            default:
                break;
        }


        yield return new WaitForSeconds(savedTextVisibilityTime);

        savedText.transform.parent.gameObject.SetActive(false);

        yield return null;
    }
}
