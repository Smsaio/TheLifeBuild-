using AttackProcess;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProcessAnim
{
    public void AttackStart();
    public void AttackEnd();
    public void AttackSE(AudioClip attackSound = null);
}
