using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimatorManager))]
public class AnimatorManagerInspector : Editor {

    SerializedProperty numOfAnimations;
    SerializedProperty foldouts;
    SerializedProperty names;
    SerializedProperty animators;

    private void OnEnable() {
        numOfAnimations = serializedObject.FindProperty("numOfAnimations");
        foldouts = serializedObject.FindProperty("foldouts");
        names = serializedObject.FindProperty("names");
        animators = serializedObject.FindProperty("animators");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        for (int i = 0; i < numOfAnimations.intValue; i++) {
            EditorGUILayout.BeginHorizontal();
            
            foldouts.GetArrayElementAtIndex(i).boolValue = EditorGUILayout.Foldout(foldouts.GetArrayElementAtIndex(i).boolValue, "Animator " + (i+1), true);
            bool minusButton = GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            if (minusButton) {
                numOfAnimations.intValue--;
                foldouts.DeleteArrayElementAtIndex(i);
                names.DeleteArrayElementAtIndex(i);
            } else {
                if (foldouts.GetArrayElementAtIndex(i).boolValue) {
                    names.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField("Name", names.GetArrayElementAtIndex(i).stringValue);
                    animators.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("Animator", animators.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Animator), true);
                }
            }
        }
        EditorGUILayout.Space();

        if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) {
            numOfAnimations.intValue++;
            foldouts.InsertArrayElementAtIndex(numOfAnimations.intValue - 1);
            foldouts.GetArrayElementAtIndex(numOfAnimations.intValue - 1).boolValue = true;
            names.InsertArrayElementAtIndex(numOfAnimations.intValue - 1);
            names.GetArrayElementAtIndex(numOfAnimations.intValue - 1).stringValue = "Animator" + numOfAnimations.intValue;
            animators.InsertArrayElementAtIndex(numOfAnimations.intValue - 1);

        }
        serializedObject.ApplyModifiedProperties();
    }
}
