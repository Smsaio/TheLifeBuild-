#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// メニューバーからゲームシーンやタイトルシーンに行くためのボタンを追加するクラス
/// </summary>
public static class SceneLauncher
{
	/// <summary>
	/// ゲームシーン
	/// </summary>
	[MenuItem("Launcher/GameScene", priority = 0)]
	public static void OpenGameScene()
	{
		EditorSceneManager.OpenScene("Assets/MyAssets/Scenes/MainStage1.unity", OpenSceneMode.Single);
	}
	/// <summary>
	/// タイトルシーン(スタートする最初のシーン)
	/// </summary>
	[MenuItem("Launcher/TitleScene", priority = 0)]
	public static void OpenStartScene()
	{
		EditorSceneManager.OpenScene("Assets/MyAssets/Scenes/StartScene.unity", OpenSceneMode.Single);
	}
	/// <summary>
	/// タイトルシーン(スタートする最初のシーン)
	/// </summary>
	[MenuItem("Launcher/GameExampleScene", priority = 0)]
	public static void OpenGameExampleScene()
	{
		EditorSceneManager.OpenScene("Assets/MyAssets/Scenes/MainStageExample.unity", OpenSceneMode.Single);
	}
	/// <summary>
	/// タイトルシーン(スタートする最初のシーン)
	/// </summary>
	[MenuItem("Launcher/TitleExampleScene", priority = 0)]
	public static void OpenStartExampleScene()
	{
		EditorSceneManager.OpenScene("Assets/MyAssets/Scenes/StartSceneExample.unity", OpenSceneMode.Single);
	}
}
#endif