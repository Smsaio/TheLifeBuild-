using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "ScriptableObjects/CreateEnemyStatus")]
public class EnemyStatus : ScriptableObject
{
    /// <summary> 名前 </summary>
    public string enemyName;
    /// <summary> 最大の体力 </summary>
    public int maxHP;
    /// <summary> 攻撃力 </summary>
    public int attackP;
    /// <summary> 防御力 </summary>
    public int defenceP;
    ///<summary> 移動速度 </summary>
    public float agentWalkSpeed;
    ///<summary> 移動速度 </summary>
    public float agentAngularSpeed = 120;
    ///<summary> 移動速度 </summary>
    public float agentAcceleration = 10;
    /// <summary> 敵の番号 </summary>
    public int enemyNo;
    /// <summary> 敵の番号 </summary>
    public int startEXP = 50;
}
