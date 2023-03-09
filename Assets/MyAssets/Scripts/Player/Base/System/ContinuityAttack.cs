using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using PlayerSpace;
using AttackProcess;
/// <summary>
///連続で攻撃する場合のステートマシン
/// </summary>
public class ContinuityAttack : StateMachineBehaviour
{
	private PlayerMove playerMove;
	private ProcessMyMove myMove;
	private int attackAnim = Animator.StringToHash("BoolAttack");
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		playerMove = animator.gameObject.GetComponent<PlayerMove>();
		//スタンドでなければ
		myMove = animator.gameObject.GetComponent<ProcessMyMove>();
		animator.SetBool(attackAnim, false);
		if (playerMove != null)
			playerMove.IsFire = false;
	}
	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//プレイヤーを取得出来た
		if (playerMove != null)
		{
			//プレイヤーが通常攻撃のボタンを押した場合
			if (playerMove.IsFire)
			{
				//攻撃する
				animator.SetBool(attackAnim, true);
				playerMove.IsFire = false;
			}
		}
	}
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//アニメーション初期化と通常攻撃のボタンを押したかどうかをfalseに
		animator.SetBool(attackAnim, false);
		if(playerMove != null)
			playerMove.IsFire = false;
		//何かがあって中断した場合に備えて攻撃の時の当たり判定を終わらす
		if (myMove != null)
		{
			myMove.AttackEnd();
			myMove.EffectOff();
		}
	}
	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
