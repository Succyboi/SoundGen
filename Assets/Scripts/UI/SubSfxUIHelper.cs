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

    public void Save()
    {
        StartCoroutine(SaveRoutine());
    }

    private IEnumerator SaveRoutine()
    {
        savedText.transform.parent.gameObject.SetActive(true);

        //notification
        bool saveSuccessful = sfx.Save();
        savedText.text = saveSuccessful ? "Saved to downloads succesfully!" : "Could not save. Sorry...";
        NotificationSounds.instance.PlaySound(saveSuccessful ? NotificationSound.Good : NotificationSound.Bad);

        yield return new WaitForSeconds(savedTextVisibilityTime);

        savedText.transform.parent.gameObject.SetActive(false);

        yield return null;
    }
}
