using UnityEngine;
using UnityEngine.SceneManagement;
using Ability;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif 
#if UNITY_EDITOR
[CustomEditor(typeof(Refusal))]
public class RefusalEditor : Editor
{
    private Refusal _target;
    // Start is called before the first frame update
    private void Awake()
    {
        _target = target as Refusal;
    }
    //インスペクター表示
    public override void OnInspectorGUI()
    {
        //スクリプトへの移動
        using (new EditorGUI.DisabledGroupScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
        }
        EditorGUI.BeginChangeCheck();

        _target.RefusalArea = (GameObject)EditorGUILayout.ObjectField("RefusalArea", _target.RefusalArea, typeof(GameObject), true);
        _target.RefusalLimitSlowSpeed = EditorGUILayout.Slider("RefusalLimitSlowSpeed", _target.RefusalLimitSlowSpeed,0.1f,0.7f);

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