using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace;
using EnemySpace;
using Zenject;
using UniRx;

public class WeaponDamageStock : ReactivePropertyController
{
    [System.Serializable]
    public class BulletVersion
    {

        //遠距離攻撃で当たった時のエフェクト
        public GameObject hitParticle;
        //弾の速度
        public float bulletSpeed;
        //弾が破壊される時間
        public float bulletDestroyTime;
    }
    //自分の攻撃がプレイヤーから離れるタイプだった場合
    public bool isBullet = false;
    //遠距離
    public BulletVersion bulletVersion = new BulletVersion();
    [SerializeField] private int damageAttackPoint = 50;
    public int DamageAttackPoint { get { return damageAttackPoint; } set { damageAttackPoint = value; } }
    //味方のクラスが起動しているとき
    [SerializeField] private bool isFellow;
    public bool IsFellow { get { return isFellow; } set { isFellow = value; } }

    private Player player;
    //遠距離の場合や参照できない場合はプロパティを参照してプレイヤーのクラスを参照する
    public Player Player { set { player = value; } get { return player; } }
    //エフェクトで攻撃をする際の攻撃オンオフ
    private bool isAttack = false;
    public bool IsAttack { get { return isAttack; } set { isAttack = value; } }
    //エフェクトで攻撃をする際の攻撃オンオフ
    private bool isDeathBlow = false;
    public bool IsDeathBlow { get { return isDeathBlow; } set { isDeathBlow = value; } }
    private FollowChara followChara;
    private PlayerSpecialityController specialityController;
    private void Awake()
    {
        if(isFellow)
            this.enabled = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        //味方用
        if (isFellow)
        {
            followChara = transform.root.GetComponent<FollowChara>();
            transform.tag = "PlayerAttack";
        }
        if (!isBullet)
        {
            //ReactivePlayer(role);
        }
    }
    private void Update()
    {
        
    }
    public override void ReactivePlayer(IRole role)
    {
        base.ReactivePlayer(role);
        if (role == null) return;
        role.CurrentPlayerSpController.Subscribe(spController => { specialityController = spController; }).AddTo(this);
        role.CurrentPlayer.Subscribe(value => { player = value; }).AddTo(this);
    }
    /// <summary>
    /// 遠距離攻撃
    /// </summary>
    /// <param name="damage">当たった敵のダメージインターフェイス</param>
    /// <param name="damagepoint">与えるダメージ</param>
    void BulletAttack(IDamageble damage, int damagepoint)
    {
        GameObject explode;
        //ヒットパーティクルがあるなら爆発エフェクトを召喚
        if (bulletVersion.hitParticle != null)
        {
            explode = Instantiate(bulletVersion.hitParticle, transform.position, Quaternion.identity);
            Destroy(explode, 0.1f);
        }
        //当たった敵にダメージを与える
        damage.ReceiveDamage(damagepoint,this, true);
    }
    /// <summary>
    /// 攻撃が当たった場合
    /// </summary>
    /// <param name="other">当たったオブジェクト</param>
    /// <param name="attackPoint">攻撃力</param>
    private void AttackHit(GameObject other, int attackPoint)
    {
        //敵に武器を当てたら
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            //敵のスクリプトを呼び出し
            var enemyBase = other.GetComponent<EnemyBase>();
            //敵停止中は連続で当たってしまうため弱めに
            int stockPoint = attackPoint / 2;
            //ポーズ中
            if (Pauser.isPause)
            {
                //記憶停滞のクラスから停滞専用のダメージをストックする関数を呼び出す
                int index = (int)Ability.Attach.Speciality.Stagnation;
                specialityController.SpecialityBases[index].EnemyDamageStock(stockPoint, other);
            }
            else
            {
                //敵のスクリプトの関数を呼び出し、ダメージを追加する
                if (enemyBase != null)
                {
                    //遠距離でなければ
                    if(!isBullet)
                    {
                        //敵の体力がある場合
                        if (enemyBase.CurrentHP > 0)
                        {
                            enemyBase.ReceiveDamage(attackPoint,this,true);
                        }
                    }
                    else
                    {
                        Debug.Log("銃弾");
                        //遠距離攻撃
                        //敵の体力がある場合
                        if (enemyBase.CurrentHP > 0)
                        {
                            BulletAttack(enemyBase, attackPoint);
                        }
                    }
                }
            }
        }
        else if(other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            //遠距離攻撃の場合、壁や地面に当たった場合、破壊
            if (isBullet)
            {
                Destroy(gameObject);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (this.enabled)
        {
            if (!other.gameObject.CompareTag("Search"))
            {
                if (player != null)
                {
                    AttackHit(other.transform.root.gameObject, player.AttackP);
                }
                else if (followChara != null)
                {
                    AttackHit(other.transform.root.gameObject, followChara.AttackP);
                }
                else
                {
                    AttackHit(other.transform.root.gameObject, damageAttackPoint);
                }
            }            
        }
    }
    //パーティクルにつけるとき用
    private void OnParticleCollision(GameObject other)
    {
        int deathBlowPoint;
        //起動している、そして攻撃した
        if (this.enabled && isAttack)
        {            
            if (!other.CompareTag("Search"))
            {
                if (player != null)
                {
                    deathBlowPoint = isDeathBlow ? player.AttackP * 6 : player.AttackP;
                    AttackHit(other.transform.root.gameObject, deathBlowPoint);
                }
                else if (followChara != null)
                {
                    AttackHit(other.transform.root.gameObject, followChara.AttackP);
                }
                else
                {
                    AttackHit(other.transform.root.gameObject, damageAttackPoint);
                }
            }
            
        }
    }
}