using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    private VisualElement _ui;

    private Label _speaker;

    private Label _text;

    private VisualElement _background;

    private List<string> _texts = new List<string>()
    {
        "In the beginning… there was Soup.",
        "And the faithful veggies worshipped the Soup.",
        "They honored it. They feared it.",
        "They… seasoned it to taste.",
        "Tonight, we offer our ripest!",
        "Their sacrifice will summon...",
        "the great spirit of the Soup!",
        "But something went wrong…",
        "Instead of their sacred deity… they got you. A demon. Summoned by mistake. Trapped in a world of crunchy, leafy fools!",
        "This is NOT vegan!!",
        "Now, there’s only one thing left to do… Take revenge.",
        "Make ‘em stew in their mistakes."

    };

    private List<string> _speakers = new List<string>()
    {
        "Narrator",
        "Narrator",
        "Narrator",
        "Narrator",
        "Pepper Leader",
        "Pepper Leader",
        "Pepper Leader",
        "Narrator",
        "Narrator",
        "Pepper Leader",
        "Narrator",
        "???"
    };

    [SerializeField]
    private List<Texture2D> _backgrounds;


    private int _textID = 0;

    private void Awake()
    {
        _ui = GetComponent<UIDocument>().rootVisualElement;
    }



    private void OnEnable()
    {
        _speaker = _ui.Q<Label>("Speaker");
        _speaker.text = _speakers[_textID];

        _text = _ui.Q<Label>("Text");
        _text.text = _texts[_textID];

        _background = _ui.Q<VisualElement>("Background");
    }
    
    public void ProgressText(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            _textID++;
            
            if (_textID >= 12)
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                _background.style.backgroundImage = _backgrounds[_textID];
                _speaker.text = _speakers[_textID];
                _text.text = _texts[_textID];
            }

                
        }
        
    }

}
