using System;
using System.IO;
using System.Collections.Generic;

using Renci.SshNet;
using Renci.SshNet.Sftp;

using UnityEngine;

// Use 'new(...)'
#pragma warning disable IDE0090

namespace com.ganast.jm.unity.BundleMadness {

    /**
     * @todo: doc
     */
    public static class BundleMadnessHelpers {

        /**
         * @todo: doc
         * 
         * @todo: Inject log instead of static access
         */
        public static string[] GetAllBundleNames(string path) {

            if (!Directory.Exists(path) || new DirectoryInfo(path).GetFiles().Length == 0) {
                return null;
            }

            AssetBundle.UnloadAllAssetBundles(true);

            string bundleName = new DirectoryInfo(path).Name;

            string buildBundlePath = Path.Combine(path, bundleName);

            BundleMadness.Log($"Loading assetbundle build from \"{buildBundlePath}\"");
            AssetBundle buildBundle = AssetBundle.LoadFromFile(buildBundlePath);

            if (buildBundle == null) {
                BundleMadness.Log($"ERROR: Could not load assetbundle build from \"{buildBundlePath}\". Make sure this is an actual AssetBundle build pipeline output directory");
                return null;
            }

            AssetBundleManifest buildManifest = buildBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (buildManifest == null) {
                BundleMadness.Log($"[AssetBundleMadness] ERROR: Could not load build manifest \"{buildBundlePath}\". Make sure this is an actual AssetBundle build pipeline output directory");
                return null;
            }

            return buildManifest.GetAllAssetBundles();
        }

        /**
         * 
         */
        public static List<string> RecursiveGetAllAssetsInDirectory(string path) {

            List<string> assets = new();

            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)) {
                if (Path.GetExtension(f) != ".meta" && Path.GetExtension(f) != ".cs" && Path.GetExtension(f) != ".unity") {
                    assets.Add(f);
                }
            }

            return assets;
        }

        /**
         * @todo: doc
         * 
         * @todo: Make async
         * @todo: Inject log
         */
        public static void Upload(string basePath, string item, string destinationPath, SftpClient ssh, bool dryRun = true) {

            // todo: if item is not a filename or a directory name, that is, if it includes
            // a path component, throw exception...

            // todo: if destination path does not exist, throw exception...

            string source = Path.Combine(basePath, item);
            string target = $"{destinationPath}/{item}";

            BundleMadness.Log($"Processing {source}");

            if (Directory.Exists(source)) {
                if (!dryRun && item != ".") {
#if DEBUG
                    Debug.Log($"DEBUG: mkdir {target}");
#endif
                    try {
                        ssh.CreateDirectory(target);
                    }
                    catch (Exception ex) {
                        BundleMadness.Log($"ERROR: Directory could not be created ({ex.Message})");
                        return;
                    }
                }
                DirectoryInfo directoryInfo = new DirectoryInfo(source);
                foreach (DirectoryInfo subdirectory in directoryInfo.GetDirectories()) {
                    Upload(source, subdirectory.Name, target, ssh, dryRun);
                }
                foreach (FileInfo file in directoryInfo.GetFiles()) {
                    Upload(source, file.Name, target, ssh, dryRun);
                }
            }
            else if (File.Exists(source)) {
#if DEBUG
                Debug.Log($"DEBUG: upload {target}");
#endif
                FileStream fs = null;
                if (!dryRun) {
                    try {
                        fs = File.OpenRead(source);
                        ssh.UploadFile(fs, target);
                    }
                    catch (Exception ex) {
                        BundleMadness.Log($"ERROR: File could not be uploaded ({ex.Message})");
                    }
                    finally {
                        if (fs != null) {
                            fs.Dispose();
                        }
                    }
                }
            }
        }

        /**
         * @todo: doc
         */
        public static void DeleteDirectory(SftpClient client, string path, bool deleteSelf = false) {
            foreach (SftpFile item in client.ListDirectory(path)) {
                if ((item.Name != ".") && (item.Name != "..")) {
                    if (item.IsDirectory) {
                        DeleteDirectory(client, item.FullName);
                        client.DeleteDirectory(item.FullName);
                    }
                    else {
                        client.DeleteFile(item.FullName);
                    }
                }
            }

            if (deleteSelf) {
                client.DeleteDirectory(path);
            }
        }
    }
}