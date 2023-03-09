using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 操作説明表示の管理クラス
/// </summary>
public class TutorialManager : MonoBehaviour
{
    //前の説明に行く
    [SerializeField] private Button beforeButton;
    //次の説明に行く
    [SerializeField] private Button nextButton;
    //操作説明
    [SerializeField] private GameObject[] Tutorials;
    private int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        beforeButton.onClick.AddListener(PageChange);
        nextButton.onClick.AddListener(PageChange);
        
        for(int i=0; i < Tutorials.Length; i++)
        {
            Tutorials[i].gameObject.SetActive(false);
        }
        Tutorials[count].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        ButtonActive(beforeButton.gameObject, 0);
        ButtonActive(nextButton.gameObject, Tutorials.Length - 1);
    }
    private void ButtonActive(GameObject button,int limit)
    {
        bool active = count != limit;
        button.SetActive(active);
    }
    private void PageChange()
    {
        TutorialActive(false);
        count = count < Tutorials.Length - 1 ? count + 1 : count - 1;
        TutorialActive(true);
    }
    private void TutorialActive(bool isActive)
    {
        Tutorials[count].SetActive(isActive);
    }
}
