using UnityEngine;
using System.Collections;
using EnemySpace;
using Zenject;
/// <summary>
/// 敵のアニメーションイベントコライダーなどのオンオフ関連
/// </summary>
public class ProcessEnemyAnim : MonoBehaviour
{
    //攻撃コライダー
    [SerializeField] private Collider basicCollider;
    //攻撃音
    [SerializeField] private AudioClip attackSE;
    //敵の基クラス
    private EnemyBase enemyBase;
    //味方かどうか
    private bool isFellow = false;
    protected virtual void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        basicCollider.enabled = false;
    }
    protected virtual void Update()
    {
        if (enemyBase != null)
        {
            //敵の攻撃中に味方に変わる可能性があるのでコリジョンを消す
            if (enemyBase.OnFellow && !isFellow)
            {
                isFellow = true;
                AttackEnd();
            }
        }
    }
//--------アニメーションイベント---------
    /// <summary>
    /// 攻撃コライダーオン
    /// </summary>
    public virtual void AttackStart()
    {
        basicCollider.enabled = true;
    }
    /// <summary>
    /// 攻撃コライダーオフ
    /// </summary>
    public virtual void AttackEnd()
    {
        basicCollider.enabled = false;
        StateEnd();
    }
    /// <summary>
    /// 状態終了(攻撃後の硬直)
    /// </summary>
    public virtual void StateEnd()
    {
        enemyBase.SetState(CharacterState.Freeze);
    }
    /// <summary>
    /// ダメージアニメーション終了
    /// </summary>
    public virtual void DamageEnd()
    {
        enemyBase.SetState(CharacterState.Move);     
    }
    /// <summary>
    /// 攻撃のSE
    /// </summary>
    /// <param name="attackSound">攻撃音</param>
    protected virtual void AttackSE(AudioClip attackSound = null)
    {
        var sound = attackSound == null ? attackSE : attackSound;
        enemyBase.AudioSourceManager.PlaySE(sound);
    }
}
