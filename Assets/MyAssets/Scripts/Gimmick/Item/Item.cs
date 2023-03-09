using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace;

/// <summary>
/// 全アイテム
/// </summary>
public class Item : MonoBehaviour
{
    //攻撃力アップ値
    public int powerUPPoint = 1;
    //防御力アップ値
    public int defenceUPPoint = 1;
    //速度上昇値
    public int speedUPPoint = 1;
    //回復量
    public int healPoint = 10;

    //アイテムの種類設定
    public ItemType.ITEMTYPE itemType = ItemType.ITEMTYPE.Heal;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    //アイテムの機能全般
    private void ItemMethod(Player player)
    {
        int upPoint = 0;
        switch (itemType)
        {
            case ItemType.ITEMTYPE.PowerUP:
                upPoint = powerUPPoint;
                break;
            case ItemType.ITEMTYPE.DefenceUP:
                upPoint = defenceUPPoint;
                break;
            case ItemType.ITEMTYPE.Heal:
                upPoint = healPoint;
                break;
        }
        player.PointUP(upPoint,itemType);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.gameObject.CompareTag("Player"))
        {
            var player = other.transform.root.gameObject.GetComponent<Player>();
            if(player != null)
                ItemMethod(player);
            Destroy(gameObject);
        }
    }
}
