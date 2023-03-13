using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using PlayerSpace;
using System;
using UniRx;
using TMPro;
using GameManagerSpace;
using Zenject;

namespace PlayerSpace
{
    //ゲーム画面での職業制御
    public class Role : MonoBehaviour,IRole
    {
        /// <summary>
        /// プレイヤーの種類
        /// </summary>   
        [NamedArray(new string[] { "SwordMan", "Gunner", "DemonPos" }),SerializeField] private GameObject[] playerObjects = new GameObject[Enum.GetValues(typeof(Attach.Role)).Length];
        //役割のスクリプタブル
        [SerializeField] private RoleScriptable roleScriptable;
        //プレイヤーの最大ステータス
        [SerializeField] private PlayerParamater[] playerParamaters = new PlayerParamater[Enum.GetValues(typeof(Attach.Role)).Length];
        public PlayerParamater[] PlayerParamaters { get { return playerParamaters; } }        

        //始めた職業ごとのレベルの配列。( (None)何もない状態があるため-1 )
        [NamedArray(new string[] { "SwordMan", "Gunner", "DemonPos" }), Header("職業ごとのレベルを格納する配列"),SerializeField]
        private int[] playerLevelList = new int[Enum.GetValues(typeof(Attach.Role)).Length];
        public int[] PlayerLevelList { get { return playerLevelList; } set { playerLevelList = value;} }
        //選択した役割に設定された番号
        private int roleNum = 0;
        public int RoleNumber { get { return roleNum; } }
        //プレイヤーの各クラスの配列
        private Player[] players = new Player[Enum.GetValues(typeof(Attach.Role)).Length];
        public Player[] Players { get { return players; } }
        private PlayerSpecialityController[] playerSpecialityControllers = new PlayerSpecialityController[Enum.GetValues(typeof(Attach.Role)).Length];
        public PlayerSpecialityController[] PlayerSpController { get { return playerSpecialityControllers; } }
        private PlayerMove[] playerMoves = new PlayerMove[Enum.GetValues(typeof(Attach.Role)).Length];
        public PlayerMove[] PlayerMoves { get { return playerMoves; } }

        //現在のプレイヤーのクラス
        private ReactiveProperty<int> currentPlayerLevel = new();
        public ReactiveProperty<int> CurrentPlayerLevel { get { return currentPlayerLevel; } set { currentPlayerLevel = value; } }
        

        private ReactiveProperty<Player> currentPlayer = new();
        public ReactiveProperty<Player> CurrentPlayer { get { return currentPlayer; } }

        private ReactiveProperty<PlayerMove> currentPlayerMove = new();
        public ReactiveProperty<PlayerMove> CurrentPlayerMove { get { return currentPlayerMove; } }

        private ReactiveProperty<PlayerSpecialityController> currentPlayerSpController = new ReactiveProperty<PlayerSpecialityController>();
        public ReactiveProperty<PlayerSpecialityController> CurrentPlayerSpController { get { return currentPlayerSpController; } }
        private ReactiveProperty<Transform> currentPlayerTransform = new();
        public ReactiveProperty<Transform> CurrentPlayerTransform { get { return currentPlayerTransform; } }
        private ReactiveProperty<bool> playerIsMenu = new();
        public ReactiveProperty<bool> PlayerIsMenu { get { return playerIsMenu; } }
        //役割が変わったか
        private bool isRoleChange = false;
        public bool IsRoleChange { set { isRoleChange = value; } get { return isRoleChange; } }
        private IGameManager gameManager;

        [Inject]
        public void Construct(IGameManager IgameManager)
        {
            gameManager = IgameManager;
        }

