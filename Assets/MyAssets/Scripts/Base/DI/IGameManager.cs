using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameManagerSpace
{
    //ÉQÅ[ÉÄÉÇÅ[Éhê›íË
    public enum GameMode : int
    {
        Title,
        Game,
        BossDefeat,
        GameClear,
        GameOver
    }
    public interface IGameManager
    {
        public GameMode CurrentGameMode { get; set; }
        public bool IsGameOver { get;  }
        public bool IsStageClear { get; set; }
        public GameModeTextScriptable[] GameModeScriptable { get; }
        public int StageNum { get; set; }
        public void StateReset();
        public void SetGameMode(GameMode mode);
    }
}