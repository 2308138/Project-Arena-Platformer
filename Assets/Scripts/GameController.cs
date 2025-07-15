using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] public int progressAmount;
    [SerializeField] public Slider progressSlider;

    [SerializeField] public GameObject player;
    [SerializeField] public GameObject loadCanvas;
    [SerializeField] public List<GameObject> levels;

    [SerializeField] public GameObject gameOverScreen;
    [SerializeField] public TMP_Text survivedText;

    [SerializeField] public static event Action OnReset;

    [SerializeField] private int currentLevelIndex = 0;

    [SerializeField] private int survivedLevelsCount;

    void Start()
    {
        progressAmount = 0;
        progressSlider.value = 0;
        Gem.OnGemCollect += IncreaseProgressAmount;
        HoldToLoadLevel.OnHoldComplete += LoadNextLevel;
        loadCanvas.SetActive(false);
        PlayerHealth.OnPlayerDied += GameOverScreen;
        gameOverScreen.SetActive(false);
    }

    void IncreaseProgressAmount(int amount)
    {
        progressAmount += amount;
        progressSlider.value = progressAmount;

        if (progressAmount >= 100)
        {
            loadCanvas.SetActive(true);
        }
    }

    void LoadLevel(int level, bool wantSurvivedIncrease)
    {
        loadCanvas.SetActive(false);

        levels[currentLevelIndex].gameObject.SetActive(false);
        levels[level].gameObject.SetActive(true);

        player.transform.position = new Vector3(0F, 0.5085F, 0F);

        currentLevelIndex = level;
        progressAmount = 0;
        progressSlider.value = 0;

        if (wantSurvivedIncrease) 
            survivedLevelsCount++;
    }

    void LoadNextLevel()
    {
        int nextLevelIndex = (currentLevelIndex == levels.Count - 1) ? 0 : currentLevelIndex + 1;
        LoadLevel(nextLevelIndex, true);
    }

    void GameOverScreen()
    {
        gameOverScreen.SetActive(true);
        survivedText.text = "YOU SURVIVED " + survivedLevelsCount + " LEVEL";

        if (survivedLevelsCount != 1)
            survivedText.text += "S";

        Time.timeScale = 0;
    }

    public void ResetGame()
    {
        gameOverScreen.SetActive(false);
        survivedLevelsCount = 0;
        LoadLevel(0, false);
        OnReset.Invoke();
        Time.timeScale = 1;
    }
}