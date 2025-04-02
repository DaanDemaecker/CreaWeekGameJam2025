using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement _player;

    [SerializeField]
    private PlayerBloodTracker _bloodTracker;
    
    private VisualElement _ui;

    private ProgressBar _blood;

    private void Awake()
    {
        _ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        _blood = _ui.Q<ProgressBar>("Bloodbar");
        _blood.value = _blood.highValue;
    }

    private void LateUpdate()
    {
        if (_blood != null && _bloodTracker != null)
        {
            _blood.value = _bloodTracker.BloodPercentage * _blood.highValue;
        }
    }
}
