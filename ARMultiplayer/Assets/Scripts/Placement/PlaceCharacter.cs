using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaceCharacter : NetworkBehaviour
{
    [SerializeField] private GameObject placementObject;

    private bool isPlaced = false;
    private bool characterPlaced = false; // New flag to avoid multiple placements
    private Camera mainCam;

    public static event Action characterPlacedEvent;

    private void Start()
    {
        mainCam = GameObject.FindObjectOfType<Camera>();
    }

    void Update()
    {
        // Prevent placing if already placed
        if (AllPlayerDataManager.Instance != default &&
            AllPlayerDataManager.Instance.GetHasPlacerPlaced(NetworkManager.Singleton.LocalClientId))
            return;

        // Add condition to prevent both mouse and touch input from triggering simultaneously
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0) && Input.touchCount == 0)  // Ignore touch when using mouse
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("UI Hit was recognized");
                return;
            }
            TouchToRay(Input.mousePosition);
        }
#endif

#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0 && Input.touchCount < 2 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = touch.position;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                Debug.Log("We hit a UI element");
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Debug.Log("Is Pointer Over GOJ, No placement");
                return;
            }
            TouchToRay(touch.position);
        }
#endif
    }

    // Method to convert touch position into raycast and handle placement
    void TouchToRay(Vector3 touch)
    {
        if (characterPlaced) return;  // Prevent duplicate placement

        Ray ray = mainCam.ScreenPointToRay(touch);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            SpawnPlayerServerRpc(hit.point, rotation, NetworkManager.Singleton.LocalClientId);
            characterPlaced = true;  // Mark as placed to avoid multiple calls
        }
    }

    // Server RPC to spawn player object
    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayerServerRpc(Vector3 position, Quaternion rotation, ulong callerID)
    {
        GameObject character = Instantiate(placementObject, position, rotation);

        NetworkObject characterNetworkObject = character.GetComponent<NetworkObject>();
        characterNetworkObject.SpawnWithOwnership(callerID);

        AllPlayerDataManager.Instance.AddPlacedPlayer(callerID);
    }
}
