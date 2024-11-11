using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Networking;

// Use 'new(...)'
#pragma warning disable IDE0090

namespace com.ganast.jm.unity.BundleMadness {

    /**
     * @todo: doc
     */
    public class BundleMadnessManifest {

        public delegate void FetchManifestSuccessCallback(BundleMadnessManifest manifest);
        public delegate void FetchManifestErrorCallback(string error, UnityWebRequest.Result result);

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

        /**
         * @todo: doc
         */
        public static void FromURL(MonoBehaviour context, string url, FetchManifestSuccessCallback successHandler, FetchManifestErrorCallback errorHandler = null) {
            context.StartCoroutine(FromURLImpl(url, successHandler, errorHandler));
        }

        /**
         * @todo: doc
         */
        protected static IEnumerator FromURLImpl(string url, FetchManifestSuccessCallback successHandler, FetchManifestErrorCallback errorHandler = null) {

            using (UnityWebRequest request = UnityWebRequest.Get(url)) {

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success && errorHandler != null) {
                    errorHandler(request.error, request.result);
                }
                else {
                    BundleMadnessManifest manifest = FromJSON(request.downloadHandler.text);
                    if (manifest == null && errorHandler != null) {
                        errorHandler("Could not parse JSON", UnityWebRequest.Result.DataProcessingError);
                    }
                    else {
                        successHandler(manifest);
                    }
                }
            }
        }
    }
}