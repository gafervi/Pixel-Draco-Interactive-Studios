using UnityEngine;
using TMPro;

public class FafnirInteract : MonoBehaviour
{
    [Header("Referencias")]
    public FafnirController fafnir;


    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public TMP_Text promptText;

    private bool playerInside = false;

    private void Awake()
    {
        if (fafnir == null)
            fafnir = GetComponentInParent<FafnirController>();


        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    private void Reset()
    {
        fafnir = GetComponentInParent<FafnirController>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        Debug.Log("[FafnirInteract] Player dentro. Presiona E.");
        if (promptText != null)
            promptText.gameObject.SetActive(true);
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        Debug.Log("[FafnirInteract] Player fuera.");
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }


    private void Update()
    {
        if (fafnir != null && fafnir.rescued)
        {
            if (promptText != null) promptText.gameObject.SetActive(false);
            return;
        }
        if (!playerInside) return;
        if (!Input.GetKeyDown(interactKey)) return;


        if (fafnir == null)
        {
            Debug.LogError("[FafnirInteract] Falta asignar referencia a FafnirController.");
            return;
        }


        fafnir.Interact();
    }
}