using UnityEngine;
using System.Collections;
using EnemySpace;
using PlayerSpace;
namespace EnemySpace
{
    /// <summary>
    /// ターゲットを探す際のクラス(見回り用)
    /// </summary>
    public class SearchCharacter : MonoBehaviour
    {
        [SerializeField] private FindMark findMark;
        //索敵用のコライダー
        private BoxCollider searchCollider;
        private ITargetSearch targetSearch;
        public ITargetSearch TargetSearch { get { return targetSearch; } set { targetSearch = value; } }
        void Start()
        {
            targetSearch = transform.root.gameObject.GetComponent<ITargetSearch>();
            searchCollider = GetComponent<BoxCollider>();
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
            if (targetSearch.MyCharacterType == CharacterType.Enemy)
            {
                if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Fellow"))
                {
                    targetSearch.TargetFind(other.gameObject);
                }
            }
            else if(targetSearch.MyCharacterType == CharacterType.Fellow)
            {
                if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss"))
                {
                    findMark.SetSize();
                    targetSearch.TargetFind(other.gameObject);
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
                    if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Fellow"))
                    {
                        Debug.Log("索敵中");
                        targetSearch.TargetFind(other.gameObject);
                    }
                }
                else if (targetSearch.MyCharacterType == CharacterType.Fellow)
                {
                    if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss"))
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