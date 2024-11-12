using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

namespace com.ganast.jm.unity.BundleMadness {

    /**
     * @todo: doc
     */
    public class BundleMadnessClient: MonoBehaviour {

        public delegate void OnFetchManifestSuccess(BundleMadnessManifest manifest);
        public delegate void OnFetchManifestError(UnityWebRequest.Result result, string error);

        public delegate void OnFetchBundleSuccess(AssetBundle bundle);
        public delegate void OnFetchBundleError(UnityWebRequest.Result result, string error);

        private static BundleMadnessClient inst = null;

        [SerializeField]
        private string url = null;

        /**
         * @todo: doc
         */
        public static BundleMadnessClient GetInstance() {
            return inst;
        }

        /**
         * @todo: doc
         */
        public void Awake() {
            inst = this;
        }

        /**
         * @todo: doc
         */
        public void SetURL(string url) {
            if (url == null) {
                return;
            }
            this.url = url;
        }

        /**
         * @todo: doc
         */
        public string GetURL() {
            return url;
        }

        /**
         * @todo: doc
         */
        public void FetchManifest(OnFetchManifestSuccess successHandler, OnFetchManifestError errorHandler = null) {
            if (url == null) {
                return;
            }
            StartCoroutine(FetchManifestWorker(successHandler, errorHandler));
        }

        /**
         * @todo: doc
         */
        protected IEnumerator FetchManifestWorker(OnFetchManifestSuccess successHandler, OnFetchManifestError errorHandler = null) {

            using (UnityWebRequest request = UnityWebRequest.Get($"{url}/MANIFEST")) {

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success && errorHandler != null) {
                    errorHandler(request.result, request.error);
                }
                else {
                    BundleMadnessManifest manifest = BundleMadnessManifest.FromJSON(request.downloadHandler.text);
                    if (manifest == null && errorHandler != null) {
                        errorHandler(UnityWebRequest.Result.DataProcessingError, "Could not parse JSON");
                    }
                    else {
                        successHandler(manifest);
                    }
                }
            }
        }

        /**
        * @todo: doc
        */
        public void FetchBundle(string name, OnFetchBundleSuccess successHandler, OnFetchBundleError errorHandler = null) {
            if (url == null) {
                return;
            }
            StartCoroutine(FetchBundleWorker(name, successHandler, errorHandler));
        }

        /**
          * @todo: doc
          */
        protected IEnumerator FetchBundleWorker(string name, OnFetchBundleSuccess successHandler, OnFetchBundleError errorHandler) {

            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle($"{url}/{name}")) {

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success && errorHandler != null) {
                    errorHandler(request.result, request.error);
                }
                else {
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                    if (bundle == null) {
                        errorHandler(request.result, "DownloadHandlerAssetBundle has no content");
                    }
                    else {
                        successHandler(bundle);
                    }
                }
            }
        }
    }
}