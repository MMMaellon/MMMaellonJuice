#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using System.IO;

namespace MMMaellon.Juice
{
    [InitializeOnLoad]
    public class AutoSetup : IVRCSDKBuildRequestedCallback
    {

        [MenuItem("MMMaellon/Juice/Manually Trigger AutoSetup")]
        static void TriggerAutoSetup()
        {
            Setup();
        }
        public static bool Setup()
        {
            JuiceContainer[] containers = GameObject.FindObjectsOfType<JuiceContainer>();
            for (int i = 0; i < containers.Length; i++)
            {
                SerializedObject serialized = new SerializedObject(containers[i]);
                serialized.FindProperty("pours").ClearArray();
                serialized.ApplyModifiedProperties();
            }
            JuicePour[] pours = GameObject.FindObjectsOfType<JuicePour>();
            for (int i = 0; i < pours.Length; i++)
            {
                if (!Utilities.IsValid(pours[i].waterSource))
                {
                    continue;
                }
                SerializedObject serialized = new SerializedObject(pours[i].waterSource);
                int index = serialized.FindProperty("pours").arraySize;
                serialized.FindProperty("pours").InsertArrayElementAtIndex(index);
                serialized.FindProperty("pours").GetArrayElementAtIndex(index).objectReferenceValue = pours[i];
                serialized.ApplyModifiedProperties();
            }

            return true;
        }

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;
            Setup();
        }

        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return Setup();
        }
    }
}
#endif