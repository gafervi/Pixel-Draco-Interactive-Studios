using UnityEngine;
using UnityEngine.Events;

public class FafnirController : MonoBehaviour
{
    [Header("Estado")]
    public bool rescued = false;

    [Header("Feedback")]
    [TextArea] public string rescuedText = "Fafnir ha sido liberado.";

    public int keysRequired = 3;
    public string needKeysText = "Necesitas 3 llaves para liberar a Fafnir.";
    public UIMessage ui;

    [Header("Events")]
    public UnityEvent OnRescued;

    private void Awake()
    {
        if (ui == null)
            ui = FindObjectOfType<UIMessage>();
    }

    //usa las llaves del jugador que interactúa
    public void Interact(PlayerKeys playerKeys)
    {
        if (rescued)
        {
            ui?.Show("Ya fue liberado.", 2f);
            return;
        }

        if (playerKeys == null || !playerKeys.HasKeys(keysRequired))
        {
            ui?.Show(needKeysText, 2f);
            return;
        }

        playerKeys.ConsumeKeys(keysRequired);

        rescued = true;
        ui?.Show(rescuedText, 3f);

        OnRescued?.Invoke();
    }
}
