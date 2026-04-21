using UnityEngine;
using Yarn.Unity;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    public GameObject placementOverlayCube;
    public GameObject scannerPrefab;
    public GameObject relayPrefab;

    private PlacementZone currentZone;

    private DialogueRunner dialogueRunner;

    private void Awake()
    {
        Instance = this;

        if (placementOverlayCube != null)
            placementOverlayCube.SetActive(false);
    }

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (dialogueRunner == null)
        {
            Debug.LogError("No DialogueRunner found in scene!");
            return;
        }

        //manual command registration
        dialogueRunner.AddCommandHandler<string>(
            "ShowPlacementOverlay",
            ShowOverlay
        );

        dialogueRunner.AddCommandHandler<string>(
            "PlaceObject",
            PlaceObject
        );
    }

    public void SetCurrentZone(PlacementZone zone)
    {
        currentZone = zone;
    }

    //[YarnCommand("ShowPlacementOverlay")]
    public void ShowOverlay(string objectType)
    {
        Debug.Log("ShowOverlay called with objectType: " + objectType);
        if (placementOverlayCube != null)
            placementOverlayCube.SetActive(true);
    }

    //[YarnCommand("PlaceObject")]
    public void PlaceObject(string objectType)
    {
        Debug.Log("PlaceObject called with objectType: " + objectType);
        if (currentZone == null) return;

        GameObject prefabToPlace = null;

        switch (objectType)
        {
            case "Scanner":
                prefabToPlace = scannerPrefab;
                break;
            case "Relay":
                prefabToPlace = relayPrefab;
                break;
        }

        if (prefabToPlace != null)
        {
            Instantiate(prefabToPlace, currentZone.transform.position, Quaternion.identity);
        }

        if (placementOverlayCube != null)
            placementOverlayCube.SetActive(false);

        currentZone.CompletePlacement();
        currentZone = null;
    }
}