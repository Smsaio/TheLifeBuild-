using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ReactiveProperty�p�֐���`�N���X
/// </summary>
public class ReactivePropertyController: MonoBehaviour
{
    /// <summary>
    /// �v���C���[�֘A�N���X�Ď��p�֐�
    /// </summary>
    /// <param name="role"></param>
    public virtual void ReactivePlayer(IRole role)
    {
        if (role == null) return;
    }
}
