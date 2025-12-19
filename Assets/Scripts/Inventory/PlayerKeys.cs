using UnityEngine;

public class PlayerKeys : MonoBehaviour
{
    [Header("Keys")]
    public int keys = 0;

    public bool HasKeys(int required) => keys >= required;

    public void AddKey(int amount = 1)
    {
        keys += amount;
        Debug.Log("[PlayerKeys] Llaves: " + keys);
    }

    public void ConsumeKeys(int amount)
    {
        keys = Mathf.Max(0, keys - amount);
        Debug.Log("[PlayerKeys] Llaves restantes: " + keys);
    }

    [Header("Cheat (opcional)")]
    public bool enableCheat = true;
    public KeyCode cheatKey = KeyCode.K;

    private void Update()
    {
        if (!enableCheat) return;

        // Cheat: 1 tecla = set a 3 llaves (no spamear)
        if (Input.GetKeyDown(cheatKey))
        {
            keys = Mathf.Max(keys, 3);
            Debug.Log("[PlayerKeys] Cheat: keys set to 3");
        }
    }

    // Debug simple en pantalla (quitalo cuando tengas HUD)
    private void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 300, 30), "Llaves: " + keys);
    }
}
