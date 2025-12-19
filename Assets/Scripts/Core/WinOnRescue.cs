using TMPro;
using UnityEngine;

public class WinOnRescue : MonoBehaviour
{
    [Header("Reference")]
    public FafnirController fafnir;

    [Header("UI (optional)")]
    public GameObject winPanel;
    public TMP_Text winText;
    [TextArea] public string message = "¡Victoria! Liberaste a Fafnir.";

    [Header("Behavior")]
    public bool pauseOnWin = true;

    private bool won;

    private void Awake()
    {
        if (fafnir == null)
            fafnir = FindObjectOfType<FafnirController>();

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    private void Update()
    {
        if (won || fafnir == null) return;

        if (fafnir.rescued)
        {
            won = true;

            if (winText != null) winText.text = message;
            if (winPanel != null) winPanel.SetActive(true);

            if (pauseOnWin) Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("[WinOnRescue] WIN!");
        }
    }
}
