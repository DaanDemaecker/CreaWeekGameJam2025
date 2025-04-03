using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TitleScreen : MonoBehaviour
{
    private VisualElement _ui;

    private Button _playButton;
    private Button _closeButton;

    private void Awake()
    {
        _ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        _playButton = _ui.Q<Button>("StartButton");
        _playButton.clicked += _playButton_clicked;

        _closeButton = _ui.Q<Button>("QuitButton");
        _closeButton.clicked += _closeButton_clicked;
    }

    private void _closeButton_clicked()
    {
        Debug.Log("Closing Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();

#endif
    }

    private void _playButton_clicked()
    {
        SceneManager.LoadScene(4);
    }
}
