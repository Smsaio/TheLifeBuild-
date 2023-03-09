using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace;

/// <summary>
/// 範囲内の敵の近くにスタンドを出現させる(スタンド使いの必殺技)
/// </summary>
public class ScanAreaEnemy : MonoBehaviour
{
    //必殺技で出すスラッシュ
    [SerializeField] private GameObject deathBlowEffect;
    //スタンドのプレイヤー
    [SerializeField] private StandPlayerMove sPlayerMove;
    [Range(0.5f,2.5f),SerializeField] private float destroyTime = 1.5f;
    //出現範囲最大値
    [SerializeField] private float maxRadius = 20.0f;
    //増加幅
    [SerializeField] private float plusWidthRadius = 0.1f;
    [SerializeField] private Player player;
    //発動時拡大する出現範囲
    private SphereCollider spawnCollider;
    private Vector3 beforeRadius;
    //サイズを動かしているか
    private bool isSizeChange = false;
    // Start is called before the first frame update
    void Start()
    {
        spawnCollider = GetComponent<SphereCollider>();
        beforeRadius = transform.localScale;
        spawnCollider.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        AreaSearch();
    }
    /// <summary>
    /// 押している長さによって索敵範囲が広がる
    /// </summary>
    private void AreaSearch()
    {
        //必殺技発動中
        if (sPlayerMove.IsDeathBlow)
        {
            isSizeChange = true;
        }
        //すべての要素が最大のサイズに達していない場合
        if (transform.localScale.x < maxRadius && transform.localScale.y < maxRadius && transform.localScale.z < maxRadius)
        {
            if (isSizeChange)
            {
                //徐々に拡大
                var plusX = transform.localScale.x <= maxRadius ? transform.localScale.x + plusWidthRadius : maxRadius;
                var plusY = transform.localScale.y <= maxRadius ? transform.localScale.y + plusWidthRadius : maxRadius;
                var plusZ = transform.localScale.z <= maxRadius ? transform.localScale.z + plusWidthRadius : maxRadius;
                transform.localScale = new Vector3(plusX, plusY, plusZ);
                //すべての要素が最大のサイズに達した場合
                if (transform.localScale.x >= maxRadius && transform.localScale.y >= maxRadius && transform.localScale.z >= maxRadius)
                {
                    ToBeforeSize();
                }
            }
        }

    }
    /// <summary>
    /// サイズを元に戻す
    /// </summary>
    private void ToBeforeSize()
    {
        //元に戻して消える
        isSizeChange = false;
        transform.localScale = beforeRadius;
        gameObject.SetActive(false);        
    }
    private void OnTriggerEnter(Collider other)
    {
        //敵を確認できたのと人数が最大に達していないとき
        if (other.gameObject.CompareTag("Enemy"))
        {
            //必殺技発動時で範囲が最大になっていない場合
            if (transform.localScale.x <= maxRadius && transform.localScale.y <= maxRadius && transform.localScale.z <= maxRadius)
            {
                //敵の位置を取得
                var targetTransform = other.gameObject.transform;
                var diff = targetTransform.position - sPlayerMove.transform.position;
                Vector3 plusPos = new Vector3(0, 2.0f, 0);
                //敵の正面に出現
                Vector3 pos = targetTransform.position + (plusPos + Vector3.Normalize(diff));
                var axis = Vector3.Cross(sPlayerMove.transform.forward, diff);
                var angle = Vector3.Angle(sPlayerMove.transform.forward, diff) * (axis.y < 0 ? -1 : 1);
                Debug.Log(pos);
                //エフェクト出現
                var obj = Instantiate(deathBlowEffect, pos, Quaternion.Euler(0,angle,0));
                //攻撃準備完了
                obj.GetComponent<WeaponDamageStock>().IsAttack = true;
                //必殺技の場合
                obj.GetComponent<WeaponDamageStock>().IsDeathBlow = true;
                //プレイヤー設定
                obj.GetComponent<WeaponDamageStock>().Player = player;
                Destroy(obj,destroyTime);
            }            
        }
    }
}
