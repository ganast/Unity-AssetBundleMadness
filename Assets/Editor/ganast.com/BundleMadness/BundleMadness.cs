using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Renci.SshNet;

using UnityEngine;
using UnityEditor;

// Use 'new(...)'
#pragma warning disable IDE0090

// Simplify object initialization
#pragma warning disable IDE0017

namespace com.ganast.jm.unity.BundleMadness {

    /**
     * @todo: doc
     */
    [InitializeOnLoad]
    public class BundleMadness {

        public static int BUTTON_WIDTH = 100;
        public static int BUTTON_HEIGHT = 30;

        private static string logText = "CAUTION: This tool messes with filesystem contents here and remotely without\nasking for confirmation for any action taken. It is up to you to provide correct\nsettings (paths, etc.) If you don't, you will lose data. You have been warned.\nReady\n";

        private static Vector2 logScrollPosition;

        /**
         * @todo: doc
         */
        static BundleMadness() {

            // ensure settings file is there...
            BundleMadnessSettings.GetSettings();
        }

        /**
         * @todo: doc
         */
        public class BundleMadnessOptionsWindow: EditorWindow {

            /**
             * @todo: doc
             */
            public void OnGUI() {

                titleContent = new GUIContent("Options - BundleMadness");

                GUILayout.BeginVertical();
                GUIStyle g = new GUIStyle(GUI.skin.label);
                g.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("No options for you! Don't be a Lazy Larry, go edit the JSON file directly!", g,
                    GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUILayout.EndVertical();
            }
        }

        /**
         * @todo: doc
         */
        public class BundleMadnessPublishWindow: EditorWindow {

            private string sshPassword = "";

            private bool dryRun = true;

