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
        if (placementOverlay != null)
        {
            placementOverlay.SetActive(false);
        }
    }

    public void SetCurrentZone(PlacementZone zone)
    {
        currentZone = zone;
    }

    [YarnCommand("ShowPlacementOverlay")]
    public void ShowOverlay(string objectType)
    {
        if (placementOverlay == null)
        {
            Debug.LogWarning("PlacementManager: placementOverlay is not assigned.");
            return;
        }

        placementOverlay.SetActive(true);
    }

    [YarnCommand("PlaceObject")]
    public void PlaceObject(string objectType)
    {
        if (currentZone == null)
        {
            Debug.LogWarning("PlacementManager: no active placement zone was found.");
            if (placementOverlay != null)
            {
                placementOverlay.SetActive(false);
            }
            return;
        }

        GameObject prefab = null;

        switch (objectType)
        {
            case "Scanner":
                prefab = scannerPrefab;
                break;
            case "Relay":
                prefab = relayPrefab;
                break;
            default:
                Debug.LogWarning($"PlacementManager: unknown object type '{objectType}'.");
                break;
        }

        if (prefab != null)
        {
            Instantiate(prefab, currentZone.transform.position, Quaternion.identity);
            currentZone.CompletePlacement();
            currentZone = null;
        }

        if (placementOverlay != null)
        {
            placementOverlay.SetActive(false);
        }
    }
}