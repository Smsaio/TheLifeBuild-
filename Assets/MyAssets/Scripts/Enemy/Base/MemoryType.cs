using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryType
{
    /// <summary>
    ///負の記憶の種類
    /// </summary>
    public enum NegativeMemory
    {
        //攻撃力上昇(壮絶な記憶)
        Hatred,    //憎しみ
        Angry,     //怒り

        //防御力上昇(悲しみの記憶)
        Tragic,    //悲壮感
        Sorrowful, //悲哀

        //体力増強(つらい記憶)
        Despair,   //絶望     
        Uneasiness,//不安感

        //経験値増加(特殊)           
        Trauma,    //トラウマ
        Darkhistory//黒歴史
    }
    /// <summary>
    /// 記憶の分類
    /// </summary>
    public enum MemoryClassification
    {
        Spectacular = 0,//壮絶な記憶
        Sad,        //悲しみの記憶
        Painful,    //つらい記憶
        Special,    //特別な記憶
        None
    }
    /// <summary>
    ///味方に変わった時の記憶(いい記憶)
    /// </summary>
    public enum GoodMemory
    {        
        Affection = 0, //憎しみから「情愛」
        Laugh,         //怒りから「笑う」        
        Satisfaction,  //悲壮感から「満足」       
        Delight,       //悲哀から「歓喜」        
        Hope,          //絶望から「希望」        
        Peaceofmind    //不安感から「安心」
    }
}
