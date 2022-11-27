# SharpBot
一个基于百度网盘开放接口的转存机器人

## 使用方法
1. 前往该链接创建自己的应用 https://pan.baidu.com/union/console/createapp
2. 前往 https://t.me/BotFather 创建自己的Telegram机器人
``` yaml  
version: "3"

services:
  sharpbot:
    image: crinte/sharpbot:main
    restart: unless-stopped
    container_name: sharpbot
    environment:
      # 配置选项参考 https://github.com/Cnicehs/SharpBot/blob/master/SharpBot/config/config.yaml
      # 环境变量仅在配置项为空时进行覆盖，并写入配置
      # 运行时以/app/config/config.yaml中的数据为准
      SharpBot_BaiduConfig_ClientID: #百度应用的AppKey
      SharpBot_BaiduConfig_ClientSecret: #百度应用的SecretKey
      SharpBot_BaiduConfig_DownloadPath: #网盘内的存放位置，该位置必须是已经存在的文件夹
      SharpBot_TelegramConfig_BotToken: #TelegramBot的token
      SharpBot_TelegramConfig_ChatID: #用户自身的TelegramID，用于生成百度Token时的操作交互
      SharpBot_TelegramConfig_TransFinishAddtionDesc: #执行结束后的通知文本，如到Alist查看转存结果
    volumes:
      - ./sharpbot/config:/app/config
```  
3. 初次运行时会收到机器人申请应用的信息，按照提示操作即可
4. 若要清除应用数据，删除 ./sharpbot/config 下的所有数据即可
5. 使用时发送如下的链接格式即可，单次消息只能发送一条分享连接
```
链接: https://pan.baidu.com/s/1GQuNCeAzb92rl2KB68X3wA 提取码: aww4 复制这段内容后打开百度网盘手机App，操作更方便哦

链接：https://pan.baidu.com/s/15yXtvMIKaozgfdw5dMRy0A 
提取码：lsol 
--来自百度网盘超级会员V6的分享
```  
## Licence
[MIT licence](https://github.com/Cnicehs/SharpBot/blob/main/LICENSE)