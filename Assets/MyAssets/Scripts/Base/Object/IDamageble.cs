using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageble
{
    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="attackPoint"></param>
    /// <param name="playDamageAnim"></param>
    /// <param name="memoryType"></param>
    /// <param name="isBodyHit"></param>
    public void ReceiveDamage(int attackPoint,
        bool playDamageAnim = true, MemoryType.MemoryClassification memoryType = MemoryType.MemoryClassification.None,
        bool isBodyHit = false);
    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="attackPoint"></param>
    /// <param name="damage"></param>
    /// <param name="playDamageAnim"></param>
    public void ReceiveDamage(int attackPoint, WeaponDamageStock damage, bool playDamageAnim);
}
