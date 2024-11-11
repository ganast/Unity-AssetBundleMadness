using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

namespace com.ganast.jm.unity.BundleMadness {

    /**
     * @todo: doc
     */
    public class BundleMadnessClient: MonoBehaviour {

        public delegate void FetchBundleSuccessCallback(AssetBundle bundle);
        public delegate void FetchBundleErrorCallback(string error, UnityWebRequest.Result result);

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
        public void FetchManifest(BundleMadnessManifest.FetchManifestSuccessCallback successHandler, BundleMadnessManifest.FetchManifestErrorCallback errorHandler = null) {
            if (url == null) {
                return;
            }
            BundleMadnessManifest.FromURL(this, $"{url}/MANIFEST", successHandler, errorHandler);
        }


        /**
         * @todo: doc
         */
        public void FetchBundle(string name, FetchBundleSuccessCallback successHandler, FetchBundleErrorCallback errorHandler = null) {
            if (url == null) {
                return;
            }
            StartCoroutine(FetchBundleImpl(name, successHandler, errorHandler));
        }

        /**
         * @todo: doc
         */
        protected IEnumerator FetchBundleImpl(string name, FetchBundleSuccessCallback successHandler, FetchBundleErrorCallback errorHandler) {

            if (url == null) {
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle($"{url}/{name}")) {

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success && errorHandler != null) {
                    errorHandler(request.error, request.result);
                }
                else {
                    successHandler(DownloadHandlerAssetBundle.GetContent(request));
                }
            }
        }
    }
}