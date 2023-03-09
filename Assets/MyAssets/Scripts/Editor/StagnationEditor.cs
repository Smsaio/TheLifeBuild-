using UnityEngine;
using UnityEngine.SceneManagement;
using Ability;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif 
#if UNITY_EDITOR
[CustomEditor(typeof(Stagnation))]
public class StagnationEditor : Editor
{
    private Stagnation _target;
    // Start is called before the first frame update
    private void Awake()
    {
        _target = target as Stagnation;
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
        //GUIの更新があったら実行
        if (EditorGUI.EndChangeCheck())
        {
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.SetDirty(_target);
        }
    }
}
#endif