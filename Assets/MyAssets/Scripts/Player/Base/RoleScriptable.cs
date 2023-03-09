using Ability;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrentRole", menuName = "ScriptableObjects/Role")]
public class RoleScriptable : ScriptableObject
{
    public Attach.Role role;
}
