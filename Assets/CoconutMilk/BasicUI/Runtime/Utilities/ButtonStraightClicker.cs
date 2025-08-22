using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonStraightClicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private Button _button;

    private bool _isFirstClick;
    private bool _isPressing;
    private float _pressTimer;

    private const float CLICK_REPEAT_TIME = 0.075f;
    private const float FIRST_CLICK_TIME = 0.15f;
    private const float START_CLICK_REPEAT_TIME = 0.35f;

    private PointerEventData _eventData;
    private ElasticButton _elasticButton;

    void Awake()
    {
        _button = GetComponent<Button>();
        _elasticButton = GetComponent<ElasticButton>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        _eventData = eventData;
        _isPressing = true;
        _isFirstClick = true;
        _pressTimer = START_CLICK_REPEAT_TIME;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressing = false;
        _isFirstClick = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPressing = false;
        _isFirstClick = false;
    }

    private void OnDisable()
    {
        _isPressing = false;
        _isFirstClick = false;
    }

    void Update()
    {
        if (_isPressing)
        {
            if (!_button.interactable)
            {
                _isPressing = false;
                _isFirstClick = false;
                return;
            }

            _pressTimer -= Time.deltaTime;
            if (_isFirstClick)
            {
                if (_pressTimer <= START_CLICK_REPEAT_TIME - FIRST_CLICK_TIME)
                {
                    _isFirstClick = false;
                    _button.onClick.Invoke();
                }
            }

            if (_pressTimer <= 0)
            {
                _pressTimer += CLICK_REPEAT_TIME;
                _button.onClick.Invoke();
                _elasticButton?.PlayPointerUp();
            }
        }
    }

    public void ForceQuit()
    {
        _isPressing = false;
        _isFirstClick = false;
    }
}