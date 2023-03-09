using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ReactiveProperty用関数定義クラス
/// </summary>
public class ReactivePropertyController: MonoBehaviour
{
    /// <summary>
    /// プレイヤー関連クラス監視用関数
    /// </summary>
    /// <param name="role"></param>
    public virtual void ReactivePlayer(IRole role)
    {
        if (role == null) return;
    }
}
