using GameManagerSpace;
using PlayerSpace;
using UnityEngine;
using Zenject;

/// <summary>
/// Project全体のProjectInstaller
/// </summary>
public class MyProjectInstaller : MonoInstaller
{
    //設定するゲームマネージャーオブジェクト
    [SerializeField] private GameObject gameManagerObject;
    //設定するオーディオマネージャーオブジェクト
    [SerializeField] private GameObject audioManagerObject;
    //設定するロールオブジェクト
    [SerializeField] private GameObject roleObject;
    public override void InstallBindings()
    {
        if (gameManagerObject != null)
        {
            //GameManagerコンポーネント
            //GameManager.csというMonoBehaviourをオブジェクトから参照する
            Container.
                Bind<IGameManager>().
                To<GameManager>().
                FromComponentOn(gameManagerObject).
                AsSingle().
                NonLazy();
        }
        if (audioManagerObject != null)
        {
            //AudioSourceManagerコンポーネント
            //AudioSourceManager.csというMonoBehaviourをオブジェクトから参照する
            Container.
                Bind<IAudioSourceManager>().
                To<AudioSourceManager>().
                FromComponentOn(audioManagerObject).
                AsSingle().
                NonLazy();
        }
        if (roleObject != null)
        {
            //Roleコンポーネント
            //Role.csというMonoBehaviourをオブジェクトから参照する
            Container.
                Bind<IRole>().
                To<Role>().
                FromComponentOn(roleObject).
                AsSingle().
                NonLazy();
        }
    }
}
