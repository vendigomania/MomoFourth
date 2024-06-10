using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ColorfulSlapGame : MonoBehaviour
{
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject levelsScreen;
    [SerializeField] private List<LevelStartButton> levelButtons = new List<LevelStartButton>();

    //Play state
    [SerializeField] private GameObject playScreen;
    [SerializeField] private Button flipBtn;
    [SerializeField] private Button discoBtn;
    [SerializeField] private Button boomBtn;

    [SerializeField] private Image[] playScreenIcons;
    [SerializeField] private Text[] playScreenValues;

    [SerializeField] private Text levelLable;
    [SerializeField] private Text timerLable;
    [SerializeField] private MatchThreeEngine.Board board;

    [SerializeField] private GameObject tasksScreen;
    [SerializeField] private Image[] tasksScreenIcons;
    [SerializeField] private Text[] tasksScreenValues;

    [SerializeField] private GameObject pauseScreen;

    //Result state

    [SerializeField] private GameObject resultScreen;
    [SerializeField] private GameObject[] resultStars;
    [SerializeField] private Text resultLable;

    [SerializeField] private ParticleSystem flipParticle;
    [SerializeField] private ParticleSystem discoParticle;
    [SerializeField] private ParticleSystem boomParticle;

    private (int id, int value)[] tasks = new(int, int)[3];

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        for (int i = 1; i < 12; i++)
        {
            levelButtons.Add(Instantiate(levelButtons[0], levelButtons[0].transform.parent));
        }

        LevelStartButton.OnLaunch += StartGame;
        board.OnMatch += OnMatch;
    }

    public int FlipCount;
    public int DiscoCount;
    public int BoomCount;

    // Update is called once per frame
    void Update()
    {
        flipBtn.interactable = board.ChoiseCount == 0 && FlipCount > 0;
        discoBtn.interactable = board.ChoiseCount == 1 && DiscoCount > 0;
        boomBtn.interactable = board.ChoiseCount == 1 && BoomCount > 0;
    }

    int currentLevel = 1;
    int levelsPage = 0;

    public void ShowLevelsScreen()
    {
        SoundManager.Instance.Click();

        startScreen.SetActive(false);
        levelsScreen.SetActive(true);

        for(var i = 0; i < 12; i++)
        {
            levelButtons[i].SetLevel(levelsPage * 12 + 1 + i);
        }
    }

    public void MovePage(int page)
    {
        SoundManager.Instance.Click();

        if (levelsPage + page < 0) return;

        levelsPage += page;

        ShowLevelsScreen();
    }

    public void StartGame(int level)
    {
        currentLevel = level;
        LoadLevel();
    }

    public void LoadLevel()
    {
        SoundManager.Instance.Click();

        tasks[0].id = (currentLevel + 9) % 5;
        tasks[1].id = (currentLevel + 9) % 7;
        tasks[2].id = (currentLevel + 9) % 9;

        tasks[0].value = 2 + (currentLevel + 1) * (currentLevel % 9 == tasks[1].id? tasks[0].id + 1 : 1);

        tasks[1].value = 2 + (currentLevel + 1) * (currentLevel % 9 == tasks[2].id? tasks[1].id + 1 : 1);
 
        tasks[2].value = 2 + (currentLevel + 1) * (currentLevel % 9 == tasks[0].id? tasks[2].id + 1 : 1);

        SetTasks(playScreenIcons, playScreenValues);
        SetTasks(tasksScreenIcons, tasksScreenValues);

        resultScreen.SetActive(false);
        levelsScreen.SetActive(false);
        playScreen.SetActive(true);
        tasksScreen.SetActive(true);
        pauseScreen.SetActive(false);

        FlipCount = 5;
        DiscoCount = 1;
        BoomCount = 3;

        levelLable.text = $"LEVEL: {currentLevel}";

        remainSeconds = 180;
    }

    public void BackToMenu()
    {
        SoundManager.Instance.Click();

        startScreen.SetActive(true);
        resultScreen.SetActive(false);
        levelsScreen.SetActive(false);
        playScreen.SetActive(false);
        tasksScreen.SetActive(false);
        pauseScreen.SetActive(false);

        StopAllCoroutines();
    }

    public void NextLevel()
    {
        currentLevel++;
        LoadLevel();
    }

    //Boosters
    public void Flip()
    {
        FlipCount--;
        board.FlipBooster(flipParticle.transform);

        flipParticle.Play();
    }

    public void Disco()
    {
        DiscoCount--;
        board.DiscoBooster();

        discoParticle.Play();
    }

    public void Boom()
    {
        BoomCount--;
        board.BoomBooster(boomParticle.transform);

        boomParticle.Play();
    }

    private void OnMatch(MatchThreeEngine.TileTypeAsset type, int count)
    {
        for(int i = 0; i < tasks.Length; i++)
        {
            if(type.id == tasks[i].id)
            {
                tasks[i].value = Mathf.Max(0, tasks[i].value - count);
            }
        }

        SetTasks(playScreenIcons, playScreenValues);

        if(remainSeconds == 180)
        {
            StartCoroutine(Timer());
        }

        if(tasks.Sum(task => task.value) == 0)
        {
            ShowResult();
        }
        else
        {
            SoundManager.Instance.Right();
        }

    }

    private void SetTasks(Image[] icons, Text[] valueLabels)
    {
        for (var i = 0; i < 3; i++)
        {
            icons[i].sprite = board.TileTypes[tasks[i].id].sprite;
            valueLabels[i].text = tasks[i].value.ToString();
        }
    }
    
    private void ShowResult()
    {
        resultScreen.SetActive(true);

        if (remainSeconds > 0) SoundManager.Instance.Win();
        else SoundManager.Instance.Lose();

        resultLable.text = remainSeconds > 0 ? "DONE!" : "TRY AGAIN!";

        resultStars[0].SetActive(remainSeconds > 0);
        resultStars[1].SetActive(remainSeconds > 60);
        resultStars[2].SetActive(remainSeconds > 90);

        PlayerPrefs.SetInt($"Level{currentLevel}", resultStars.Count(star => star.activeSelf));
    }

    int remainSeconds = 0;
    IEnumerator Timer()
    {
        while(remainSeconds > 0)
        {
            if(!pauseScreen.activeSelf)
            {
                timerLable.text = System.TimeSpan.FromSeconds(remainSeconds).ToString(@"mm\:ss");
                remainSeconds--;
            }

            yield return new WaitForSeconds(1f);
        }

        ShowResult();
    }
}
