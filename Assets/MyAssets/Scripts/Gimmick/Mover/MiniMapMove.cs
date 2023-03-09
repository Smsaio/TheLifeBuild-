using UnityEngine;
using System.Collections;
using Ability;
using PlayerSpace;
using UniRx;
using Zenject;

/// <summary>
/// ミニマップ用のカメラ(ターゲットを追いかける)
/// </summary>
public class MiniMapMove : ReactivePropertyController
{
    private Transform target;
    private void Start()
    {
        //ReactivePlayer(role);
    }
    private void Update()
    {
        
    }
    
    //プレイヤーに付いて移動する
    private void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
    }
    public override void ReactivePlayer(IRole role)
    {
        base.ReactivePlayer(role);
        if (role == null) return;
        role.CurrentPlayerTransform.Subscribe(targetTransform => { target = targetTransform; }).AddTo(this);
    }

}