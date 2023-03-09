using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using GameManagerSpace;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceManager : MonoBehaviour,IAudioSourceManager
{
    //各場面ごとのBGM
    [NamedArray(new string[] { "Title", "Game", "GameOver", "GameClear" }), Header("各モードごとのBGM"),SerializeField]
    private AudioSource[] BGMLists;
    private AudioSource audioSource;
    IGameManager gameManager = default;

    [Inject]
    public void Construct(IGameManager IgameManager)
    {
        gameManager = IgameManager;
    }
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        BGMChange();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    ///<summary>
    ///SEを鳴らす
    ///</summary> 
    ///<param name="clip"></param>
    public void PlaySE(AudioClip clip)
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.Log("オーディオソースが設定されてない");
        }
    }
    ///<summary>
    ///SEを鳴らす
    ///</summary> 
    public void StopSE()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        else
        {
            Debug.Log("オーディオソースが設定されてない");
        }
    }
    public void BGMChange()
    {
        for(int i=0; i < BGMLists.Length; i++)
        {
            BGMLists[i].Stop();
        }
        BGMLists[(int)gameManager.CurrentGameMode].Play();
    }
}
