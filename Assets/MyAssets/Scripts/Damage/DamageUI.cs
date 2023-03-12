using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ダメージテキスト
/// </summary>
public class DamageUI : MonoBehaviour
{
	[SerializeField] private TextMeshPro damageText;
	//　移動値
	[Header("移動速度"),Range(0.1f,3.0f),SerializeField] private float moveSpeed = 0.4f;
	//消えるまでの時間
	[Header("消えるまでの時間"),Range(0.3f,1.0f),SerializeField] private float banishTime = 0.5f;

	//受けたダメージ
	private int damage = 10;
	public int Damage { set { damage = value; } }
	private RectTransform rect;
	private float ranX = 0;
	void Start()
	{
		rect= GetComponent<RectTransform>();
		ranX = UnityEngine.Random.Range(-2.0f, 2.0f);
	}

	void LateUpdate()
	{
		DisplayDamageText();
	}
	/// <summary>
	/// ダメージテキストを表示
	/// </summary>
	private void DisplayDamageText()
    {
		//ダメージを受けた場合
		if (damage > 0)
		{
			string damageTextUI = damage.ToString();
			//テキストに食らったダメージを反映
			damageText.SetText(damageTextUI);
			var pos = transform.position;
			//メッシュ
			damageText.color = damageText.color - new Color32(0, 0, 0, 1);
			transform.position = new Vector3(pos.x + (ranX * Time.deltaTime), pos.y + (moveSpeed * Time.deltaTime), pos.z);
			rect.transform.LookAt(Camera.main.transform);
			//消えるまでの時間をカウントダウン
			banishTime -= Time.deltaTime;
			if (banishTime <= 0 || damageText.color.a <= 0)
			{
				//消す
				Destroy(gameObject);
			}
		}
	}
}
