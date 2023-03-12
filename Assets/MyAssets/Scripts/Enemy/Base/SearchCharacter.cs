using UnityEngine;
using System.Collections;
using EnemySpace;
using PlayerSpace;
using Zenject;
using GameManagerSpace;

namespace EnemySpace
{
    /// <summary>
    /// ターゲットを探す際のクラス(見回り用)
    /// </summary>
    public class SearchCharacter : MonoBehaviour
    {
        //見つけた時に出るマーク
        [SerializeField] private FindMark findMark;
        [SerializeField] private EnemyBase enemyBase;
        [SerializeField] private string[] fellowHitTag = { "Enemy", "Boss" };
        [SerializeField] private string[] enemyHitTag = { "Fellow", "Player" };
        //索敵用のコライダー
        private BoxCollider searchCollider;
        private ITargetSearch targetSearch;
        public ITargetSearch TargetSearch { get { return targetSearch; } set { targetSearch = value; } }
                
        void Start()
        {
            searchCollider = GetComponent<BoxCollider>();
            if(enemyBase == null)
            {
                enemyBase = transform.root.GetComponent<EnemyBase>();
            }
            targetSearch = enemyBase;
        }
        void Update()
        {

        }
        public void ScaleCollider(float deathBlowSize)
        {
            if (searchCollider != null)
            {
                searchCollider.size = new Vector3(deathBlowSize, 1, deathBlowSize);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (this.enabled)
            {
                if (targetSearch.MyCharacterType == CharacterType.Enemy)
                {
                    if (other.gameObject.CompareTag(enemyHitTag[0]) 
                        || other.gameObject.CompareTag(enemyHitTag[1]))
                    {
                        targetSearch.TargetFind(other.gameObject);
                    }
                }
                else if (targetSearch.MyCharacterType == CharacterType.Fellow)
                {
                    if (other.gameObject.CompareTag(fellowHitTag[0])
                        || other.gameObject.CompareTag(fellowHitTag[1]))
                    {
                        findMark.SetSize();
                        targetSearch.TargetFind(other.gameObject);
                    }
                }
            }
        }
        void OnTriggerStay(Collider other)
        {
            if (this.enabled)
            {
                if (targetSearch.MyCharacterType == CharacterType.Enemy)
                {
                    //　プレイヤーキャラクターを発見
                    if (other.gameObject.CompareTag(enemyHitTag[0]) 
                        || other.gameObject.CompareTag(enemyHitTag[1]))
                    {
                        targetSearch.TargetFind(other.gameObject);
                    }
                }
                else if (targetSearch.MyCharacterType == CharacterType.Fellow)
                {
                    if (other.gameObject.CompareTag(fellowHitTag[0]) 
                        || other.gameObject.CompareTag(fellowHitTag[1]))
                    {
                        targetSearch.TargetFind(other.gameObject);
                    }
                    else
                    {
                        targetSearch.TargetLost();
                    }
                }
            }

        }
        void OnTriggerExit(Collider other)
        {
            //起動していて、目標発見済み
            if (this.enabled && targetSearch.IsTargetFind)
            {
                //　目標を見失った
                if (targetSearch.MyCharacterType == CharacterType.Enemy)
                {                    
                    if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Fellow"))
                    {
                        targetSearch.TargetLost();
                    }
                }
                else if (targetSearch.MyCharacterType == CharacterType.Fellow)
                {
                    if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss"))
                    {
                        targetSearch.TargetLost();
                    }
                }
            }
        }
    }
}