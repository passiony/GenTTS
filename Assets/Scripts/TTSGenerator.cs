using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum Language : byte
{
    Chinese,
    English,
}

public enum VoiceType
{
    Xiaoxiao, //晓晓(女声 - 常用)
    Xiaoyi, //晓伊
    Yunxia, //云夏
    Yunjian, //云健(男声 - 常用)//男声
    Yunxi, //云希
    Yunyang, //云杨
}

public enum VoiceTypeEn
{
    Jenny, // (女声 - 常用)
    Aria, // (女声)
    Ana, // (女声)
    Guy, // (男声 - 常用)//男声
    Christopher, // (男声)
    Steffan, // (男声)
}

public static class TTSGenerator
{
    private const int MaxRetries = 3; // 最大重试次数
    private static readonly object lockObject = new object();
    private static bool isGenerating = false;

    /// <summary>
    /// 生成TTS语音文件（异步）
    /// </summary>
    public static void GenerateTTS(string filePath, string content, Language language, int speaker,
        Action<bool, string> onComplete = null, int retryCount = 0)
    {
        // 确保同一时间只有一个TTS生成任务在执行
        if (isGenerating)
        {
            string errorMsg = "TTS生成任务已在执行中，请稍后重试";
            Debug.LogWarning(errorMsg);
            ExecuteCallback(onComplete, false, errorMsg);
            return;
        }

        // 参数验证
        if (string.IsNullOrEmpty(filePath))
        {
            string errorMsg = "文件路径不能为空";
            Debug.LogError(errorMsg);
            ExecuteCallback(onComplete, false, errorMsg);
            return;
        }

        if (string.IsNullOrEmpty(content))
        {
            string errorMsg = "TTS内容不能为空";
            Debug.LogError(errorMsg);
            ExecuteCallback(onComplete, false, errorMsg);
            return;
        }

        // 标记开始生成
        lock (lockObject)
        {
            isGenerating = true;
        }

        // 创建一个线程来执行TTS生成
        Thread ttsThread = new Thread(() =>
        {
            Process process = null;
            bool success = false;
            string resultMessage = string.Empty;

            try
            {
                // 构建语言参数
                string languageParam = language switch
                {
                    Language.Chinese => "zh-CN",
                    Language.English => "en-US",
                    _ => "zh-CN" // 默认中文
                };
                string speakerParam = language switch
                {
                    Language.Chinese => ((VoiceType)speaker).ToString(),
                    Language.English => ((VoiceTypeEn)speaker).ToString(),
                    _ => speaker.ToString() // 默认中文
                };
                string voiceType = $"{languageParam}-{speakerParam}Neural";
                string escapedContent = EscapeCommandLineArgument(content);// 转义特殊字符
                string arguments = $"--voice {voiceType} --text \"{escapedContent}\" --write-media \"{filePath}\"";

                Debug.Log($"edge-tts {arguments}");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c edge-tts {arguments}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                process = new Process();
                process.StartInfo = startInfo;

                // 启动进程并等待完成
                process.Start();

                // 读取输出
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // 等待进程结束
                process.WaitForExit();

                success = process.ExitCode == 0;

                if (success)
                {
                    resultMessage = filePath;
                    Debug.Log($"TTS生成成功: {filePath}");
                }
                else
                {
                    resultMessage = $"TTS生成失败: {error}";
                    Debug.LogError($"TTS生成失败 (尝试 {retryCount + 1}/{MaxRetries}):");
                    Debug.LogError($"命令: {process.StartInfo.Arguments}");
                    Debug.LogError($"错误信息: {error}");
                }
            }
            catch (Exception ex)
            {
                success = false;
                resultMessage = $"TTS生成异常: {ex.Message}";
                Debug.LogError($"TTS生成异常: {ex.Message}");
                Debug.LogError($"堆栈跟踪: {ex.StackTrace}");
            }
            finally
            {
                // 释放资源
                if (process != null)
                {
                    process.Dispose();
                }
            }

            // 处理重试逻辑
            if (!success && retryCount < MaxRetries - 1)
            {
                // 重试
                Debug.Log($"正在重试... ({retryCount + 2}/{MaxRetries})");
                Thread.Sleep(1000); // 等待1秒后重试
                GenerateTTS(filePath, content, language, speaker, onComplete, retryCount + 1);
            }
            else
            {
                // 执行回调
                if (!success && retryCount >= MaxRetries - 1)
                {
                    resultMessage = $"TTS生成失败，已达到最大重试次数 ({MaxRetries}): {resultMessage}";
                    Debug.LogError(resultMessage);
                }

                ExecuteCallback(onComplete, success, resultMessage);

                // 标记生成结束
                lock (lockObject)
                {
                    isGenerating = false;
                }
            }
        });

        // 启动线程
        ttsThread.IsBackground = true;
        ttsThread.Start();
    }

    /// <summary>
    /// 转义命令行参数中的特殊字符
    /// </summary>
    private static string EscapeCommandLineArgument(string argument)
    {
        if (string.IsNullOrEmpty(argument))
            return argument;

        // 替换双引号为转义双引号
        argument = argument.Replace("\"", "\\\"");
        // 替换反斜杠为双反斜杠
        argument = argument.Replace("\\", "\\\\");

        return argument;
    }

    /// <summary>
    /// 在主线程中执行回调
    /// </summary>
    private static void ExecuteCallback(Action<bool, string> callback, bool success, string message)
    {
        if (callback == null)
            return;


        // 否则使用Unity的主线程调度器
        UnityMainThreadDispatcher.Instance.Enqueue(() => { callback(success, message); });
    }
}