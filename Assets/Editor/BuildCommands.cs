using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace King.EditorTools
{
    // Batchmode entry points, driven from the command line:
    //   unity -quit -batchmode -nographics -projectPath . -executeMethod King.EditorTools.BuildCommands.WebGL
    // Output lands in build/<platform>/ next to the project.
    //
    // This is the local path. The Actions workflow leaves buildMethod unset and
    // lets unity-builder use its own build script, which writes to different
    // folder names - so if you ever point CI at these methods, fix the paths the
    // Pages upload step reads.
    public static class BuildCommands
    {
        const string OutputRoot = "build";

        static string[] Scenes()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
                throw new Exception("no enabled scenes in the build settings");
            return scenes;
        }

        public static void WebGL() => Run(BuildTarget.WebGL, "WebGL", "WebGL");

        public static void LinuxDesktop() => Run(BuildTarget.StandaloneLinux64, "Linux", "Rifki.x86_64");

        public static void MacDesktop() => Run(BuildTarget.StandaloneOSX, "macOS", "Rifki.app");

        public static void WindowsDesktop() => Run(BuildTarget.StandaloneWindows64, "Windows", "Rifki.exe");

        public static void Android() => Run(BuildTarget.Android, "Android", "Rifki.apk");

        // The table is built entirely from code, so no asset in the project ever
        // references the uGUI shaders. The build strips them and every player
        // comes out solid magenta while the editor looks fine. Pin them.
        static void EnsureUiShaders()
        {
            var wanted = new[] { "UI/Default", "UI/Default Font", "Sprites/Default" };
            var settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (settings.Length == 0)
                throw new Exception("could not open GraphicsSettings.asset");

            var so = new SerializedObject(settings[0]);
            var list = so.FindProperty("m_AlwaysIncludedShaders");
            bool changed = false;

            foreach (var name in wanted)
            {
                var shader = Shader.Find(name);
                if (shader == null)
                {
                    Debug.LogWarning("no shader called " + name + "; skipping");
                    continue;
                }

                bool present = false;
                for (int i = 0; i < list.arraySize; i++)
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                        present = true;
                if (present) continue;

                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = shader;
                changed = true;
                Debug.Log("added " + name + " to the always-included shaders");
            }

            if (changed)
            {
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }

        static void Run(BuildTarget target, string folder, string artifact)
        {
            EnsureUiShaders();
            var group = BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(group, target))
                throw new Exception(target + " support is not installed in this editor");

            // WebGL puts the player in a folder; the desktop targets want a file name.
            string path = Path.Combine(OutputRoot, folder);
            if (target != BuildTarget.WebGL)
                path = Path.Combine(path, artifact);
            Directory.CreateDirectory(target == BuildTarget.WebGL ? path : Path.GetDirectoryName(path));

            if (target == BuildTarget.WebGL)
            {
                // Gzip needs server-side content negotiation that a plain static host
                // (and file://) won't do, so keep the payload uncompressed.
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.template = "APPLICATION:Default";
                PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            }

            var options = new BuildPlayerOptions
            {
                scenes = Scenes(),
                locationPathName = path,
                target = target,
                targetGroup = group,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"{target} build {summary.result}: {summary.totalSize} bytes in {summary.totalTime}");

            if (summary.result != BuildResult.Succeeded)
            {
                foreach (var step in report.steps)
                    foreach (var msg in step.messages)
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                            Debug.LogError(step.name + ": " + msg.content);
                EditorApplication.Exit(1);
            }
        }
    }
}
