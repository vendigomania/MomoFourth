using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorfulSlapGame : MonoBehaviour
{
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject levelsScreen;
    [SerializeField] private GameObject playScreen;
    [SerializeField] private GameObject tasksScreen;
    [SerializeField] private GameObject resultScreen;

    public static int[] LevelStars = new int[10];

    // Start is called before the first frame update
    void Start()
    {
        string levelStars = PlayerPrefs.GetString("LevelsStars", string.Empty);

        for(int i = 0; i < LevelStars.Length; i++)
        {
            if(i < levelStars.Length && int.TryParse(levelStars[i].ToString(), out var number))
            {
                LevelStars[i] = number;
            }
            else
            {
                LevelStars[i] = 0;
            }
        }

        LevelStartButton.OnLaunch += StartGame;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame(int level)
    {

    }
}
