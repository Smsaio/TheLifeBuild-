using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManagerSpace;

[CreateAssetMenu(fileName = "GameModeText", menuName = "ScriptableObjects/GameModeText")]
public class GameModeTextScriptable : ScriptableObject
{
    public GameMode gameMode;
    public string gameModeText;
    public string sceneName;
}
