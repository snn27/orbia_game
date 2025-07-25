using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private int WhatLevel;

    [SerializeField]
    private GameObject MenuPanel;


    private void Awake()
    {
        WhatLevel = SceneManager.GetActiveScene().buildIndex ;
    }

    public void StartGameButton()
    {
        SceneManager.LoadScene("Level0");
    }

    public void MenuButton()
    {
        Time.timeScale = 0f; // Zamanı dondur
        MenuPanel.SetActive(true);
    }

    public void RepeatButton()
    {
        Time.timeScale = 1f; // Sahneyi yeniden yüklemeden hemen önce zamanı normale döndür!
        SceneManager.LoadScene(WhatLevel);
    }

    public void BaseButton()
    {
        SceneManager.LoadScene("BaseManu");
    }

    public void PlayButton()
    {
        MenuPanel.SetActive(false);
        Time.timeScale = 1f; // Zamanı dondur
    }
}
