using UnityEngine;

public class PlayerKeys : MonoBehaviour
{
    public int keys = 0;

    public bool HasKeys(int required) => keys >= required;

    // Solo para pruebas (opcional):
    // Presiona K para sumar una llave
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            keys++;
            Debug.Log("[PlayerKeys] Llaves: " + keys);
        }
    }
}
