using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace;
using UniRx;

public interface IRole
{
    //プレイヤーのスクリプト
    public Player[] Players { get; }
    public PlayerSpecialityController[] PlayerSpController { get; }
    public PlayerMove[] PlayerMoves { get; }
    public ReactiveProperty<int> CurrentPlayerLevel { get; }
    public ReactiveProperty<Transform> CurrentPlayerTransform { get; }
    public ReactiveProperty<Player> CurrentPlayer { get; }
    public ReactiveProperty<PlayerSpecialityController> CurrentPlayerSpController { get; }
    public ReactiveProperty<PlayerMove> CurrentPlayerMove { get; }         
    //プレイヤーの最大ステータス
    public PlayerParamater[] PlayerParamaters { get; }
    //プレイヤーのレベルの配列
    public int[] PlayerLevelList { get; set; }
    //役割の番号
    public int RoleNumber { get; }
    //役割変更中
    public bool IsRoleChange { get; set; }
    //役割変更
    public void RoleChange();
}
