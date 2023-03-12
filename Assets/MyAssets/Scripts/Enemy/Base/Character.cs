using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemySpace;
using GameManagerSpace;
namespace EnemySpace
{
    public enum CharacterState
    {
        Move,//�ړ�
        Wait,//�҂�
        Chase,//�ǐ�
        Charge,//���ߎ���
        Stan,//�X�^�����
        DeathBlow,//���`
        Damage,//�_���[�W
        Attack,//�ʏ�U��
        Freeze,//�d�����(�U����)
        Death,//���S
        Guard,//�h�䂷��(�_���[�W�������Ȃ��Ȃ�)
        Run,//����
    };
}
/// <summary>
/// �G�Ɩ����ƃv���C���[�̊�{�ϐ���֐�
/// </summary>
public class Character : MonoBehaviour,IReactiveProperty
{
    protected int currentHP = 0;
    public int CurrentHP { get { return currentHP; } set { currentHP = value; } }
    protected int currentMaxHP = 0;
    public int CurrentMaxHP { get { return currentMaxHP; } set { currentMaxHP = value; } }
    protected int attackP = 0;
    public int AttackP { get { return attackP; } set { attackP = value; } }
    protected int defenceP = 0;
    public int DefenceP { get { return defenceP; } set { defenceP = value; } }
    protected IRole role = default;
    public IRole Role { set { role = value; } }
    protected IGameManager gameManager = default;
    public IGameManager GameManager { set { gameManager = value; } get { return gameManager; } }
//-----------------�G�□���p�̊֐�------------
    /// <summary>
    /// ��Ԃ�ݒ�
    /// </summary>
    /// <param name="state">�w�肷����</param>
    /// <param name="targetTransform">�ڕW�����m�����ꍇ�̖ڕW�̈ʒu</param>
    public virtual void SetState(CharacterState state, Transform targetTransform = null)
    {

    }
    /// <summary>
    /// update�̒��ł̓���
    /// </summary>
    protected virtual void StateMove()
    {

    }
    public virtual void ReactivePlayer(IRole Irole)
    {

    }
}
