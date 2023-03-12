using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace;

/// <summary>
/// 銃士の必殺技で放つ爆弾
/// </summary>
public class Bomb : MonoBehaviour
{
    //爆発オブジェクト
    [Header("爆発エフェクト")]
    [SerializeField] private GameObject explosionObject;
    //召喚するオブジェクト
    [Header("召喚するオブジェクト(爆弾)")]
    [SerializeField] private GameObject bombObject;
    //出現半径
    [Header("出現半径")]
    [Range(0.0f, 15.0f), SerializeField] private float spawnRadius = 5.0f;
    //レイを当てる対象のレイヤー
    [Header("爆弾を召喚するときの高さの上限を決めるレイヤー")]
    [SerializeField] private LayerMask layer;
    private IRole role;
    public IRole Role { set { role = value; } }
    //爆弾の最高高度
    private float maxHeight = 10.0f;
    private void Start()
    {
        
    }
    private void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Enemy"))
        {
            var obj = Instantiate(explosionObject,transform.position,Quaternion.identity);
            if (bombObject != null)
            {
                BombSpawn(obj.transform,25);
            }
            Destroy(obj, 0.2f);
        }
    }
    //必殺技で前方や後方に投げる
    private void BombSpawn(Transform spawnTransform = null, int count = 10)
    {
        RaycastHit hit;
        float maxY;       
        for (int i = 0; i < count; i++)
        {
            //範囲内のランダムな位置を設定
            var rayPos = UnityEngine.Random.insideUnitSphere * spawnRadius;
            Ray ray = new Ray(rayPos, spawnTransform.up);
            //レイで障害物に指定したレイヤーに当たった場合
            if (Physics.Raycast(ray, out hit, 100, layer))
            {
                maxY = hit.point.y;
            }
            else
            {
                maxY = maxHeight;
            }
            //高さを設定
            var randomy = UnityEngine.Random.Range(3, maxY);
            var ranpos = new Vector3(rayPos.x, randomy, rayPos.z);
            var pos = spawnTransform.position + ranpos;
            var obj = Instantiate(bombObject, pos, Quaternion.identity);
            var damage = obj.GetComponent<WeaponDamageStock>();
            if(damage != null)
            {
                damage.IsDeathBlow = true;
                damage.ReactivePlayer(role);
            }
        }
    }
}
