using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EnemySpace 
{
    public enum CharacterType
    {
        Enemy,  //�v���C���[�̓G��
        Fellow  //�v���C���[�̖�����
    }
    public interface ITargetSearch
    {
        public bool IsTargetFind { get; set; }
        public void TargetFind(GameObject obj);
        public void TargetLost();
        public CharacterType MyCharacterType { get; }
    }
}