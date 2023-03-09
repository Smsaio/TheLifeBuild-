using UnityEngine;

/// <summary>
/// インスペクターの配列のelementを設定した名前に変更する
/// </summary>
public class NamedArrayAttribute : PropertyAttribute
{
    //配列の要素の名前を入れる配列
    public readonly string[] names;
    public NamedArrayAttribute(string[] names) { this.names = names; }
}