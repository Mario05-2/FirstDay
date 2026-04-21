using UnityEngine;
using Yarn.Unity;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [Header("Overlay")]
    public GameObject placementOverlay;

    [Header("Prefabs")]
    public GameObject scannerPrefab;
    public GameObject relayPrefab;

    private PlacementZone currentZone;

    private void Awake()
    {
        Instance = this;
        placementOverlay.SetActive(false);
    }

    public void SetCurrentZone(PlacementZone zone)
    {
        currentZone = zone;
    }

    [YarnCommand("ShowPlacementOverlay")]
    public void ShowOverlay(string objectType)
    {
        placementOverlay.SetActive(true);
    }

    [YarnCommand("PlaceObject")]
    public void PlaceObject(string objectType)
    {
        if (currentZone == null) return;

        GameObject prefab = null;

        switch (objectType)
        {
            case "Scanner":
                prefab = scannerPrefab;
                break;
            case "Relay":
                prefab = relayPrefab;
                break;
        }

        if (prefab != null)
        {
            Instantiate(prefab, currentZone.transform.position, Quaternion.identity);
        }

        placementOverlay.SetActive(false);

        currentZone.CompletePlacement();
        currentZone = null;
    }
}