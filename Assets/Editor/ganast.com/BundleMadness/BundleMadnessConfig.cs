using System.IO;

using UnityEngine;

namespace com.ganast.jm.unity.BundleMadness {

    /**
     * @todo: doc
     */
    public static class BundleMadnessConfig {

        public static readonly string PATH_REL = "ganast.com/BundleMadness";

        public static readonly string PATH_FULL = Path.Combine(Application.dataPath, PATH_REL);

        public static readonly string PATH_SETTINGS = Path.Combine(PATH_FULL, "BundleMadnessSettings.json");

        public static readonly int LOG_MAXLINES = 100;
    }
}