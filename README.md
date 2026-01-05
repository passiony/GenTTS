参考：<https://github.com/rany2/edge-tts>\
**确保当前主机可正常上网**

1.  安装python，推荐版本3.11.9
2.  安装pip
3.  执行命令

```python
pip install edge-tts

```

1.  执行命令，确保可正常生成音频

```python
edge-tts --text "Hello, world!" --write-media hello.mp3

edge-tts --voice zh-CN-XiaoxiaoNeural --text "" --write-media "C:\Users\GILLAR\Desktop\hello.mp3"
```

1.  由于是本地生成音频，故音色支持能力有限，仅支持以下中文音色，与Edge浏览器中的大声朗读功能音色一致

| ShortName                    | Locale         | 地区     |
| :--------------------------- | :------------- | :----- |
| zh-HK-HiuGaaiNeural          | zh-HK          | 香港     |
| zh-HK-HiuMaanNeural          | zh-HK          | 香港     |
| zh-HK-WanLungNeural          | zh-HK          | 香港     |
| **zh-CN-XiaoxiaoNeural**     | zh-CN          | 中国（大陆） |
| **zh-CN-XiaoyiNeural**       | zh-CN          | 中国（大陆） |
| **zh-CN-YunjianNeural**      | zh-CN          | 中国（大陆） |
| **zh-CN-YunxiNeural**        | zh-CN          | 中国（大陆） |
| **zh-CN-YunxiaNeural**       | zh-CN          | 中国（大陆） |
| **zh-CN-YunyangNeural**      | zh-CN          | 中国（大陆） |
| zh-CN-liaoning-XiaobeiNeural | zh-CN-liaoning | 中国（辽宁） |
| zh-TW-HsiaoChenNeural        | zh-TW          | 台湾     |
| zh-TW-YunJheNeural           | zh-TW          | 台湾     |
| zh-TW-HsiaoYuNeural          | zh-TW          | 台湾     |
| zh-CN-shaanxi-XiaoniNeural   | zh-CN-shaanxi  | 中国（陕西） |

![](http://192.168.8.22:8091/uploads/upload_441bb09203023613e17acd1b30b59401.png)\
6\. TTSDownload.cs文件替换如下：**(若替换后，unity下不能正常生成，可尝试重启计算机)**

