using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSaverPreferences : ScriptableObject
{
    public bool IsEnabled = false;
    public bool LogEnabled = false;
    public bool SaveOnPlayModeChange = false;
    public float Interval = 30;
    public string AutoSaveMessage = "Auto saving all open scenes.";
    public string PreferencesPath = "Assets/Editor/AutoSaver/AutoSaverPreferences.asset";
}
