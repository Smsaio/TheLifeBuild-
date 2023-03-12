using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioSourceManager 
{
    public void BGMChange(GameManagerSpace.GameMode gameMode);
    public void PlaySE(AudioClip clip);
    public void StopSE();
}
