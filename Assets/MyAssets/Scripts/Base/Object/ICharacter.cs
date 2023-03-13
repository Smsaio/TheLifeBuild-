using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacter
{
    public int CurrentMaxHP { get; set;  }
    public int CurrentHP { get; set;  }
    public int AttackP { get; set; }
    public int DefenceP { get; set; }
}