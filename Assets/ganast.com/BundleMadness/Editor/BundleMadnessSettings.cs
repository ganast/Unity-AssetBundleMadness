using System;
using System.IO;

using UnityEngine;

namespace com.ganast.jm.unity.BundleMadness {

    /**
     * @todo: doc
     */
    [Serializable]
    public class BundleMadnessSettings {

        private static BundleMadnessSettings settings = null;

        [SerializeField]
        public string buildPath = "Build/AssetBundles";

        [SerializeField]
        public string bundlesPath = "Assets/AssetBundles";

        [SerializeField]
        public string sshHost = "";

        [SerializeField]
        public string sshPort = "22";

        [SerializeField]
        public string sshUser = "";

        [SerializeField]
        public string sshDestination = "";

        /**
         * @todo: doc
         */
        public static BundleMadnessSettings GetSettings() {
            if (settings == null) {
                if (!File.Exists(BundleMadnessConfig.PATH_SETTINGS)) {
                    Debug.Log("Settings not found, creating.");
                    Directory.CreateDirectory(BundleMadnessConfig.PATH_FULL);
                    settings = new BundleMadnessSettings();
                    settings.Save(BundleMadnessConfig.PATH_SETTINGS);
                }
                else {
                    settings = BundleMadnessSettings.Load(BundleMadnessConfig.PATH_SETTINGS);
                }
            }
            return settings;
        }

        /**
         * @todo: doc
         */
        public void Save(string path) {
            File.WriteAllText(path, JsonUtility.ToJson(this));
        }

        /**
         * @todo: doc
         */
        public static BundleMadnessSettings Load(string path) {
            try {
                return JsonUtility.FromJson<BundleMadnessSettings>(File.ReadAllText(path));
            }
            catch (Exception) {
                return null;
            }
        }
    }
}