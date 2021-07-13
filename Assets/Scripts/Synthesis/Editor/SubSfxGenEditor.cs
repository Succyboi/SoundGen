using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SubSfxGen))]
public class SubSfxGenEditor : Editor
{
    private SubSfxGen component;
    private Texture2D clipPreview;

    private void OnEnable()
    {
        component = ((SubSfxGen)target);
        component.source = component.GetComponent<AudioSource>();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Glitch"))
        {
            component.Glitch();
        }
        if (GUILayout.Button("Glitch Pluck"))
        {
            component.GlitchPluck();
        }
        if (GUILayout.Button("Random Explosion"))
        {
            component.RandomExplosion();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Generate"))
        {
            component.PlaySfx();
            clipPreview = AudioTools.PaintWaveformSpectrum(component.source.clip, Mathf.RoundToInt(GUILayoutUtility.GetLastRect().size.x), Mathf.RoundToInt(GUI.skin.label.lineHeight * 2));
        }

        //get waveform
        if (component.source.clip != null && clipPreview == null)
        {
            clipPreview = AudioTools.PaintWaveformSpectrum(component.source.clip, Mathf.RoundToInt(GUILayoutUtility.GetLastRect().size.x), Mathf.RoundToInt(GUI.skin.label.lineHeight * 2));
        }

        if (clipPreview != null)
        {
            //GUILayout.Label(clipPreview);
        }

        base.OnInspectorGUI();
    }
}
 
public static class EditorSFX
{

    public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
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

        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "StopAllPreviewClips",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { },
            null
        );

        Debug.Log(method);
        method.Invoke(
            null,
            new object[] { }
        );
    }
}