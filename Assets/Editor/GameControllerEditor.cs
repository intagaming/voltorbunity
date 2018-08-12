using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameController))]
public class GameControllerEditor : Editor {

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogBox"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("scoreDialogBox"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("renderManager"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pointerController"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gameObjectReferences"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteReferences"), true);
        serializedObject.ApplyModifiedProperties();

    }
}
