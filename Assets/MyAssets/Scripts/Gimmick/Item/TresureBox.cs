using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace;
using TMPro;
using System;

/// <summary>
/// 宝箱 アイテムが出てくる
/// </summary>
public class TresureBox : MonoBehaviour
{
	//入手ボタン表示
	[SerializeField] private TextMeshPro getButtonText;
	//宝箱から出てくるアイテム
	[SerializeField] private GameObject[] itemList;
	//開いたかどうか
	private bool isOpen = false;
	//アイテムのオブジェクトと物理
	private GameObject item;
	private Rigidbody itemrb;
	//アニメーターとプレイヤー
	private Animator anim;
	private PlayerMove player;
	// Use this for initialization
	void Start()
	{
		anim = GetComponent<Animator>();
		TresureItem();
	}
	void Update()
    {
        if (player != null && player.IsAttack && !isOpen)
        {
			OpenBox();
			FlyItem();
			isOpen = true;
		}
	}
	/// <summary>
	/// 宝の中身を事前に接置
	/// </summary>
	void TresureItem()
    {
		var random = UnityEngine.Random.Range(0, itemList.Length - 1);
		item = Instantiate(itemList[random],transform.position + (Vector3.up * 3),Quaternion.identity);
		item.SetActive(false);
		var lifeBuild = item.GetComponent<LifeBuild>();
		itemrb = item.GetComponent<Rigidbody>();
		int randomMemory;
		if (lifeBuild != null)
        {
			lifeBuild.EXP = 200;
			randomMemory = UnityEngine.Random.Range(0, Enum.GetValues(typeof(MemoryType.NegativeMemory)).Length - 1);
			lifeBuild.MyMemory = (MemoryType.NegativeMemory)randomMemory;
        }
    }
	/// <summary>
	/// 宝箱を調べたらアイテムを飛ばす
	/// </summary>
	void FlyItem()
    {
		item.SetActive(true);
		itemrb.GetComponent<Rigidbody>().AddForce(Vector3.up * 15, ForceMode.Impulse);
		Destroy(gameObject);
	}
	/// <summary>
	/// 宝箱を開ける
	/// </summary>
	private void OpenBox()
	{
		isOpen = true;
		anim.SetBool("Open", true);
	}
	/// <summary>
	/// 宝箱を閉じる
	/// </summary>
	private void CloseBox()
	{
		isOpen = false;
		anim.SetBool("Open", false);
	}
    private void OnTriggerEnter(Collider other)
    {
		//プレイヤーが範囲内に入れば
		if (other.gameObject.CompareTag("Player"))
        {
			if (player == null)
				player = other.gameObject.GetComponent<PlayerMove>();
			//入手テキスト表示
			getButtonText.gameObject.SetActive(true);
        }
    }
	private void OnTriggerExit(Collider other)
	{
		//プレイヤーが範囲内から出れば
		if (other.gameObject.CompareTag("Player"))
		{
			//入手テキスト非表示
			getButtonText.gameObject.SetActive(false);
		}
	}
}