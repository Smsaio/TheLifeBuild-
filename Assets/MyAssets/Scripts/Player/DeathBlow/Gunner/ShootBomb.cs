using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerSpace;
using Zenject;

/// <summary>
/// 爆弾を投げる
/// </summary>
public class ShootBomb : MonoBehaviour
{
    #region シリアライズ
    // 爆弾のPrefab
    [SerializeField, Tooltip("弾のPrefab")] private GameObject bombPrefab;
    // 弾の速さ
    [SerializeField, Range(10.0F, 50.0F), Tooltip("弾の射出する速さ")] private float shootingSpeed = 1.0f;
    #endregion
    #region プロパティ
    //投げる準備(放物線描画)
    private bool throwPre = false;
    public bool ThrowPre { get { return throwPre; }}
    // 弾を生成する位置情報
    private Vector3 instantiatePosition;
    // 弾の生成座標(読み取り専用)
    public Vector3 InstantiatePosition { get { return instantiatePosition; } }
    // 弾の初速度
    private Vector3 shootVelocity;
    // 弾の初速度(読み取り専用)
    public Vector3 ShootVelocity{get { return shootVelocity; }}
#endregion
    private GunnerPlayerMove gunnerPlayerMove;
    private IRole role = default;
    [Inject]
    public void Construct(IRole Irole)
    {
        role = Irole;
    }
    private void Start()
    {
        gunnerPlayerMove = GetComponent<GunnerPlayerMove>();
    }
    void Update()
    {
        BombThrowPreparation();
    }
    /// <summary>
    /// 爆弾の方向を決める
    /// </summary>
    private void BombThrowPreparation()
    {        
        if (gunnerPlayerMove.IsDeathBlow)
        {
            // 弾の初速度を更新
            shootVelocity = gunnerPlayerMove.ThrowPoint.transform.up * shootingSpeed;

            // 弾の生成座標を更新
            instantiatePosition = gunnerPlayerMove.ThrowPoint.transform.position;
        }
    }
    /// <summary>
    /// 爆弾を投げる
    /// </summary>
    public void BombThrow()
    {        
        // 弾を生成して飛ばす
        GameObject obj = Instantiate(bombPrefab, instantiatePosition, Quaternion.identity);
        var bomb = obj.GetComponent<Bomb>();
        var rb = obj.GetComponent<Rigidbody>();
        var damage = obj.GetComponent<WeaponDamageStock>();
        rb.AddForce(shootVelocity * shootingSpeed);
        if (bomb != null)
        {
            bomb.Role = role;
            if (damage != null)
            {
                //ダメージ処理のクラスにプレイヤーを設定
                damage.ReactivePlayer(role);
                damage.IsDeathBlow = true;
            }
        }
    }    
}
