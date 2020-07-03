namespace UnityEngine.Rendering
{
    // This file can't be in the editor assembly as we need to access it in runtime-editor-specific
    // places like OnGizmo etc and we don't want to add the editor assembly as a dependency of the
    // runtime one

    // The UI layout/styling in this panel is broken and can't match the one from built-ins
    // preference panels as everything needed is internal/private (at the time of writing this
    // comment)

#if UNITY_EDITOR
    using UnityEditor;
    class Styles
    {
        public static readonly GUIContent userDefaults = EditorGUIUtility.TrTextContent("Use Defaults");
    }
    public static class CoreRenderPipelinePreferences
    {
        static bool m_Loaded = false;

        // Added default Colors so that they can be reverted back to these values
        static Color s_VolumeGizmoColorDefault = new Color(0.2f, 0.8f, 0.1f, 0.5f);
        static Color s_PreviewCameraBackgroundColorDefault = new Color(82.0f / 255.0f, 82.0f / 255.0f, 82.0f / 255.0f, 0.0f);
        static Color s_VolumeGizmoColor = s_VolumeGizmoColorDefault;
        static Color s_PreviewCameraBackgroundColor = s_PreviewCameraBackgroundColorDefault;

        public static Color volumeGizmoColor
        {
            get => s_VolumeGizmoColor;
            set
            {
                if (s_VolumeGizmoColor == value) return;
                s_VolumeGizmoColor = value;
                EditorPrefs.SetInt(Keys.volumeGizmoColor, (int)ColorUtils.ToHex(value));
            }
        }

        public static Color previewBackgroundColor
        {
            get => s_PreviewCameraBackgroundColor;
            set
            {
                if (s_PreviewCameraBackgroundColor == value) return;
                s_PreviewCameraBackgroundColor = value;
                EditorPrefs.SetInt(Keys.cameraBackgroundColor, (int)ColorUtils.ToHex(value));
            }
        }

        static class Keys
        {
            internal const string volumeGizmoColor = "CoreRP.Volume.GizmoColor";
            internal const string cameraBackgroundColor = "CoreRP.PreviewCamera.BackgroundColor";
        }

        [SettingsProvider]
        static SettingsProvider PreferenceGUI()
        {
            return new SettingsProvider("Preferences/Colors/SRP", SettingsScope.User)
            {
                guiHandler = searchContext =>
                {
                    if (!m_Loaded)
                        Load();

                    Rect r = EditorGUILayout.GetControlRect();
                    r.xMin = 10;
                    EditorGUIUtility.labelWidth = 251;
                    volumeGizmoColor = EditorGUI.ColorField(r, "Volume Gizmo Color", volumeGizmoColor);

                    Rect re = EditorGUILayout.GetControlRect();
                    re.xMin = 10;
                    previewBackgroundColor = EditorGUI.ColorField(re, "Preview Background Color", previewBackgroundColor);
                    
                    if (GUILayout.Button(Styles.userDefaults, GUILayout.Width(120)))
                    {
                        RevertColors();
                    }
                }
            };
        }

        static void RevertColors()
        {
            volumeGizmoColor = s_VolumeGizmoColorDefault;
            previewBackgroundColor = s_PreviewCameraBackgroundColorDefault;
        }

        static CoreRenderPipelinePreferences()
        {
            Load();
        }

        static void Load()
        {
            s_VolumeGizmoColor = GetColor(Keys.volumeGizmoColor, new Color(0.2f, 0.8f, 0.1f, 0.5f));
            s_PreviewCameraBackgroundColor = GetColor(Keys.cameraBackgroundColor, new Color(49.0f / 255.0f, 49.0f / 255.0f, 49.0f / 255.0f, 0.0f));

            m_Loaded = true;
        }

        static Color GetColor(string key, Color defaultValue)
        {
            int value = EditorPrefs.GetInt(key, (int)ColorUtils.ToHex(defaultValue));
            return ColorUtils.ToRGBA((uint)value);
        }
    }
#endif
}
