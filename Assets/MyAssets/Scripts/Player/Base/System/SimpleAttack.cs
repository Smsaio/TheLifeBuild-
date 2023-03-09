using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using PlayerSpace;
using AttackProcess;

public class SimpleAttack : StateMachineBehaviour
{
	private PlayerMove playerMove;
	private ProcessMyMove myMove;
	private ProcessStandMove standMove;
	private int attackAnim = Animator.StringToHash("BoolAttack");
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//������
		playerMove = animator.gameObject.GetComponent<PlayerMove>();
		myMove = animator.gameObject.GetComponent<ProcessMyMove>();
		standMove = animator.gameObject.GetComponent<ProcessStandMove>();
		if(playerMove != null)
			playerMove.IsFire = false;
		animator.SetBool(attackAnim, false);
	}
	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	/*override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		
	}*/
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//������
		playerMove.IsFire = false;
		animator.SetBool(attackAnim, false);
		//�����������Ē��f�����ꍇ�ɔ����čU���̎��̓����蔻����I��炷
		if(myMove != null)
			myMove.AttackEnd();
		if (standMove != null)
			standMove.AttackEnd();
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
