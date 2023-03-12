using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemySpace;
using PlayerSpace;
using Ability;
using Zenject;
using UniRx;
namespace Ability
{
    /// <summary>
    /// 記憶転換専用の武器につける。武器に引き寄せられた敵が触れた場合、味方に変わる。
    /// </summary>
    public class Anuvis : MonoBehaviour,IReactiveProperty
    {
        //味方に変わる中心からの距離
        [Header("味方になる距離")]
        [Range(0.1f, 3.0f), SerializeField] private float fellowDistance = 2.0f;
        //アヌビスで引き寄せられる力 
        [Header("引力の増加率")]
        [SerializeField] private float magnetPower = 5;
        private Rigidbody rb;
        public Rigidbody Rigid { get { return rb; } }
        //現在味方に変えた人数
        private int changeFellowCount = 0;
        private PlayerSpecialityController specialityController;
        private IRole role = default;
        [Inject]
        public void Construct(IRole Irole)
        {
            role = Irole;
        }

        // Start is called before the first frame update

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            ReactivePlayer(role);
        }
        // Update is called once per frame
        void Update()
        {

        }
        public void ReactivePlayer(IRole role)
        {
            if (role == null) return;
            role.CurrentPlayerSpController.Subscribe(spController => { specialityController = spController; }).AddTo(this);
        }
        void OnTriggerEnter(Collider col)
        {
            if (this.gameObject.activeSelf) return;
            if (col.transform.root.gameObject.CompareTag("Enemy"))
            {
                var enemybase = col.gameObject.transform.root.GetComponent<EnemyBase>();
                if (enemybase != null)
                {
                    //敵が範囲内に入ってきたか
                    enemybase.InAnuvisArea = true;
                    //アヌビス本体を敵が参照する
                    enemybase.AnuvisObject = gameObject;
                    //引力の式に代入
                    enemybase.AnuvisPower = magnetPower;
                }
            }
        }
        void OnTriggerStay(Collider col)
        {
            if (specialityController != null && this.gameObject.activeSelf)
            {
                if (col.gameObject.CompareTag("Search")) return;
                //索敵したオブジェクトが敵だった場合かつ、味方の数が最大に達した場合
                if (col.transform.root.gameObject.CompareTag("Enemy") && changeFellowCount < specialityController.SpecialityBases[(int)Ability.Attach.Speciality.Convert].MaxFollowCharaCount)
                {
                    //中心からの距離が味方になる距離であれば
                    if ((col.transform.root.transform.position - transform.position).magnitude < fellowDistance)
                    {
                        //敵のクラスを取得した場合
                        var enemybase = col.gameObject.transform.root.GetComponent<EnemyBase>();
                        if (enemybase != null)
                        {
                            //味方にすることが可能な敵なら目標が変わる
                            enemybase.TargetChange();
                            changeFellowCount++;
                        }
                    }
                }
            }
        }
    }
}