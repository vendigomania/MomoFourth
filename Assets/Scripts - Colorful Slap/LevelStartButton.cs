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

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    private void OnEnable()
    {
        
    }

    public void Launch()
    {

    }
}
