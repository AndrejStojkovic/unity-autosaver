using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;
using Unity.EditorCoroutines.Editor;

public class AutoSaver : EditorWindow
{
    static bool IsEnabled = false;
    static bool LogEnabled = false;
    static bool SaveOnPlayModeChange = false;
    static float Interval = 30;
    static string AutoSaveMessage = "Auto saving all open scenes.";
    static string PreferencesPath = "Assets/Editor/AutoSaver";

    static AutoSaverPreferences preferences;
    static EditorCoroutine saveCoroutine;
    static string Path = PreferencesPath + "/AutoSaverPreferences.asset";
    private Vector2 scrollPos;

    [InitializeOnLoadMethod]
    private void OnEnable() {
        LoadPreferences();
    }

    private void OnDisable() {
        if(saveCoroutine != null) {
            EditorCoroutineUtility.StopCoroutine(saveCoroutine);
        }

        if(SaveOnPlayModeChange) {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChangeEvent;
        }
    }

    [MenuItem("Tools/AutoSaver")]
    private static void ShowWindow() {
        var window = GetWindow<AutoSaver>();
        window.titleContent = new GUIContent("Auto-Saver Preferences");
        window.Show();
    }
 
    private void OnGUI() {
        IsEnabled = EditorGUILayout.Toggle("Auto Saver Enabled", IsEnabled);
        LogEnabled = EditorGUILayout.Toggle("Logs Enabled", LogEnabled);
        SaveOnPlayModeChange = EditorGUILayout.Toggle("Save on Play", SaveOnPlayModeChange);
        Interval = EditorGUILayout.FloatField("Interval", Interval);
        AutoSaveMessage = EditorGUILayout.TextField("Auto Saver Message", AutoSaveMessage);
        PreferencesPath = EditorGUILayout.TextField("Preferences Path", PreferencesPath);
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Press Update Preferences to save and update your changes!", MessageType.Info);
        EditorGUILayout.Space(5);

        if(GUILayout.Button("Create Preferences Asset")) {
            CreatePreferencesAsset();
        }

        if(GUILayout.Button("Save/Update Preferences")) {
            SavePreferences();
            LoadPreferences();
        }
    }

    public void SaveEditor() {
        if(LogEnabled) Debug.Log(AutoSaveMessage);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private IEnumerator SaveEditorCoroutine() {
        while(true) {
            if(IsEnabled && !Application.isPlaying) {
                SaveEditor();
            }

            yield return new EditorWaitForSeconds(Interval * 60);
        }
    }

    public void OnPlayModeStateChangeEvent(PlayModeStateChange state) {
        if(EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying) {
            SaveEditor();
        }
    }

    public void CreatePreferencesAsset() {
        preferences = CreateInstance<AutoSaverPreferences>();
        AssetDatabase.CreateAsset(preferences, Path);
    }

    private void SavePreferences() {
        preferences = (AutoSaverPreferences)AssetDatabase.LoadAssetAtPath(Path, typeof(AutoSaverPreferences));

        if(preferences == null) {
            CreatePreferencesAsset();
        }

        preferences.IsEnabled = IsEnabled;
        preferences.LogEnabled = LogEnabled;
        preferences.SaveOnPlayModeChange = SaveOnPlayModeChange;
        preferences.Interval = Interval;
        preferences.AutoSaveMessage = AutoSaveMessage;
        preferences.PreferencesPath = PreferencesPath;

        EditorUtility.SetDirty(preferences);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void LoadPreferences() {
        preferences = (AutoSaverPreferences)AssetDatabase.LoadAssetAtPath(Path, typeof(AutoSaverPreferences));
        
        if(preferences == null) {
            CreatePreferencesAsset();
            SavePreferences();
        }

        IsEnabled = preferences.IsEnabled;
        LogEnabled = preferences.LogEnabled;
        SaveOnPlayModeChange = preferences.SaveOnPlayModeChange;
        Interval = preferences.Interval;
        AutoSaveMessage = preferences.AutoSaveMessage;
        PreferencesPath = preferences.PreferencesPath;

        if(IsEnabled)
            saveCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(SaveEditorCoroutine());

        if(SaveOnPlayModeChange)
            EditorApplication.playModeStateChanged += OnPlayModeStateChangeEvent;
    }
}
