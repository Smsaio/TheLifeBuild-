using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
namespace Ability
{
    /// <summary>
    /// 記憶拒絶の際に用いる特技のクラス
    /// </summary>
    public class Refusal : SpecialityBase
    {
        private Transform playerTransform;
        private GameObject SpawnRefusalArea;
        // Start is called before the first frame update
        void Start()
        {
            SpawnRefusalArea = Instantiate(refusalArea);
            SpawnRefusalArea.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        public override void UseSpeciality()
        {
            SpawnRefusalArea.SetActive(true);
            SpawnRefusalArea.transform.position = playerTransform.position;
        }
        public override void ReactivePlayer(IRole role)
        {
            if (role == null) return;
            role.CurrentPlayerTransform.Subscribe(currentTransform => { playerTransform = currentTransform; }).AddTo(this);
        }
        public override void DoneSpeciality()
        {
            if (SpawnRefusalArea != null)
            {
                //拒絶範囲非表示
                SpawnRefusalArea.SetActive(false);
            }
        }
    }
}