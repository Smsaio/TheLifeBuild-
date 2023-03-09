using UnityEngine;
using UnityEngine.SceneManagement;
using Ability;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif 
#if UNITY_EDITOR
[CustomEditor(typeof(Convert))]
public class ConvertEditor : Editor
{
    private Convert _target;
    // Start is called before the first frame update
    private void Awake()
    {
        _target = target as Convert;
    }
    //�C���X�y�N�^�[�\��
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        using (new EditorGUI.DisabledGroupScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
        }
        EditorGUI.BeginChangeCheck();
        _target.AnuvisWeapon = (GameObject)EditorGUILayout.ObjectField("AnuvisWeapon", _target.AnuvisWeapon,typeof(GameObject),true);
        _target.MaxFollowCharaCount = EditorGUILayout.IntSlider("MaxFollowCharaCount", _target.MaxFollowCharaCount,1,5);
        //GUI�̍X�V������������s
        if (EditorGUI.EndChangeCheck())
        {
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.SetDirty(_target);
        }
    }
}
#endif