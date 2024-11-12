using System;
using System.Collections.Generic;

using Newtonsoft.Json;

// Use 'new(...)'
#pragma warning disable IDE0090

namespace com.ganast.jm.unity.BundleMadness {

    /**
     * @todo: doc
     */
    public class BundleMadnessManifest {

        public Dictionary<string, string[]> bundles = new Dictionary<string, string[]>();

        /**
         * @todo: doc
         */
        public string ToJSON() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /**
         * @todo: doc
         */
        public static BundleMadnessManifest FromJSON(string json) {
            try {
                return JsonConvert.DeserializeObject<BundleMadnessManifest>(json);
            }
            catch (Exception) {
                return null;
            }
        }
    }
}