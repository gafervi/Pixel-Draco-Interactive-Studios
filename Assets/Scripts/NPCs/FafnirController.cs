using UnityEngine;


public class FafnirController : MonoBehaviour
{
    [Header("Estado")]
    public bool rescued = false;


    [Header("Feedback")]
    [TextArea] public string promptText = "Presiona E para interactuar";
    [TextArea] public string rescuedText = "Fafnir ha sido liberado.";

    public int keysRequired = 3;
    public string needKeysText = "Necesitas 3 llaves para liberar a Fafnir.";
    public UIMessage ui;

    public void Interact()
    {
        if (rescued)
        {
            ui?.Show("Ya fue liberado.", 2f);
            return;
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        var keys = player != null ? player.GetComponent<PlayerKeys>() : null;

        if (keys == null || !keys.HasKeys(keysRequired))
        {
            ui?.Show(needKeysText, 2f);
            return;
        }

        rescued = true;
        ui?.Show(rescuedText, 3f);
    }

    private void Awake()
    {
        if (ui == null)
            ui = FindObjectOfType<UIMessage>();
    }


}