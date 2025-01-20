using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StoryManager : MonoBehaviour
{
    public TextReader _textReader;
    public StoryPlayerController _controller;
    private VisualElement _root;
    private Label _label;
    private Button _skipButton;
    public PigController pigcontroller;
    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _label=_root.Q<Label>("TextLabel");
        _skipButton = _root.Q<Button>("SkipButton");
        _skipButton.clickable.clicked += () => Skip();
    }
    public IEnumerator FirstStoryStart()
    {
        yield return new WaitForSeconds(2);
        for (int i=1;i<7; i++)
        {
            TextData textData = _textReader.GetTextData(i);
            if (textData.Talker == "µÅÁö")
            {
                _label.style.color = Color.red;
            }
            else
            {
                _label.style.color = Color.black;
            }
            _label.text = $"{textData.Talker}: {textData.Text}";
            if (i == 3)
            {
                pigcontroller.PigRun(true);
            }
            
            yield return new WaitForSeconds(_textReader.GetTextData(i).Term);
            if (i == 6)
            {
                foreach (var pig in FindObjectsOfType<PigController>())
                {
                    if (pig.gameObject.name == "Pig_Pink")
                    {
                        StartCoroutine(pig.TranslatePigs());
                    }
                }
                StartCoroutine(_controller.Run());
            }
        }
    }
    private void Skip()
    {
       // SceneManager.LoadScene("Battle");
    }
   
}
