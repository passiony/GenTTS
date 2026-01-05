using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// 音频项数据结构
[Serializable]
public class TTSAudioItem
{
    public string FileName;
    public string FilePath;
    public string VoiceType;
    public string ContentPreview;
    public DateTime CreationTime;
}

public class TTSUIController : MonoBehaviour
{
    // 配置
    private const string AudioOutputPath = "TTS_Audio/";
    private const string DefaultAudioExtension = ".mp3";

    // UI组件
    [Header("UI组件")] [SerializeField] private TMP_InputField textInputField;
    [SerializeField] private TMP_Dropdown voiceDropdown;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Button generateButton;
    [SerializeField] private Transform audioListContainer;
    [SerializeField] private GameObject audioItemPrefab;
    [SerializeField] private TextMeshProUGUI statusText;

    // 数据
    private List<TTSAudioItem> audioList = new List<TTSAudioItem>();
    private int selectedVoice = (int)VoiceType.Xiaoxiao;
    private Language selectedLanguage = Language.Chinese;
    private string saveDataPath;

    private void Start()
    {
        // 初始化持久化路径
        saveDataPath = Path.Combine(Directory.GetCurrentDirectory(), AudioOutputPath);

        // 创建输出目录
        if (!Directory.Exists(saveDataPath))
        {
            Directory.CreateDirectory(saveDataPath);
        }

        // 初始化UI事件
        InitializeUI();

        // 加载已有音频
        LoadAudioList();
    }

    // 从文件名解析信息
    private TTSAudioItem ParseAudioFileName(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

        // 使用正则表达式解析文件名
        // 支持多种格式：
        // 1. [VoiceType]_[Text]_[Timestamp] (推荐格式)
        // 2. [VoiceType]_[Timestamp]
        // 3. [Text]_[Timestamp]
        // 4. [Timestamp]

        // 格式1: [VoiceType]_[Text]_[Timestamp]
        Regex regexFormat1 = new Regex(@"^(.+?)_([^_]+?)_(\d{13,17})$");
        Match match1 = regexFormat1.Match(nameWithoutExt);

        // 格式2: [VoiceType]_[Timestamp]
        Regex regexFormat2 = new Regex(@"^(.+?)_(\d{13,17})$");
        Match match2 = regexFormat2.Match(nameWithoutExt);

        // 格式3: [Text]_[Timestamp]
        Regex regexFormat3 = new Regex(@"^([^_]+?)_(\d{13,17})$");
        Match match3 = regexFormat3.Match(nameWithoutExt);

        // 格式4: [Timestamp]
        Regex regexFormat4 = new Regex(@"^(\d{13,17})$");
        Match match4 = regexFormat4.Match(nameWithoutExt);

        TTSAudioItem item = new TTSAudioItem
        {
            FilePath = filePath,
            FileName = fileName,
            VoiceType = "未知",
            ContentPreview = "无预览",
            CreationTime = File.GetCreationTime(filePath)
        };

        if (match1.Success)
        {
            item.VoiceType = match1.Groups[1].Value;
            item.ContentPreview = match1.Groups[2].Value;

            // 解析时间戳
            if (long.TryParse(match1.Groups[3].Value, out long timestamp1))
            {
                item.CreationTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(timestamp1)
                    .ToLocalTime();
            }
        }
        else if (match2.Success)
        {
            item.VoiceType = match2.Groups[1].Value;
            item.ContentPreview = "无预览";

            if (long.TryParse(match2.Groups[2].Value, out long timestamp2))
            {
                item.CreationTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(timestamp2)
                    .ToLocalTime();
            }
        }
        else if (match3.Success)
        {
            item.VoiceType = "未知";
            item.ContentPreview = match3.Groups[1].Value;

            if (long.TryParse(match3.Groups[2].Value, out long timestamp3))
            {
                item.CreationTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(timestamp3)
                    .ToLocalTime();
            }
        }
        else if (match4.Success)
        {
            if (long.TryParse(match4.Groups[1].Value, out long timestamp4))
            {
                item.CreationTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(timestamp4)
                    .ToLocalTime();
            }
        }

        return item;
    }

    private void InitializeUI()
    {
        // 生成按钮事件
        RefreshLangDropdown();
        RefreshVoiceDropdown();

        generateButton.onClick.AddListener(OnGenerateButtonClick);
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        voiceDropdown.onValueChanged.AddListener(OnVoiceChanged);

        // 初始状态
        UpdateStatusText("就绪");
    }

    private void OnLanguageChanged(int arg0)
    {
        var language = (Language)arg0;
        if (selectedLanguage != language)
        {
            selectedLanguage = language;
            RefreshVoiceDropdown();
        }
    }

    void RefreshLangDropdown()
    {
        languageDropdown.ClearOptions();
        List<string> languageOptions = new List<string>();
        foreach (Language language in Enum.GetValues(typeof(Language)))
        {
            languageOptions.Add(language.ToString());
        }

        languageDropdown.AddOptions(languageOptions);
    }