        void Awake()
        {
            if (gameManager.CurrentGameMode == GameMode.Game)
            {
                InitializeLevel();

                InitializeRole();
                ActiveInitialization();
            }
        }
        void Start()
        {
            
        }
        void Update()
        {
            
        }
        //役割適応
        public void InitializeRole()
        {
            int length = Enum.GetValues(typeof(Attach.Role)).Length;
            //プレイヤーに関するクラス取得、ステータス初期化
            for (int i = 0; i < length; i++)
            {
                if (playerObjects[i] != null)
                {
                    players[i] = playerObjects[i].GetComponent<Player>();
                    players[i].StatusInitialization(playerParamaters[roleNum]);
                    playerMoves[i] = playerObjects[i].GetComponent<PlayerMove>();
                    playerSpecialityControllers[i] = playerObjects[i].GetComponent<PlayerSpecialityController>();
                }
            }
            //スタート画面で役割設定をしていた場合
            Attach.role = roleScriptable.role;
            roleNum = (int)Attach.role;
            //プレイヤーの中に選択した役割の番号と要素番号が一致した場合
            playerObjects[roleNum].SetActive(true);
            //現在のプレイヤークラス購読
            currentPlayer.Value = players[roleNum];
            currentPlayerMove.Value = playerMoves[roleNum];
            currentPlayerSpController.Value = playerSpecialityControllers[roleNum];
            currentPlayerTransform.Value = playerObjects[roleNum].transform;
            //個別にやっているが、一括で取得して関数を実行した方がいいのかは迷い中
            var list = GameExtensions.FindObjectOfInterfaces<IReactiveProperty>();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].ReactivePlayer(this);
            }
        }
        /// <summary>
        /// 役割をテキストに反映
        /// </summary>
        /// <param name="role">役割</param>
        /// <returns></returns>
        public string RoleToText(Attach.Role role)
        {
            string text;
            switch (role)
            {
                case Attach.Role.Swordman:
                    text = "剣士";
                    break;
                case Attach.Role.Gunner:
                    text = "銃士";
                    break;
                case Attach.Role.DemonPos:
                    text = "悪魔憑き";                    
                    break;
                default:
                    text = "役割無し";
                    break;
            }
            return text;
        }

        /// <summary>
        /// レベルの初期化
        /// </summary>
        private void InitializeLevel(int initLevel = 1)
        {
            for (int i = 0; i < playerLevelList.Length; i++)
            {
                playerLevelList[i] = initLevel;
            }
            currentPlayerLevel.Value = playerLevelList[roleNum];
        }
        /// <summary>
        /// 最初に選んだキャラだけ表示
        /// </summary>
        private void ActiveInitialization()
        {            
            for (int i = 0; i < playerObjects.Length; i++)
            {                
                if (i != roleNum)
                {
                    playerObjects[i].SetActive(false);
                }
            }
        }
        /// <summary>
        /// 役割変換
        /// </summary>
        public void RoleChange()
        {
            isRoleChange = true;
            //役割を変えようとしているとき
            if (isRoleChange)
            {
                //次の要素番号を計算
                //役割で分けられているプレイヤーの体力があればそのまま変える
                if (players[roleNum].CurrentHP > 0)
                {
                    Changer(roleNum);
                }
                else
                {
                    //無ければ全体から探す
                    for (int i = 0; i < players.Length; i++)
                    {
                        //体力が残っていて、次に変更しようしている役割の番号ではなかったら
                        if (players[roleNum].CurrentHP > 0 && roleNum != i)
                        {
                            Changer(i);
                        }
                    }
                }
                isRoleChange = false;
            }
        }
        /// <summary>
        ///次に変わる予定の役割があるかどうかは変えるときにすでにしているので
        ///ここでしているのは、変更する機能のみ 
        /// </summary>
        /// <param name="num"></param>
        private void Changer(int num)
        {
            //現在のプレイヤーの要素数を取得
            int beforeNum = num;
            roleNum = num < players.Length - 1 ? num + 1 : 0;
            //現在の役割のプレイヤーを非アクティブ化
            playerObjects[beforeNum].SetActive(false);
            //味方リスト引き換え
            playerSpecialityControllers[roleNum].SpecialityBases[(int)Ability.Attach.Speciality.Convert].FollowCharaList.Clear();
            playerSpecialityControllers[roleNum].SpecialityBases[(int)Ability.Attach.Speciality.Convert].FollowCharaList.AddRange(playerSpecialityControllers[beforeNum].SpecialityBases[(int)Ability.Attach.Speciality.Convert].FollowCharaList);            
            //次のプレイヤーアクティブ化
            playerObjects[roleNum].SetActive(true);
            //現在のプレイヤーを設定
            currentPlayer.Value = players[roleNum];
            currentPlayerLevel.Value = players[roleNum].CurrentLevel;
            playerLevelList[roleNum] = currentPlayerLevel.Value;
            currentPlayerMove.Value = playerMoves[roleNum];
            currentPlayerSpController.Value = playerSpecialityControllers[roleNum];
            currentPlayerTransform.Value = players[roleNum].gameObject.transform;
            //現在のプレイヤーの位置に変更予定の位置に設定
            players[roleNum].transform.position = players[beforeNum].transform.position;
            players[roleNum].transform.rotation = players[beforeNum].transform.rotation;
            Attach.role = (Attach.Role)roleNum;
            isRoleChange = false;
        }
    }
}