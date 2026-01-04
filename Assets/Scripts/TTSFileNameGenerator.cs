// 文件名生成器类

using System;
using System.IO;
using System.Text.RegularExpressions;

public static class TTSFileNameGenerator
{
    /// <summary>
    /// 生成包含语音类型和内容的安全文件名
    /// </summary>
    /// <param name="voiceType">语音类型</param>
    /// <param name="text">完整文本内容</param>
    /// <param name="maxContentLength">内容预览最大长度</param>
    /// <returns>安全的文件名（不包含扩展名）</returns>
    public static string GenerateFileName(string voiceType, string text, int maxContentLength = 15)
    {
        // 1. 清理文本内容
        string cleanedText = CleanTextForFileName(text);
        
        // 2. 截取文本（如果过长）
        string truncatedText = TruncateText(cleanedText, maxContentLength);
        
        // 3. 生成时间戳（毫秒级，确保唯一性）
        long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        // 4. 组合最终文件名格式：[VoiceType]_[Text]_[Timestamp]
        return $"{voiceType}_{truncatedText}_{timestamp}";
    }
    
    /// <summary>
    /// 清理文本中的非法文件名字符
    /// </summary>
    private static string CleanTextForFileName(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "Empty";
            
        // 替换空格为下划线
        string cleaned = text.Replace(' ', '_');
        
        // 移除所有非法文件名字符
        string invalidChars = new string(Path.GetInvalidFileNameChars());
        foreach (char c in invalidChars)
        {
            cleaned = cleaned.Replace(c.ToString(), string.Empty);
        }
        
        // 移除连续的下划线
        cleaned = Regex.Replace(cleaned, @"_+", "_");
        
        // 确保不为空
        return string.IsNullOrEmpty(cleaned) ? "CleanedText" : cleaned.Trim('_');
    }
    
    /// <summary>
    /// 截取文本到指定长度
    /// </summary>
    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
            
        return text.Substring(0, maxLength);
    }
}