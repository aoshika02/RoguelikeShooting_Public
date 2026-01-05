using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TitleManager : SingletonMonoBehaviour<TitleManager>
{
    private bool _loaded;
    async void Start()
    {
        _loaded = true;
        await FadeManager.FadeIn();
        GameStateManager.Instance.SetGameState(GameStateType.TextEvent);
        _loaded = false;
    }
    public async void Load(bool isTutorial)
    {
        if (_loaded) return;
        _loaded = true;
        SoundManager.Instance.PlaySE(SEType.TapBotton);
        await FadeManager.FadeOut();
        InputManager.Instance.gameObject.SetActive(false);
        Destroy(InputManager.Instance.gameObject);
        GameStateManager.Instance.SetGameState(GameStateType.None);
        await SceneLoadMananger.Instance.LoadSceneAsync(SceneType.Main);
        GameManager.Instance.Init(isTutorial);
    }
}
