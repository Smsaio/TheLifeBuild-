using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif 
#if UNITY_EDITOR
[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    private Item _target;
    // Start is called before the first frame update
    private void Awake()
    {
        _target = target as Item;
    }
    //インスペクター表示
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        //スクリプトへの移動
        using (new EditorGUI.DisabledGroupScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
        }
        EditorGUI.BeginChangeCheck();
        _target.itemType =(ItemType.ITEMTYPE)EditorGUILayout.EnumPopup("itemType", _target.itemType);
        //アイテムの種類
        switch (_target.itemType)
        {
            //攻撃力が上がる。
            case ItemType.ITEMTYPE.PowerUP:
                EditorGUILayout.LabelField("攻撃力アップ量");
                _target.powerUPPoint = EditorGUILayout.IntSlider("powerUPPoint", _target.powerUPPoint, 1, 30);

                break;
            //防御力が上がる。
            case ItemType.ITEMTYPE.DefenceUP:
                EditorGUILayout.LabelField("防御力アップ量");
                _target.defenceUPPoint = EditorGUILayout.IntSlider("defenceUPPoint", _target.defenceUPPoint,1,30);

                break;
            //体力が回復する。
            case ItemType.ITEMTYPE.Heal:
                EditorGUILayout.LabelField("回復量");
                _target.healPoint = EditorGUILayout.IntSlider("healPoint", _target.healPoint,10,510);
                break;
        }
        // GUIの更新があったら実行
        if (EditorGUI.EndChangeCheck())
        {
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.SetDirty(_target);
        }
    }
}
#endif