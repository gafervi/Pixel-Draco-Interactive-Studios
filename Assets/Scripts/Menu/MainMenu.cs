using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject MenuPrincipal;
    public GameObject MenuOpciones;

    // --- OPCIONES ---
    public AudioSource audioSource;   // Música o audio principal

    public void OpenMainPanel()
    {
        MenuPrincipal.SetActive(true);
        MenuOpciones.SetActive(false);
    }

    public void OpenOptionsPanel()
    {
        MenuPrincipal.SetActive(false);
        MenuOpciones.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Nivel_1");
    }

    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void ChangeVolume(float volume)
    {
        audioSource.volume = volume;
    }
}

