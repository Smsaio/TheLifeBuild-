using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using PlayerSpace;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_EDITOR
[CustomEditor(typeof(WeaponDamageStock))]
public class WeaponDamageStockEditor : Editor
{
    private WeaponDamageStock _target;

    private void Awake()
    {
        _target = target as WeaponDamageStock;
    }
    //インスペクターの表示を変更する
    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledGroupScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("遠距離の場合");
        _target.isBullet = EditorGUILayout.ToggleLeft("isBullet", _target.isBullet);
        EditorGUILayout.LabelField("味方になる可能性がある");
        _target.IsFellow = EditorGUILayout.ToggleLeft("isFellow", _target.IsFellow);
        //遠距離攻撃の場合
        if (_target.isBullet)
        {
            EditorGUILayout.LabelField("当たった時のパーティクル");
            _target.bulletVersion.hitParticle = (GameObject)EditorGUILayout.ObjectField("hitParticle", _target.bulletVersion.hitParticle, typeof(GameObject), true);
            EditorGUILayout.LabelField("進む速度");
            _target.bulletVersion.bulletSpeed = EditorGUILayout.FloatField("BulletSpeed", _target.bulletVersion.bulletSpeed);
            EditorGUILayout.LabelField("破壊されるまでの時間");
            _target.bulletVersion.bulletDestroyTime = EditorGUILayout.FloatField("BulletDestroyTime", _target.bulletVersion.bulletDestroyTime);

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