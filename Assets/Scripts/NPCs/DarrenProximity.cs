using UnityEngine;
using TMPro;

public class DarrenProximity : MonoBehaviour
{
    [Header("Trigger")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;

    [Header("UI - Panels")]
    public GameObject promptPanel;     // DarrenPromptPanel
    public GameObject dialoguePanel;   // DarrenDialoguePanel

    [Header("UI - Texts")]
    public TMP_Text promptText;        // DarrenPromptText
    public TMP_Text dialogueText;      // DarrenDialogueText

    [Header("Dialogue")]
    [TextArea(3, 8)]
    public string[] lines;

    private bool playerInside = false;
    private bool isTalking = false;
    private int index = 0;

    private void Awake()
    {
        // Estado inicial seguro (aunque en el Inspector estén prendidos)
        ShowPrompt(false);
        ShowDialogue(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;

        if (!isTalking)
        {
            ShowPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;
        EndDialogue(); // apaga todo y resetea
    }

    private void Update()
    {
        if (!playerInside) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (!isTalking)
                StartDialogue();
            else
                NextLine();
        }
    }

    private void StartDialogue()
    {
        if (lines == null || lines.Length == 0) return;

        isTalking = true;
        index = 0;

        ShowPrompt(false);
        ShowDialogue(true);

        dialogueText.text = lines[index];
    }

    private void NextLine()
    {
        index++;

        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = lines[index];
    }

    private void EndDialogue()
    {
        isTalking = false;
        index = 0;

        ShowDialogue(false);

        if (playerInside)
            ShowPrompt(true);
        else
            ShowPrompt(false);
    }

    private void ShowPrompt(bool show)
    {
        if (promptPanel != null)
            promptPanel.SetActive(show);

        if (promptText != null && show)
            promptText.text = "[E] Hablar con Darren";
    }

    private void ShowDialogue(bool show)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(show);

        if (dialogueText != null && !show)
            dialogueText.text = "";
    }
}
