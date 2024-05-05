using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource click;
    [SerializeField] private AudioSource right;
    [SerializeField] private AudioSource wrong;
    [SerializeField] private AudioSource win;
    [SerializeField] private AudioSource lose;

    public static SoundManager Instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public void SetVolume(float vol)
    {
        click.volume = vol;
        right.volume = vol;
        wrong.volume = vol;
        win.volume = vol;
        lose.volume = vol;
    }

    public void Click()
    {
        click.Play();
    }

    public void Right()
    {
        right.Play();
    }

    public void Wrong()
    {
        wrong.Play();
    }

    public void Win()
    {
        win.Play();
    }

    public void Lose()
    {
        lose.Play();
    }
}
