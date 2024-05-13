using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelStartButton : MonoBehaviour
{
    [SerializeField] private GameObject[] stars;
    [SerializeField] private Text levelLable;
    [SerializeField] private GameObject launchButton;
    [SerializeField] private int level;

    public static UnityAction<int> OnLaunch;

    public void SetLevel(int _level)
    {
        level = _level;

        UpdateView();
    }

    public void Launch()
    {
        OnLaunch?.Invoke(level);
    }

    private void UpdateView()
    {
        int starsCount = PlayerPrefs.GetInt($"Level{level}", 0);

        for(var i = 0; i < stars.Length; i++)
            stars[i].gameObject.SetActive(i < starsCount);

        levelLable.text = level.ToString();

        launchButton.gameObject.SetActive(level == 1 || PlayerPrefs.GetInt($"Level{level - 1}", 0) > 0);
    }
}