    void RefreshVoiceDropdown()
    {
        voiceDropdown.ClearOptions();
        List<string> voiceOptions = new List<string>();

        switch (selectedLanguage)
        {
            case Language.Chinese:
                foreach (VoiceType voice in Enum.GetValues(typeof(VoiceType)))
                {
                    voiceOptions.Add(voice.ToString());
                }

                voiceDropdown.AddOptions(voiceOptions);
                break;
            case Language.English:
                foreach (VoiceTypeEn voice in Enum.GetValues(typeof(VoiceTypeEn)))
                {
                    voiceOptions.Add(voice.ToString());
                }

                voiceDropdown.AddOptions(voiceOptions);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnVoiceChanged(int index)
    {
        selectedVoice = index;
    }

    string GetVoiceName()
    {
        switch (selectedLanguage)
        {
            case Language.Chinese:
                return ((VoiceType)selectedVoice).ToString();
            case Language.English:
                return ((VoiceTypeEn)selectedVoice).ToString();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnGenerateButtonClick()
    {
        string text = textInputField.text.Trim();

        if (string.IsNullOrEmpty(text))
        {
            UpdateStatusText("错误：请输入要转换的文本", Color.red);
            return;
        }

        UpdateStatusText("正在生成音频...", Color.yellow);
        generateButton.interactable = false;

        // 生成文件名
        string voiceTypeStr = GetVoiceName();
        string fileName = TTSFileNameGenerator.GenerateFileName(GetVoiceName(), text);
        string filePath = Path.Combine(saveDataPath, $"{fileName}{DefaultAudioExtension}");

        // 调用TTS生成
        TTSGenerator.GenerateTTS(filePath, text, selectedLanguage, selectedVoice, (success, result) =>
        {
            if (success)
            {
                UpdateStatusText("音频生成成功！" + filePath, Color.green);

                // 添加到列表
                TTSAudioItem newItem = new TTSAudioItem
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    VoiceType = voiceTypeStr,
                    ContentPreview = text.Length > 15 ? text.Substring(0, 15) + "..." : text,
                    CreationTime = DateTime.Now
                };

                audioList.Add(newItem);
                AddAudioItemToUI(newItem);
            }
            else
            {
                UpdateStatusText($"生成失败：{result}", Color.red);
            }

            generateButton.interactable = true;
        });
    }

    private void LoadAudioList()
    {
        audioList.Clear();

        if (Directory.Exists(saveDataPath))
        {
            string[] audioFiles = Directory.GetFiles(saveDataPath, $"*{DefaultAudioExtension}");

            foreach (string filePath in audioFiles)
            {
                try
                {
                    // 从文件名解析音频信息
                    TTSAudioItem item = ParseAudioFileName(filePath);
                    audioList.Add(item);
                    AddAudioItemToUI(item);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"加载音频文件信息失败: {ex.Message}");
                }
            }

            // 按创建时间排序（最新的在最前面）
            audioList.Sort((a, b) => b.CreationTime.CompareTo(a.CreationTime));
        }
    }

    private void AddAudioItemToUI(TTSAudioItem item)
    {
        if (audioItemPrefab == null)
        {
            Debug.LogError("音频项预制体未设置");
            return;
        }

        GameObject itemObj = Instantiate(audioItemPrefab, audioListContainer);
        AudioItemUI itemUI = itemObj.GetComponent<AudioItemUI>();

        if (itemUI != null)
        {
            itemUI.Initialize(item, OnAudioItemSelect, OnAudioItemDelete);
            itemObj.SetActive(true);
        }
    }

    private void OnAudioItemSelect(TTSAudioItem item)
    {
        // 首先打印文件路径，方便复制
        Debug.Log("播放音频文件路径: " + item.FilePath);
        UpdateStatusText($"播放音频: {item.FileName}", Color.blue);

        // 然后加载并播放音频
        StartCoroutine(PlayAudioClip(item.FilePath));
    }

    private IEnumerator PlayAudioClip(string filePath)
    {
        // 确保路径包含file://前缀（Windows需要转义反斜杠）
        string fullPath = filePath.Replace(@"\", "/");
        if (!fullPath.StartsWith("file://"))
        {
            fullPath = "file://" + fullPath;
        }

        Debug.Log("完整音频URL: " + fullPath);

        // 自动检测音频格式
        string extension = System.IO.Path.GetExtension(filePath).ToLower();
        AudioType audioType = AudioType.UNKNOWN;

        switch (extension)
        {
            case ".wav":
                audioType = AudioType.WAV;
                break;
            case ".mp3":
                audioType = AudioType.MPEG;
                break;
            case ".ogg":
                audioType = AudioType.OGGVORBIS;
                break;
            case ".aiff":
                audioType = AudioType.AIFF;
                break;
            default:
                Debug.LogWarning("未知音频格式，尝试自动检测: " + extension);
                break;
        }

        // 使用正确的方式加载本地音频文件
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("加载音频失败: " + www.error);
            Debug.LogError("请求URL: " + www.url);
            UpdateStatusText($"播放失败: {www.error}", Color.red);
        }
        else
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            if (audioClip != null)
            {
                Debug.Log("音频加载成功，长度: " + audioClip.length + "秒");
                UpdateStatusText($"播放中: {audioClip.length:F1}秒", Color.green);

                // 确保有AudioSource组件
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }

                // 播放音频
                audioSource.clip = audioClip;
                audioSource.Play();

                // 等待播放完成
                yield return new WaitForSeconds(audioClip.length);
                UpdateStatusText("播放完成", Color.white);
            }
            else
            {
                Debug.LogError("无法解析音频文件，可能是格式不支持");
                Debug.LogError("文件路径: " + filePath);
                Debug.LogError("文件扩展名: " + extension);
                UpdateStatusText("播放失败: 不支持的音频格式", Color.red);
            }
        }

        www.Dispose();
    }

    private void OnAudioItemDelete(TTSAudioItem item)
    {
        // 从列表中移除
        audioList.Remove(item);

        // 删除文件
        if (File.Exists(item.FilePath))
        {
            try
            {
                File.Delete(item.FilePath);
                UpdateStatusText($"已删除: {item.FileName}", Color.yellow);
            }
            catch (Exception ex)
            {
                Debug.LogError($"删除文件失败: {ex.Message}");
                UpdateStatusText($"删除失败: {item.FileName}", Color.red);
            }
        }
    }

    private void UpdateStatusText(string message, Color color = default)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color == default ? Color.white : color;
        }
    }
}