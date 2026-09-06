using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FahrCheck.Editor
{
    public sealed class SetupWindow : EditorWindow
    {
        Vector2 scroll;
        public static string Status { get => UnityEditor.SessionState.GetString("FahrCheck.Setup", "Ready to create the course."); set { UnityEditor.SessionState.SetString("FahrCheck.Setup", value); Debug.Log("FahrCheck setup: " + value); } }
        public static string BlockReason => EditorApplication.isPlayingOrWillChangePlaymode ? "Stop Play mode before creating or opening the course." : EditorApplication.isCompiling ? "Unity is compiling. Wait for compilation to finish; resolve Console compiler errors if it fails." : EditorApplication.isUpdating ? "Unity is importing assets. Please wait." : EditorUtility.scriptCompilationFailed ? "Script compilation failed. Open Window > General > Console and resolve the first compiler error." : "";
        [MenuItem("FahrCheck/Open Setup", false, 0)]
        public static void Open() { GetWindow<SetupWindow>("FahrCheck Setup").minSize = new Vector2(480, 310); }
        void OnInspectorUpdate() { Repaint(); }
        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            GUILayout.Label("Lindenallee course setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Unity 2022.3.62f3 / Built-in Render Pipeline\n" + PrototypeBuilder.ScenePath, MessageType.Info);
            if (BlockReason.Length > 0) EditorGUILayout.HelpBox(BlockReason, MessageType.Warning);
            EditorGUILayout.HelpBox(Status, MessageType.None);
            if (GUILayout.Button("Create Course", GUILayout.Height(40))) { try { PrototypeBuilder.CreatePrototype(); } catch (Exception e) { Status = e.ToString(); Debug.LogException(e); } }
            using (new EditorGUI.DisabledScope(BlockReason.Length > 0 || AssetDatabase.LoadAssetAtPath<SceneAsset>(PrototypeBuilder.ScenePath) == null))
            {
                if (GUILayout.Button("Open Lindenallee")) OpenCourse();
                if (GUILayout.Button("Open and Play")) { if (OpenCourse()) EditorApplication.isPlaying = true; }
                if (GUILayout.Button("Run rule and scene checks")) { if (OpenCourse()) { try { PrototypeChecks.Run(); Status = "Rule and scene checks passed."; } catch (Exception e) { Status = e.ToString(); Debug.LogException(e); } } }
            }
            EditorGUILayout.EndScrollView();
        }
        static bool OpenCourse() { if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false; EditorSceneManager.OpenScene(PrototypeBuilder.ScenePath); return true; }
    }

}
