using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MuteButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (label == null)
        {
            label = GetComponentInChildren<TextMeshProUGUI>();
        }

        button.onClick.AddListener(HandleButtonClick);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleButtonClick);
        }
    }

    private void OnEnable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnMuteChanged += UpdateState;
            UpdateState(AudioManager.Instance.IsMuted);
        }
        else
        {
            UpdateLabel(false);
        }
    }

    private void OnDisable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnMuteChanged -= UpdateState;
        }
    }

    private void HandleButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
        }
    }

    private void UpdateState(bool muted)
    {
        UpdateLabel(muted);
    }

    private void UpdateLabel(bool muted)
    {
        if (label != null)
        {
            label.text = muted ? "SOUND: OFF" : "SOUND: ON";
        }
    }
}
