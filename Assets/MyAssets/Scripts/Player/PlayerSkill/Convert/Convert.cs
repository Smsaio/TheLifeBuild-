using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Ability;
namespace Ability
{
    /// <summary>
    /// 記憶転換の際に用いる特技のクラス
    /// </summary>
    public class Convert : SpecialityBase
    {
        private Transform playerTransform;
        private void Start()
        {
            spawnPosition = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            spawnAnuvisObject = Instantiate(anuvisWeapon, spawnPosition, Quaternion.identity);
            spawnAnuvisObject.SetActive(false);
            var anuvis = spawnAnuvisObject.GetComponent<Anuvis>();
            if (anuvis != null)
            {
                anuvis.ReactivePlayer(role);
            }
        }
        private void Update()
        {

        }
        /// <summary>
        /// 味方になったオブジェクトに番号を振る
        /// </summary>
        /// <param name="followChara">味方クラス</param>
        public override void NumFellowCount(FollowChara followChara)
        {
            //味方の人数に余裕があるとき
            if (maxFollowCharaCount > fellowCount)
            {
                if (followChara != null)
                {
                    //代入してからfellowCountが増える
                    followChara.MyFellowCount = fellowCount++;
                }
            }
        }
        /// <summary>
        /// 味方追加
        /// </summary>
        public override void AddFellow(GameObject followObject)
        {
            if (maxFollowCharaCount > followCharaList.Count)
            {
                var followChara = followObject.GetComponent<FollowChara>();
                //味方に番号割り振り
                NumFellowCount(followChara);
                //味方に追加
                followCharaList.Add(followChara);
                followObject.transform.tag = "Fellow";
            }
        }

        /// <summary>
        /// 味方削除、味方が死んだ場合、配列振り直し
        /// </summary>
        public override void RemoveFellow(FollowChara followChara)
        {
            //死んだ味方の要素番号で指定した部分を消す
            bool valid = followCharaList.Contains(followChara);
            if (valid)
            {
                int index = followCharaList.IndexOf(followChara);
                followCharaList.RemoveAt(index);
            }
            //配列一時避難
            FollowChara[] followCharas = followCharaList.ToArray();
            //現在の要素番号
            int count = 0;
            for (int i = 0; i < followCharaList.Count; i++)
            {
                //配列の中が空ではないか
                if (followCharas[i] != null)
                {
                    Debug.Log("現在の味方番号" + count.ToString());
                    //要素番号ふり直し
                    //次の要素番号へ
                    followCharaList[count].MyFellowCount = count++;
                    Debug.Log("次の味方番号" + count.ToString());
                }
            }
            //味方の数反映
            fellowCount = followCharaList.Count;
        }
        public override void ReactivePlayer(IRole role)
        {
            if (role == null) return;
            role.CurrentPlayerTransform.Subscribe(currentTransform => { playerTransform = currentTransform; }).AddTo(this);

        }
        public override void UseSpeciality()
        {
            if (playerTransform != null)
            {
                spawnPosition = playerTransform.position + (Vector3.up * 0.15f);
                spawnAnuvisObject.transform.position = spawnPosition;
                spawnAnuvisObject.SetActive(true);
            }
        }
        public override void DoneSpeciality()
        {
            if (spawnAnuvisObject != null)
            {
                spawnAnuvisObject.SetActive(false);
            }
        }
    }
}