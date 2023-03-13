using Ability;
using EnemySpace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Ability
{
    /// <summary>
    /// 特技のベースクラス
    /// </summary>
    public class SpecialityBase : MonoBehaviour,IReactiveProperty
    {
        //------共通-------------

        protected IRole role = default;
        public IRole Role { get { return role; } set { role = value; } }
        //-------------記憶停滞------------------
        //敵を止めてるときにダメージを与えた時の合計
        protected int damageStock;
        //止めてる間にダメージを与えたゲームオブジェクト
        protected List<GameObject> stopDamageObject = new List<GameObject>();
        //-------------記憶拒絶------------------
        [SerializeField] protected GameObject refusalArea;
        public GameObject RefusalArea { set { refusalArea = value; } get { return refusalArea; } }
        protected float refusalLimitSlowSpeed = 0.2f;
        public float RefusalLimitSlowSpeed { get { return refusalLimitSlowSpeed; } set { refusalLimitSlowSpeed = value; } }
        //-------------記憶転換------------------
        //特技のクールタイム設定
        [SerializeField] protected GameObject anuvisWeapon;
        public GameObject AnuvisWeapon { get { return anuvisWeapon; } set { anuvisWeapon = value; } }
        //味方になる最大数
        protected int maxFollowCharaCount = 3;
        public int MaxFollowCharaCount { get { return maxFollowCharaCount; } set { maxFollowCharaCount = value; } }
        //味方になった敵をプレイヤーに集める
        protected bool fellowAssembly = false;
        public bool FellowAssembly { get { return fellowAssembly; } set { fellowAssembly = value; } }

        //味方になった敵の番号0からスタート
        protected int fellowCount = 0;
        public int FellowCount { get { return fellowCount; } }
        //味方になった敵の配列
        protected List<FollowChara> followCharaList = new List<FollowChara>();
        public List<FollowChara> FollowCharaList { get { return followCharaList; } }
        //転換で出した武器のオブジェクト
        protected GameObject spawnAnuvisObject;
        //出現する位置
        protected Vector3 spawnPosition = Vector3.zero;
        [Inject]
        public void Construct(IRole Irole)
        {
            role = Irole;
        }
        private void Update()
        {

        }
        public virtual void ReactivePlayer(IRole Irole)
        {

        }
        /// <summary>
        /// 記憶停滞が終わった後に実行される
        /// </summary>
        public virtual void StopStay()
        {

        }
        /// <summary>
        /// 記憶停滞の際に与えたダメージをストック
        /// </summary>
        /// <param name="attackPoint"></param>
        /// <param name="other"></param>
        public virtual void EnemyDamageStock(int attackPoint, GameObject other)
        {

        }
        /// <summary>
        /// 味方に番号割り振り
        /// </summary>
        /// <param name="followChara"></param>

        public virtual void NumFellowCount(FollowChara followChara)
        {

        }
        /// <summary>
        /// 味方追加
        /// </summary>
        /// <param name="followObject"></param>
        public virtual void AddFellow(GameObject followObject)
        {

        }
        /// <summary>
        /// 味方削除
        /// </summary>
        /// <param name="followChara"></param>
        public virtual void RemoveFellow(FollowChara followChara)
        {

        }
        /// <summary>
        /// 特技終了時に実行される
        /// </summary>
        public virtual void DoneSpeciality()
        {

        }
        /// <summary>
        /// 特技を使用した際の実行される
        /// </summary>
        public virtual void UseSpeciality()
        {

        }
    }
}