            /**
             * @todo: doc
             */
            private void OnGUI() {

                BundleMadnessSettings settings = BundleMadnessSettings.GetSettings();

                titleContent = new GUIContent("Publish - BundleMadness");

                GUILayout.BeginVertical();

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                GUIStyle g = new GUIStyle(GUI.skin.label);
                g.padding.top = 3;

                GUILayout.Label("Build directory:", g);

                if (EditorGUILayout.LinkButton(Path.Combine(BundleMadnessSettings.GetSettings().buildPath))) {
                    string directory = EditorUtility.OpenFolderPanel("Select directory", "", "");
                    if (directory != null && directory != "" && Directory.Exists(directory)) {
                        settings.buildPath = directory;
                        settings.Save(BundleMadnessConfig.PATH_SETTINGS);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(2);

                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.Label("Host:");
                settings.sshHost = GUILayout.TextField(settings.sshHost);
                EditorGUILayout.Space();

                GUILayout.Label("Port:");
                settings.sshPort = GUILayout.TextField(settings.sshPort);
                EditorGUILayout.Space();

                GUILayout.Label("Username:");
                settings.sshUser = GUILayout.TextField(settings.sshUser);
                EditorGUILayout.Space();

                GUILayout.Label("Password:");
                sshPassword = GUILayout.PasswordField(sshPassword, '*');
                EditorGUILayout.Space();

                GUILayout.Label("Destination:");
                settings.sshDestination = GUILayout.TextField(settings.sshDestination);
                EditorGUILayout.Space();

                GUILayout.EndVertical();

                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true));
                dryRun = EditorGUILayout.Toggle(dryRun, GUILayout.Width(20));
                GUILayout.Label("Dry run", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Publish", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    Publish(dryRun);
                }

                if (GUILayout.Button("Save", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    BundleMadnessSettings.GetSettings().Save(BundleMadnessConfig.PATH_SETTINGS);
                }

                if (GUILayout.Button("Show output", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    EditorUtility.RevealInFinder(BundleMadnessSettings.GetSettings().buildPath);
                }

                if (GUILayout.Button("Clear log", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    logText = "";
                }

                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal(GUILayout.Height(162));
                logScrollPosition = GUILayout.BeginScrollView(logScrollPosition, GUI.skin.box);
                GUILayout.Label(logText);
                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            /**
             * @todo: doc
             * 
             * @todo: make async with GUI updates or move to another thread...
             */
            public void Publish(bool dryRun = true) {

                BundleMadnessSettings settings = BundleMadnessSettings.GetSettings();

                SftpClient sftp = new SftpClient(settings.sshHost, int.Parse(settings.sshPort), settings.sshUser, sshPassword);

                string[] bundles = BundleMadnessHelpers.GetAllBundleNames(settings.buildPath);

                if (bundles == null || bundles.Length == 0) {
                    return;
                }

                try {

                    // connect...
                    Log($"Connecting to {settings.sshHost}:{settings.sshPort} as {settings.sshUser}");
                    sftp.Connect();

                    try {
                        Log($"Cleaning up \"{settings.sshDestination}\"");
                        if (!dryRun) {
                            BundleMadnessHelpers.DeleteDirectory(sftp, settings.sshDestination, false);
                        }
                    }
                    catch (Exception ex) {
                        Log($"ERROR: Cleanup failed ({ex.Message})");
                        return;
                    }

                    try {

                        // first, upload build bundle...
                        string buildBundle = (new DirectoryInfo(settings.buildPath)).Name;
                        BundleMadnessHelpers.Upload(settings.buildPath, buildBundle, settings.sshDestination, sftp, dryRun);
                        BundleMadnessHelpers.Upload(settings.buildPath, buildBundle + ".manifest", settings.sshDestination, sftp, dryRun);

                        // then, iterate over all built assetbundles...
                        foreach (string bundle in bundles) {
                            BundleMadnessHelpers.Upload(settings.buildPath, bundle, settings.sshDestination, sftp, dryRun);
                            BundleMadnessHelpers.Upload(settings.buildPath, bundle + ".manifest", settings.sshDestination, sftp, dryRun);
                        }

                        /*
                        // -- or --
                        // upload everything inside build directory...
                        Upload(settings.buildPath, ".", settings.sshDestination, sftp, dryRun);
                        */

                        // lastly, upload the custom manifest, if available...
                        if (File.Exists(Path.Combine(settings.buildPath, "MANIFEST"))) {
                            BundleMadnessHelpers.Upload(settings.buildPath, "MANIFEST", settings.sshDestination, sftp, dryRun);
                        }
                    }
                    catch (Exception ex) {
                        Log($"ERROR: Upload failed ({ex.Message})");
                        return;
                    }
                }
                catch (Exception ex) {
                    Log($"ERROR: Connection could not be established ({ex.Message})");
                }
                finally {
                    Log("Disconnecting");
                    sftp.Disconnect();
                }
            }

        }

        /**
         * @todo: doc
         */
        public class BundleMadnessBuildWindow: EditorWindow {

            private Vector2 contentsScrollPosition;

            private int selectedBundle;

            private static string[] bundleNames;

            private static string[] bundleContents;

            private bool initialized = false;

            /**
             * @todo: doc
             */
            protected void Refresh() {
                bundleNames = BundleMadnessHelpers.GetAllBundleNames(BundleMadnessSettings.GetSettings().buildPath);
                selectedBundle = 0;
                RefreshSelectedBundleContents();
            }

            /**
             * @todo: doc
             */
            protected void RefreshSelectedBundleContents() {
                if (bundleNames != null && bundleNames.Length != 0) {
                    BundleMadnessSettings settings = BundleMadnessSettings.GetSettings();
                    AssetBundle.UnloadAllAssetBundles(true);
                    AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(settings.buildPath, bundleNames[selectedBundle]));
                    bundleContents = assetBundle.GetAllAssetNames();
                }
                else {
                    bundleContents = null;
                }
            }

            /**
             * @todo: doc
             */
            private void OnGUI() {

                titleContent = new GUIContent("Build - BundleMadness");

                if (!initialized) {
                    Refresh();
                    initialized = true;
                }

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Build", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    Build();
                    Refresh();
                }
                /*
                // TODO: a clean button (delete all files and directories in directory) would be
                // dangerous without some kind of confirmation...
                if (GUILayout.Button("Clean", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    Clean();
                    Refresh();
                }
                */
                if (GUILayout.Button("Publish", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    ShowPublishWindow();
                }
                if (GUILayout.Button("Refresh", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    Refresh();
                }
                if (GUILayout.Button("Show output", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    EditorUtility.RevealInFinder(BundleMadnessSettings.GetSettings().buildPath);
                }
                if (GUILayout.Button("Clear log", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT))) {
                    logText = "";
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                GUIStyle asdasd = new GUIStyle(GUI.skin.label);
                asdasd.padding.top = 3;

                GUILayout.Label("Build directory:", asdasd);

                if (EditorGUILayout.LinkButton(Path.Combine(BundleMadnessSettings.GetSettings().buildPath))) {
                    string directory = EditorUtility.OpenFolderPanel("Select directory", "", "");

                    if (directory != null && directory != "" && Directory.Exists(directory) &&
                        Directory.GetParent(directory) != null) {

                        BundleMadnessSettings settings = BundleMadnessSettings.GetSettings();
                        settings.buildPath = directory;
                        settings.Save(BundleMadnessConfig.PATH_SETTINGS);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(2);

                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(200));

                if (bundleNames != null) {
                    int newSelectedBundle = GUILayout.SelectionGrid(selectedBundle, bundleNames, 1);
                    if (newSelectedBundle != selectedBundle) {
                        selectedBundle = newSelectedBundle;
                        RefreshSelectedBundleContents();
                    }
                }
                else {
                    GUILayout.Label("No assetbundles found");
                }

                GUILayout.FlexibleSpace();

                GUILayout.EndVertical();

                contentsScrollPosition = GUILayout.BeginScrollView(contentsScrollPosition, GUI.skin.box);

                if (bundleContents != null) {

                    string s = "";
                    foreach (string assetBundleContent in bundleContents) {
                        s += assetBundleContent + "\n";
                    }

                    GUILayout.Label(s, GUILayout.ExpandWidth(true));
                }
                else {
                    GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                }
                GUILayout.EndScrollView();

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(162));
                logScrollPosition = GUILayout.BeginScrollView(logScrollPosition);
                GUILayout.Label(logText);
                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            /**
             * @todo: doc
             */
            private void Build() {

                BundleMadnessSettings settings = BundleMadnessSettings.GetSettings();

                List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

                BundleMadnessManifest customManifest = new BundleMadnessManifest();

                foreach (string dir in Directory.GetDirectories(settings.bundlesPath)) {

                    DirectoryInfo di = new DirectoryInfo(dir);

                    string bundleName = di.Name;

                    AssetBundleBuild build = new AssetBundleBuild();
                    build.assetBundleName = bundleName;
                    build.assetNames = BundleMadnessHelpers.RecursiveGetAllAssetsInDirectory(dir).ToArray();

                    builds.Add(build);

                    customManifest.bundles.Add(build.assetBundleName, build.assetNames);
                }

                if (!Directory.Exists(settings.buildPath)) {
                    Directory.CreateDirectory(settings.buildPath);
                }

                BuildAssetBundlesParameters buildParams = new BuildAssetBundlesParameters();
                buildParams.outputPath = settings.buildPath;
                buildParams.options = BuildAssetBundleOptions.AssetBundleStripUnityVersion;
                buildParams.bundleDefinitions = builds.ToArray();

                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(buildParams);

                if (manifest != null) {

                    File.WriteAllText(Path.Combine(settings.buildPath, "MANIFEST"), customManifest.ToJSON());

                    foreach (var bundleName in manifest.GetAllAssetBundles()) {
                        string projectRelativePath = settings.buildPath + "/" + bundleName;
                        Log($"Built bundle {projectRelativePath} ({new FileInfo(projectRelativePath).Length} byte(s))");
                    }
                }
                else {
                    Log("ERROR: Build failed");
                }
            }

            /**
             * @todo: doc
             */
            /*
            private void Clean() {

                DirectoryInfo di;
                try {
                    di = new DirectoryInfo(BundleMadnessSettings.GetSettings().buildPath);
                }
                catch (Exception) {
                    return;
                }

                if (!di.Exists) {
                    return;
                }

                foreach (FileInfo file in di.EnumerateFiles()) {
                    try {
                        file.Delete();
                    }
                    catch (Exception ex) {
                        Log($"ERROR: Could not delete file \"{file.FullName}\" ({ex.GetType()})");
                    }
                }

                foreach (DirectoryInfo dir in di.EnumerateDirectories()) {
                    try {
                        dir.Delete(true);
                    }
                    catch (Exception ex) {
                        Log($"ERROR: Could not delete directory \"{dir.FullName}\" ({ex.GetType()})");
                    }
                }
            }
            */
        }

        /**
         * @todo: doc
         */
        [MenuItem("ganast.com/BundleMadness/Build", priority = 1)]
        private static void ShowBuildWindow() {
            EditorWindow.GetWindow<BundleMadnessBuildWindow>().Show();
        }

        /**
         * @todo: doc
         */
        [MenuItem("ganast.com/BundleMadness/Publish", priority = 2)]
        private static void ShowPublishWindow() {
            EditorWindow.GetWindow<BundleMadnessPublishWindow>().Show();
        }

        /**
         * @todo: doc
         */
        [MenuItem("ganast.com/BundleMadness/Options", priority = 100)]
        private static void ShowOptionsWindow() {
            EditorWindow.GetWindow<BundleMadnessOptionsWindow>().Show();
        }

        /**
         * @todo: doc
         */
        [MenuItem("ganast.com/BundleMadness/About", priority = 1000)]
        private static void About() {
        }

        /**
         * @todo: doc
         */
        public static void Log(string s) {

            // add new text...
            logText += s + "\n";

            // remove excessive lines...
            while (Regex.Matches(logText, "\n").Count > BundleMadnessConfig.LOG_MAXLINES) {
                logText = logText.Substring(logText.IndexOf("\n") + 1);
            }

            try {
                // scroll to bottom, the calculated size of the scrollview content is always
                // greater than the max scroll position value for a scrollview of non-zero
                // height, Unity will fix the value on the next frame, bit lame but works and no
                // need to invest more time in ill-designed IMGUI stuff...
                logScrollPosition.y = (int) GUI.skin.label.CalcSize(new GUIContent(logText)).y;
            }
            catch (Exception) {

            }

#if DEBUG
            // if in editor mode, log to Unity console and log as well...
            Debug.Log($"[BundleMadness] {s}");
#endif
        }
    }
}