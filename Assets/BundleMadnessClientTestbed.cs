using com.ganast.jm.unity.BundleMadness;

using UnityEngine;

public class BundleMadnessClientTestbed: MonoBehaviour {

    public void OnGUI() {

        if (GUI.Button(new Rect(10, 10, 100, 30), "Manifest")) {

            BundleMadnessClient client = BundleMadnessClient.GetInstance();

            if (client.GetURL() == null || client.GetURL() == "") {
                Debug.Log("[BundleMadnessClientTestbed] ERROR: Client URL not set");
            }

            else {

                client.FetchManifest(onManifestFetchSuccess);
            }
        }
    }

    public void onManifestFetchSuccess(BundleMadnessManifest manifest) {
        
        if (manifest == null) {
            Debug.Log("[BundleMadnessClientTestbed] ERROR: Could not fetch manifest");
            return;
        }

        foreach (string bundle in manifest.bundles.Keys) {

            Debug.Log(bundle);

            foreach (string asset in manifest.bundles[bundle]) {
                Debug.Log($"- {asset}");
            }
        }
    }
}
