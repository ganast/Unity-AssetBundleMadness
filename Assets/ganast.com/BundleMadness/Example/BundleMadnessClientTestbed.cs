using System;
using System.Collections.Generic;
using System.IO;

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
                Debug.Log($"[BundleMadnessClientTestbed] ERROR: Could not spawn prefab: {ex.Message}");
            }
        }
    }

    public void OnGUI() {

        GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height - 20));

        BundleMadnessClient client = BundleMadnessClient.GetInstance();

        if (GUILayout.Button("Fetch manifest")) {
            client.FetchManifest(OnManifestFetchSuccess, OnManifestFetchError);
        }

        if (GUILayout.Button("Fetch Materials bundle")) {
            client.FetchBundle("materials", OnBundleFetchSuccess, OnBundleFetchError);
        }

        if (GUILayout.Button("Fetch Cubes bundle")) {
            client.FetchBundle("cubes", OnBundleFetchSuccess, OnBundleFetchError);
        }

        if (GUILayout.Button("Spawn Red Cube prefab")) {
            SpawnPrefab("cubes", "coloured/redcube.prefab");
        }

        if (GUILayout.Button("Spawn Blue Cube prefab")) {
            SpawnPrefab("cubes", "coloured/bluecube.prefab");
        }

        if (GUILayout.Button("Spawn Default Cube prefab")) {
            SpawnPrefab("cubes", "defaultcube.prefab");
        }

        if (GUILayout.Button("Fetch Spheres bundle")) {
            client.FetchBundle("spheres", OnBundleFetchSuccess, OnBundleFetchError);
        }

        if (GUILayout.Button("Spawn Red Sphere prefab")) {
            SpawnPrefab("spheres", "coloured/redsphere.prefab");
        }

        if (GUILayout.Button("Spawn Green Sphere prefab")) {
            SpawnPrefab("spheres", "coloured/greensphere.prefab");
        }

        if (GUILayout.Button("Spawn Default Sphere prefab")) {
            SpawnPrefab("spheres", "defaultsphere.prefab");
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
