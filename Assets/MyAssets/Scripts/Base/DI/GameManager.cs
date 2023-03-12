using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Ability;
using PlayerSpace;
using TMPro;
using Zenject;

namespace GameManagerSpace
{
    /// <summary>
    /// ゲームの中で共通で必要なもの
    /// </summary>
    public class GameManager : MonoBehaviour,IGameManager
    {
        //現在の目的
        [SerializeField] private TextMeshProUGUI currentPurPoseText;
        //ボス倒した後の目的
        [Header("目的の文言"),NamedArray(new string[] { "Title", "Game" , "BossDefeat" ,"GameClear","GameOver"}),SerializeField]
        private GameModeTextScriptable[] gameModeScriptable;
        public GameModeTextScriptable[] GameModeScriptable { get { return gameModeScriptable; } }
        //ゲームモード
        [Header("ゲームモード"),SerializeField]private GameMode gameMode = GameMode.Game;
        public GameMode CurrentGameMode { get { return gameMode; } set { gameMode = value; } }
        //現在のステージのナンバリング
        [Header("現在のステージ"),SerializeField] private int stageNum = 1;
        public int StageNum { get { return stageNum; }set { stageNum = value; } }
        private bool isGameOver = false;
        public bool IsGameOver { get { return isGameOver; } }
        private bool isStageClear = false;
        public bool IsStageClear { get { return isStageClear; } set { isStageClear = value; } }
        protected IAudioSourceManager audioSourceManager = default;
        [Inject]
        public void Construct(IAudioSourceManager IaudioSourceManager)
        {
            audioSourceManager = IaudioSourceManager;
        }
        private void OnEnable()
        {

        }
        
        private void Start()
        {
            
        }
        private void Update()
        {

        }
        /// <summary> 
        /// 最初から始める時の処理
        /// </summary> 
        public void RetryGame()
        {
            isGameOver = false;
            Time.timeScale = 1.0f;
            stageNum = 1;
        }
        public void StateReset()
        {
            isGameOver = false;
            isStageClear = false;
        }
        public void SetGameMode(GameMode mode)
        {
            if (gameMode == mode) return;
            gameMode = mode;
            if(mode == GameMode.GameOver)
            {
                isGameOver = true;
            }
            else if(mode == GameMode.GameClear)
            {
                isStageClear = true;
            }
            GameStateText(gameMode);
            audioSourceManager.BGMChange(mode);
        }
        private void GameStateText(GameMode gameMode)
        {
            if (currentPurPoseText == null) return;
            switch (gameMode)
            {
                case GameMode.Game:
                    currentPurPoseText.text = gameModeScriptable[(int)GameMode.Game].gameModeText;
                    currentPurPoseText.color = Color.yellow;
                    break;
                case GameMode.BossDefeat:
                    currentPurPoseText.text = gameModeScriptable[(int)GameMode.BossDefeat].gameModeText;
                    currentPurPoseText.color = Color.blue;
                    break;
                case GameMode.GameClear:
                    currentPurPoseText.text = gameModeScriptable[(int)GameMode.GameClear].gameModeText;
                    currentPurPoseText.color = Color.blue;
                    break;
                case GameMode.Title:
                    currentPurPoseText.text = gameModeScriptable[(int)GameMode.Title].gameModeText;
                    break;
                case GameMode.GameOver:
                    currentPurPoseText.text = gameModeScriptable[(int)GameMode.GameOver].gameModeText;
                    currentPurPoseText.color = Color.red;
                    break;
                default:
                    Debug.LogError("ゲームの状況に関するエラーです。ゲームマネージャーを確認してください。");
                    break;
            }
        }
    }
}