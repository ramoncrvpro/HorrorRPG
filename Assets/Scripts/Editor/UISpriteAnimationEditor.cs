namespace HorrorRPG.Editor
{
using HorrorRPG.Presentation;


using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UISpriteAnimation))]
public class UISpriteAnimationEditor : Editor
{
    private SerializedProperty spritesProperty;
    private SerializedProperty fpsProperty;
    private SerializedProperty loopProperty;

    private void OnEnable()
    {
        spritesProperty = serializedObject.FindProperty("sprites");
        fpsProperty = serializedObject.FindProperty("fps");
        loopProperty = serializedObject.FindProperty("loop");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Animation Properties", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.PropertyField(spritesProperty, new GUIContent("Sprites"));
        EditorGUILayout.PropertyField(fpsProperty, new GUIContent("FPS"));
        EditorGUILayout.PropertyField(loopProperty, new GUIContent("Loop"));

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);

        UISpriteAnimation animation = (UISpriteAnimation)target;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Animation Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Frames: {animation.FrameCount}");
        if (animation.FrameCount > 0 && animation.FPS > 0)
        {
            float duration = animation.FrameCount / animation.FPS;
            EditorGUILayout.LabelField($"Duration: {duration:F2}s");
        }
        EditorGUILayout.EndVertical();
    }
}


}
