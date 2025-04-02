using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement _player;
    
    private VisualElement _ui;

    private ProgressBar _blood;

    private void Awake()
    {
        _ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        _blood = _ui.Q<ProgressBar>("Bloodbar");
        //_blood.value = _player.bloodCount;
    }
}
