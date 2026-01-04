// 音频项UI组件

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioItemUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI voiceTypeText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button deleteButton;

    private TTSAudioItem audioItem;
    private Action<TTSAudioItem> onSelectCallback;
    private Action<TTSAudioItem> onDeleteCallback;

    public void Initialize(TTSAudioItem item, Action<TTSAudioItem> selectCallback, Action<TTSAudioItem> deleteCallback)
    {
        audioItem = item;
        onSelectCallback = selectCallback;
        onDeleteCallback = deleteCallback;

        if (voiceTypeText != null)
            voiceTypeText.text = $"{item.VoiceType}";
        
        if (contentText != null)
            contentText.text = $"{item.ContentPreview}";

        if (dateText != null)
            dateText.text = item.CreationTime.ToString("yyyy-MM-dd HH:mm");

        // 绑定事件
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClick);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
    }

    private void OnPlayButtonClick()
    {
        onSelectCallback?.Invoke(audioItem);
    }

    private void OnDeleteButtonClick()
    {
        onDeleteCallback?.Invoke(audioItem);
        Destroy(gameObject);
    }
}