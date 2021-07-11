using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SubSfxGen))]
public class SubSfxGenEditor : Editor
{
    private SubSfxGen component;
    private AudioClip clip;

    private void OnEnable()
    {
        component = ((SubSfxGen)target);
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate"))
        {
            clip = component.GenerateSfx();

            if(clip != null)
            {
                PublicAudioUtil.PlayClip(clip);
                Selection.activeObject = clip;
            }
        }

        base.OnInspectorGUI();
    }
}

public static class PublicAudioUtil
{
    public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

        System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null
        );

        Debug.Log(method);
        method.Invoke(
            null,
            new object[] { clip, startSample, loop }
        );
    }

    public static void StopAllClips()
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

        System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "StopAllPreviewClips",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new System.Type[] { },
            null
        );

        Debug.Log(method);
        method.Invoke(
            null,
            new object[] { }
        );
    }
}