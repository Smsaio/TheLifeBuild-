using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EnemySpace 
{
    public enum CharacterType
    {
        Enemy,  //プレイヤーの敵か
        Fellow  //プレイヤーの味方か
    }
    public interface ITargetSearch
    {
        public bool IsTargetFind { get; set; }
        public void TargetFind(GameObject obj);
        public void TargetLost();
        public CharacterType MyCharacterType { get; }
    }
}