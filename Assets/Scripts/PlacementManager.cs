using UnityEngine;
using Yarn.Unity;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [Header("Dialogue Runner")]
    [SerializeField] private DialogueRunner dialogueRunner;

    [Header("Prefabs")]
    public GameObject scannerPrefab;
    public GameObject relayPrefab;

    [Header("Overlays")]
    public GameObject scannerOverlayCube;
    public GameObject relayOverlayCube;

    private PlacementZone currentZone;
    private bool scannerPlaced = false;

    private void Awake()
    {
        Instance = this;

        if (dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner NOT assigned!");
            return;
        }

        Debug.Log("REGISTERING YARN COMMANDS ON: " + dialogueRunner.name);

        dialogueRunner.AddCommandHandler<string>(
            "ShowPlacementOverlay",
            ShowOverlay
        );

        dialogueRunner.AddCommandHandler<string>(
            "PlaceObject",
            PlaceObject
        );
    }

    private void Start()
    {
        HideAllOverlays();
    }

    //zone management
    public void SetCurrentZone(PlacementZone zone)
    {
        currentZone = zone;
        Debug.Log("ZONE SET: " + zone.name);
    }

    public void ClearZone()
    {
        currentZone = null;
    }

    //overlay
    private void HideAllOverlays()
    {
        if (scannerOverlayCube) scannerOverlayCube.SetActive(false);
        if (relayOverlayCube) relayOverlayCube.SetActive(false);
    }

    private void ShowOverlay(string objectType)
    {
        Debug.Log("SHOW OVERLAY: " + objectType);

        HideAllOverlays();

        switch (objectType)
        {
            case "Scanner":
                if (scannerOverlayCube)
                    scannerOverlayCube.SetActive(true);
                break;

            case "Relay":
                if (relayOverlayCube)
                    relayOverlayCube.SetActive(true);
                break;
        }
    }

    //placement object
    private void PlaceObject(string objectType)
    {
        Debug.Log("PLACE OBJECT: " + objectType);

        if (currentZone == null)
        {
            Debug.LogError("NO CURRENT ZONE!");
            return;
        }

        GameObject prefab = null;

        switch (objectType)
        {
            case "Scanner":
                prefab = scannerPrefab;
                scannerPlaced = true;
                Debug.Log("Scanner placed ✔");
                break;

            case "Relay":

                if (!scannerPlaced)
                {
                    Debug.LogWarning("You must place Scanner first!");
                    HideAllOverlays();
                    return;
                }

                prefab = relayPrefab;
                Debug.Log("Relay placed ✔");
                break;
        }

        if (prefab != null)
        {
            Instantiate(prefab, currentZone.transform.position, Quaternion.identity);
        }

        HideAllOverlays();

        currentZone.ClearZone();
        currentZone = null;
    }
}