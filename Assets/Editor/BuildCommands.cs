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

        public static void LinuxDesktop() => Run(BuildTarget.StandaloneLinux64, "Linux", "Greenbaize.x86_64");

        public static void MacDesktop() => Run(BuildTarget.StandaloneOSX, "macOS", "Greenbaize.app");

        public static void WindowsDesktop() => Run(BuildTarget.StandaloneWindows64, "Windows", "Greenbaize.exe");

        public static void Android() => Run(BuildTarget.Android, "Android", "Greenbaize.apk");

        static void Run(BuildTarget target, string folder, string artifact)
        {
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
