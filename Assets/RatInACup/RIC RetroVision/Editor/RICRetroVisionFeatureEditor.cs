using UnityEditor;
using UnityEngine;
using RatInACup.RICRetroVision;

namespace RatInACup.RICRetroVision.Editor
{
    /// <summary>Provides preset application and reset actions for the RIC RetroVision feature Inspector.</summary>
    [CustomEditor(typeof(VHSRendererFeature))]
    public sealed class RICRetroVisionFeatureEditor : UnityEditor.Editor
    {
        private SerializedProperty presetProperty;

        private void OnEnable()
        {
            presetProperty = serializedObject.FindProperty("preset");
        }

        /// <summary>Draws the standard feature properties plus RIC RetroVision workflow actions.</summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            VHSRendererFeature feature = target as VHSRendererFeature;
            RICRetroVisionPreset preset = presetProperty == null ? null : presetProperty.objectReferenceValue as RICRetroVisionPreset;

            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("RIC RetroVision Tools", EditorStyles.boldLabel);

                if (feature != null && !feature.HasValidShader)
                    EditorGUILayout.HelpBox("RIC RetroVision: nenhum shader válido foi encontrado. Atribua o shader comercial acima ou verifique o stripping.", MessageType.Error);

                using (new EditorGUI.DisabledScope(preset == null || feature == null))
                {
                    if (GUILayout.Button("Apply Preset"))
                    {
                        Undo.RecordObject(feature, "Apply RIC RetroVision Preset");
                        feature.ApplyPreset(preset);
                        EditorUtility.SetDirty(feature);
                        serializedObject.Update();
                    }
                }

                using (new EditorGUI.DisabledScope(feature == null))
                {
                    if (GUILayout.Button("Reset To Defaults"))
                    {
                        Undo.RecordObject(feature, "Reset RIC RetroVision Settings");
                        feature.ResetSettingsToDefaults();
                        EditorUtility.SetDirty(feature);
                        serializedObject.Update();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
