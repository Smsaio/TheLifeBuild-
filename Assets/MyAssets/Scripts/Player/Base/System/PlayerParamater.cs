using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerParamater", menuName = "ScriptableObjects/PlayerParamater")]
public class PlayerParamater : ScriptableObject
{
    /// <summary> プレイヤーの最大攻撃力 </summary>
    public int maxAttackPoint;
    /// <summary> プレイヤーの最大体力 </summary>
    public int maxHP;
    /// <summary> プレイヤーの最大防御力 </summary>
    public int maxDefencePoint;
    /// <summary> プレイヤーの歩行速度 </summary>
    public float moveSpeed = 18.0f;
}