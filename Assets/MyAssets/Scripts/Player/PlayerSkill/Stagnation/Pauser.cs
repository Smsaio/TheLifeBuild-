using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 完全に停止させる(ポーズ)
/// </summary>
public class Pauser : MonoBehaviour
{
	// ポーズ対象のスクリプト
	static List<Pauser> targets = new List<Pauser>();  

	// ポーズ対象のコンポーネント
	Behaviour[] pauseBehavs = null;

	Rigidbody[] rgBodies = null;
	Vector3[] rgBodyVels = null;
	Vector3[] rgBodyAVels = null;

	Rigidbody2D[] rg2dBodies = null;
	Vector2[] rg2dBodyVels = null;
	float[] rg2dBodyAVels = null;

	public static bool isPause = false;
	// 初期化
	void Start()
	{
		// ポーズ対象に追加する
		targets.Add(this);
	}
	/// <summary>
	///  破棄されるとき
	/// </summary>
    private void OnDestroy()
    {
		// ポーズ対象から除外する
		targets.Remove(this);
	}

    /// <summary>
    /// ポーズされたとき
    /// </summary>
    private void OnPause()
	{
		if (pauseBehavs != null)
		{
			return;
		}

		// 有効なコンポーネントを取得
		pauseBehavs = Array.FindAll(GetComponentsInChildren<Behaviour>(), (obj) => { return obj.enabled; });
		foreach (var com in pauseBehavs)
		{
			com.enabled = false;
		}
		//rigidbodyも含めた場合、すべて停止
		rgBodies = Array.FindAll(GetComponentsInChildren<Rigidbody>(), (obj) => { return !obj.IsSleeping(); });
		rgBodyVels = new Vector3[rgBodies.Length];
		rgBodyAVels = new Vector3[rgBodies.Length];
		for (var i = 0; i < rgBodies.Length; ++i)
		{
			rgBodyVels[i] = rgBodies[i].velocity;
			rgBodyAVels[i] = rgBodies[i].angularVelocity;
			rgBodies[i].Sleep();
		}
		//rigidbody2dも含めた場合、すべて停止
		rg2dBodies = Array.FindAll(GetComponentsInChildren<Rigidbody2D>(), (obj) => { return !obj.IsSleeping(); });
		rg2dBodyVels = new Vector2[rg2dBodies.Length];
		rg2dBodyAVels = new float[rg2dBodies.Length];
		for (var i = 0; i < rg2dBodies.Length; ++i)
		{
			rg2dBodyVels[i] = rg2dBodies[i].velocity;
			rg2dBodyAVels[i] = rg2dBodies[i].angularVelocity;
			rg2dBodies[i].Sleep();
		}
	}

	/// <summary>
	/// ポーズ解除されたとき
	/// </summary>
	private void OnResume()
	{
		if (pauseBehavs == null)
		{
			return;
		}

		// ポーズ前の状態にコンポーネントの有効状態を復元
		foreach (var com in pauseBehavs)
		{
			com.enabled = true;
		}
		//rigidbody再起動
		for (var i = 0; i < rgBodies.Length; ++i)
		{
			rgBodies[i].WakeUp();
			rgBodies[i].velocity = rgBodyVels[i];
			rgBodies[i].angularVelocity = rgBodyAVels[i];
		}
		//rigidbody2d再起動
		for (var i = 0; i < rg2dBodies.Length; ++i)
		{
			rg2dBodies[i].WakeUp();
			rg2dBodies[i].velocity = rg2dBodyVels[i];
			rg2dBodies[i].angularVelocity = rg2dBodyAVels[i];
		}

		pauseBehavs = null;

		rgBodies = null;
		rgBodyVels = null;
		rgBodyAVels = null;

		rg2dBodies = null;
		rg2dBodyVels = null;
		rg2dBodyAVels = null;
	}

	/// <summary>
	///  ポーズ開始
	/// </summary>
	public static void Pause()
	{
		foreach (var obj in targets)
		{
			if (obj != null)
			{
				obj.OnPause();
			}
		}
		isPause = true;
	}
	/// <summary>
	/// ポーズ解除
	/// </summary>
	public static void Resume()
	{
		foreach (var obj in targets)
		{
			if (obj != null)
			{
				obj.OnResume();
			}
		}
		isPause = false;
	}
}