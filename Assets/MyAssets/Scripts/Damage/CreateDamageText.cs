
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//文字列を一文字に変え、その文字をアニメーションする
public class CreateDamageText : MonoBehaviour
{
    [SerializeField]
    private GameObject damageTextCanvas;
    //　インスタンス化したダメージテキストリスト
    private List<GameObject> damageTextList = new List<GameObject>(10);
    //　現在アニメーションをしているテキストの番号
    private int charNum;
    //　全ての文字のアニメーションが終わってからゲームオブジェクトを消すまでの時間
    [SerializeField]
    private float deleteTime = 1f;
    /// <summary>
    /// ダメージテキストにアニメーションさせながら出現させる場合
    /// </summary>
    /// <param name="parentObj">出現させる場所</param>
    /// <param name="damagePoint">ダメージ</param>
    public void CreateText(GameObject parentObj, int damagePoint)
    {
        //　ダメージテキストの幅を取得
        var height = damageTextCanvas.GetComponent<RectTransform>().rect.height;
        GameObject damageTextIns;
        //　ダメージポイントの桁数分のダメージテキストをインスタンス化
        for (int i = 0; i < damagePoint.ToString().Length; i++)
        {
            damageTextIns = Instantiate(damageTextCanvas, parentObj.transform);
            //　ダメージテキストの位置はダメージテキストの幅分上(右)に移動させていく
            damageTextIns.GetComponentInChildren<RectTransform>().anchoredPosition = new Vector2(damageTextIns.transform.position.x,
                damageTextIns.transform.position.y + (i * height));
            //　ダメージポイントの桁毎の数値をダメージテキストに表示
            damageTextIns.GetComponentInChildren<TextMeshProUGUI>().text = damagePoint.ToString()[i].ToString();
            //　アニメーションを管理する為にインスタンスをリストに保持しておく
            damageTextList.Insert(i,damageTextIns);
            //アニメーション
            AnimateNextChar();
        }        
    }
    private void Start()
    {
        Destroy(gameObject, 10.0f);
    }
    /// <summary>
    /// 次の文字のアニメーションを開始させる
    /// </summary>
    private void AnimateNextChar()
    {
        //　全ての文字をアニメーションさせていなければ次の文字をアニメーションさせる
        if (charNum < damageTextList.Count - 1)
        {
            damageTextList[charNum].GetComponent<Animator>().SetTrigger("Damage");
            charNum++;
        }
        else
        {
            //　全ての文字のアニメーションが終わっていたら1秒後に削除
            Destroy(gameObject, deleteTime);
        }
    }
}
