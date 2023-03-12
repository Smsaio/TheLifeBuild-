using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemySpace;
using GameManagerSpace;
namespace EnemySpace
{
    public enum CharacterState
    {
        Move,//移動
        Wait,//待ち
        Chase,//追跡
        Charge,//貯め時間
        Stan,//スタン状態
        DeathBlow,//奥義
        Damage,//ダメージ
        Attack,//通常攻撃
        Freeze,//硬直状態(攻撃後)
        Death,//死亡
        Guard,//防御する(ダメージが利かなくなる)
        Run,//逃走
    };
}
/// <summary>
/// 敵と味方とプレイヤーの基本変数や関数
/// </summary>
public class Character : MonoBehaviour,IReactiveProperty
{
    protected int currentHP = 0;
    public int CurrentHP { get { return currentHP; } set { currentHP = value; } }
    protected int currentMaxHP = 0;
    public int CurrentMaxHP { get { return currentMaxHP; } set { currentMaxHP = value; } }
    protected int attackP = 0;
    public int AttackP { get { return attackP; } set { attackP = value; } }
    protected int defenceP = 0;
    public int DefenceP { get { return defenceP; } set { defenceP = value; } }
    protected IRole role = default;
    public IRole Role { set { role = value; } }
    protected IGameManager gameManager = default;
    public IGameManager GameManager { set { gameManager = value; } get { return gameManager; } }
//-----------------敵や味方用の関数------------
    /// <summary>
    /// 状態を設定
    /// </summary>
    /// <param name="state">指定する状態</param>
    /// <param name="targetTransform">目標を感知した場合の目標の位置</param>
    public virtual void SetState(CharacterState state, Transform targetTransform = null)
    {

    }
    /// <summary>
    /// updateの中での動き
    /// </summary>
    protected virtual void StateMove()
    {

    }
    public virtual void ReactivePlayer(IRole Irole)
    {

    }
}
