using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemType : MonoBehaviour
{
    //アイテムの種類
    public enum ITEMTYPE
    {
        PowerUP = 0,   //攻撃力アップ
        DefenceUP,     //防御力アップ
        Reverce,       //逆になる
        Heal,          //回復       
    }
}
