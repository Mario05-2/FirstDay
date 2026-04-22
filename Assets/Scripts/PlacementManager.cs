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

    //mission state tracking
    private enum MissionState
    {
        ScannerPending,
        ScannerPlaced,
        RelayUnlocked,
        RelayPlaced
    }

    private MissionState state = MissionState.ScannerPending;

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

    // overlay
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

                //block before scanner is placed
                if (state < MissionState.RelayUnlocked)
                {
                    Debug.LogWarning("Relay not unlocked yet!");
                    return;
                }

                if (relayOverlayCube)
                    relayOverlayCube.SetActive(true);
                break;
        }
    }

    //placement
    private void PlaceObject(string objectType)
    {
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
                state = MissionState.ScannerPlaced;
                Debug.Log("Scanner placed");

                // unlock relay after scanner
                state = MissionState.RelayUnlocked;
                Debug.Log("Relay unlocked");
                break;

            case "Relay":

                if (state < MissionState.RelayUnlocked)
                {
                    Debug.LogWarning("You must place Scanner first!");
                    HideAllOverlays();
                    return;
                }

                prefab = relayPrefab;
                state = MissionState.RelayPlaced;
                Debug.Log("Relay placed");
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

    public bool IsRelayUnlocked()
    {
        return state >= MissionState.RelayUnlocked;
    }
}