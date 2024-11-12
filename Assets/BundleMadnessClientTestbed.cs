using System;
using System.Collections.Generic;

using com.ganast.jm.unity.BundleMadness;

using UnityEngine;
using UnityEngine.Networking;

public class BundleMadnessClientTestbed: MonoBehaviour {

    private Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();

    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    protected void SpawnPrefab(string bundleName, string prefabName) {
        if (!bundles.ContainsKey(bundleName)) {
            Debug.Log($"[BundleMadnessClientTestbed] ERROR: Bundle {bundleName} not loaded");
        }
        else {
            try {

                string prefabPath = $"assets/assetbundles/{bundleName}/{prefabName}";

                if (!prefabs.ContainsKey(prefabPath)) {
                    GameObject prefab = bundles[bundleName].LoadAsset<GameObject>(prefabPath);
                    prefabs.Add(prefabPath, prefab);
                }

                Instantiate(
                    prefabs[prefabPath],
                    new Vector3(
                        UnityEngine.Random.Range(-10, 10),
                        UnityEngine.Random.Range(-10, 10),
                        UnityEngine.Random.Range(-10, 10)
                    ),
                    Quaternion.identity);
            }
            catch (Exception ex) {
                Debug.Log($"[BundleMadnessClientTestbed] ERROR: Could not spawn prefab: {ex.Message})");
            }
        }
    }

    public void OnGUI() {

        GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height / 3 - 10));

        BundleMadnessClient client = BundleMadnessClient.GetInstance();

        if (GUILayout.Button("Fetch manifest")) {
            client.FetchManifest(OnManifestFetchSuccess, OnManifestFetchError);
        }

        if (GUILayout.Button("Fetch Bottles bundle")) {
            client.FetchBundle("bottles", OnBundleFetchSuccess, OnBundleFetchError);
        }

        if (GUILayout.Button("Spawn Bottle prefab")) {
            SpawnPrefab("bottles", "bottle/bottlemodel.prefab");
        }

        if (GUILayout.Button("Fetch Cups bundle")) {
            client.FetchBundle("cups", OnBundleFetchSuccess, OnBundleFetchError);
        }

        if (GUILayout.Button("Spawn Cup prefab")) {
            SpawnPrefab("cups", "cup/cupmodel.prefab");
        }

        if (GUILayout.Button("Fetch Skulls bundle")) {
            client.FetchBundle("skulls", OnBundleFetchSuccess, OnBundleFetchError);
        }

        if (GUILayout.Button("Spawn Skull prefab")) {
            SpawnPrefab("skulls", "skullprefab.prefab");
        }

        GUILayout.EndArea();
    }

    public void OnManifestFetchError(UnityWebRequest.Result result, string error) {
        Debug.Log($"[BundleMadnessClientTestbed] ERROR: Could not fetch manifest: {error} ({result})");
    }

    public void OnManifestFetchSuccess(BundleMadnessManifest manifest) {

        foreach (string bundle in manifest.bundles.Keys) {

            Debug.Log($"[BundleMadnessClientTestbed] {bundle}");

            foreach (string asset in manifest.bundles[bundle]) {
                Debug.Log($"[BundleMadnessClientTestbed]   {asset}");
            }
        }
    }

    public void OnBundleFetchError(UnityWebRequest.Result result, string error) {
        Debug.Log($"[BundleMadnessClientTestbed] ERROR: Could not fetch asset bundle: {error} ({result})");
    }

    public void OnBundleFetchSuccess(AssetBundle bundle) {

        bundles.Add(bundle.name, bundle);

        foreach (string asset in bundle.GetAllAssetNames()) {

            Debug.Log($"[BundleMadnessClientTestbed] {asset}");
        }
    }
}
