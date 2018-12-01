using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FolderTemplate : MonoBehaviour
{
    const string PARENT_FOLDER = "Assets";
    const string EDITOR_SETTING_ASSET_PATH = "ProjectSettings/EditorSettings.asset";
    const string EDITOR_USE_MODE_BEHAVIOUR_KEY = "FOLDER_TEMPLATE_USE_MODE_BEHAVIOUR_SETTING";
    const string EDITOR_USE_2D_TEMPLATE_KEY = "FOLDER_TEMPLATE_USE_2D_TEMPLATE";

    static readonly string[] _2D_FolderNames = new string[]
    {
        "Scenes",
        "Scripts",
        "Prefabs",
        "Sprites",
        "Sounds"
    };

    static readonly string[] _3D_FolderNames = new string[]
    {
        "Scenes",
        "Scripts",
        "Prefabs",
        "Textures",
        "Materials",
        "Sounds"
    };

    static bool isInit = false;
    static bool isUseTemplate2D = true;
    static bool isUseModeBehaviourInEditorSetting = true;


    [PreferenceItem("Folder Template")]
    static void PreferenceGUI()
    {
        Initialize();
        DrawGUI();
    }

    [MenuItem("Custom/Create new template folders %#F12")]
    static void CreateTemplateFolders()
    {
        if (isUseModeBehaviourInEditorSetting) {
            isUseTemplate2D = IsModeBehaviourIs2D();
        }

        string messageFormat = "About to create new folders with {0} Template";
        string strTemplate = (isUseTemplate2D) ? "2D" : "3D";
        string message = string.Format(messageFormat, strTemplate);

        bool isCreate = EditorUtility.DisplayDialog("Warning", message, "Create", "Cancel");

        if (!isCreate) {
            return;
        }

        string[] folderPaths = GetAllPathNames(isUseTemplate2D);
        string[] folderNames = GetFolderNames(isUseTemplate2D);

        for (int i = 0; i < folderPaths.Length; ++i)
        {
            bool isFolderExist = AssetDatabase.IsValidFolder(folderPaths[i]);

            if (!isFolderExist) {
                AssetDatabase.CreateFolder(PARENT_FOLDER, folderNames[i]);
            }
        }

        EditorUtility.DisplayDialog("Success", "Finished create folders", "OK");
    }

    [MenuItem("Custom/Create new template folders %#F12", true)]
    static bool ValidateCreateTemplateFolders()
    {
        return !IsAllFolderExist(isUseTemplate2D);
    }

    static void Initialize()
    {
        if (isInit) {
            return;
        }

        if (EditorPrefs.HasKey(EDITOR_USE_MODE_BEHAVIOUR_KEY)) {
            LoadSetting();
        }
        else {
            isUseModeBehaviourInEditorSetting = true;
            isUseTemplate2D = true;
            SaveSetting();
        }

        isInit = true;
    }

    static void LoadSetting()
    {
        isUseModeBehaviourInEditorSetting = EditorPrefs.GetBool(EDITOR_USE_MODE_BEHAVIOUR_KEY);
        isUseTemplate2D = EditorPrefs.GetBool(EDITOR_USE_2D_TEMPLATE_KEY);
    }

    static void SaveSetting()
    {
        EditorPrefs.SetBool(EDITOR_USE_MODE_BEHAVIOUR_KEY, isUseModeBehaviourInEditorSetting);
        EditorPrefs.SetBool(EDITOR_USE_2D_TEMPLATE_KEY, isUseTemplate2D);
    }

    static void DrawGUI()
    {
        isUseModeBehaviourInEditorSetting = EditorGUILayout.Toggle("Use Mode Behaviour Setting", isUseModeBehaviourInEditorSetting);

        if (!isUseModeBehaviourInEditorSetting) {
            isUseTemplate2D = EditorGUILayout.Toggle("Use 2D Template", isUseTemplate2D);
        }

        if (GUILayout.Button("Apply")) {
            SaveSetting();
        }
    }

    static bool IsAllFolderExist(bool isUseTemplate2D)
    {
        bool result = false;
        string[] folderPaths = GetAllPathNames(isUseTemplate2D);

        foreach (string name in folderPaths)
        {
            result = AssetDatabase.IsValidFolder(name);

            if (!result) {
                break;
            }
        }

        return result;
    }

    static string[] GetAllPathNames(bool isUseTemplate2D)
    {
        List<string> folderPaths = new List<string>();
        string[] folderNames = GetFolderNames(isUseTemplate2D);

        foreach (string name in folderNames)
        {
            string path = (PARENT_FOLDER + @"\" + name);
            folderPaths.Add(path);
        }

        string[] result = folderPaths.ToArray();
        return result;
    }

    static string[] GetFolderNames(bool isUseTemplate2D)
    {
        return (isUseTemplate2D) ? _2D_FolderNames : _3D_FolderNames;
    }

    static bool IsModeBehaviourIs2D()
    {
        bool result = false;
        Object obj = AssetDatabase.LoadAllAssetsAtPath(EDITOR_SETTING_ASSET_PATH)[0];

        SerializedObject editorSetting = new SerializedObject(obj);
        SerializedProperty defaultBehaviourMode = editorSetting.FindProperty("m_DefaultBehaviorMode");

        result = (defaultBehaviourMode.intValue == 1) ? true : false;
        return result;
    }
}
