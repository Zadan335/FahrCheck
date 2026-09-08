using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace FahrCheck.Editor
{
    public static class BuildVerification
    {
        public static void Foundation()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PrototypeBuilder.ScenePath) == null) PrototypeBuilder.CreatePrototype();
            else EditorSceneManager.OpenScene(PrototypeBuilder.ScenePath);
            PrototypeChecks.Run();
            AcademyChecks.Run();
            RealismChecks.Run();
            Debug.Log("FAHRCHECK FOUNDATION PASSED");
        }
        public static void Upgrade() { PrototypeBuilder.CreatePrototype(); Foundation(); }
        public static void AcademyPlay() { PrototypeBuilder.CreatePrototype(); PlayChecks(); }
        public static void PlayChecks()
        {
            Foundation();
            EditorApplication.isPlaying = true;
        }
        public static void UpgradeAndBuild() { PrototypeBuilder.CreatePrototype(); BuildMac(); }
        public static void BuildMac()
        {
            Foundation();
            CoursePreview.Capture();
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            EditorUserBuildSettings.SetPlatformSettings("Standalone", "OSXUniversal", "Architecture", "x64");
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "../Builds/Mac"));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { PrototypeBuilder.ScenePath }, locationPathName = "Builds/Mac/FahrCheck-R8.app", target = BuildTarget.StandaloneOSX, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Mac build failed: " + report.summary.result);
            Debug.Log("FAHRCHECK MAC BUILD PASSED: " + report.summary.outputPath);
        }
    }
}
