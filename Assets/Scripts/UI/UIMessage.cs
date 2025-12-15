using UnityEngine;
using TMPro;

public class UIMessage : MonoBehaviour
{
    public TMP_Text messageText;
    public float defaultSeconds = 2f;
    float timer;

    private void Awake()
    {
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (timer <= 0f) return;
        timer -= Time.deltaTime;
        if (timer <= 0f && messageText != null)
            messageText.gameObject.SetActive(false);
    }

    public void Show(string msg, float seconds = -1f)
    {
        if (messageText == null) return;
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        timer = seconds > 0f ? seconds : defaultSeconds;
    }
}
