# 鹅鸭杀辅助工具
仅供学习交流，不对使用产生的后果负责

仅支持64位游戏进程，因为注入的汇编代码只写了64位的，懒得再写32位，毕竟2023年了...

本项目代码不对游戏数值进行修改，如需修改要同时修改隐藏的加密数值，游戏的反作弊模块原理请查看：https://codestage.net/uas/actk

# 功能
使用il2cpp函数读取地址，游戏版本更新也基本无需更新代码

游戏多开，使用方法请查看下面图片，原理就是把Mutex的句柄Kill掉，进游戏要用不同账号，不然没法进同一个房间

统计玩家信息(是否隐身、变形、被鹈鹕吃等等) 以及游戏信息（当前地图、房间号、当前游戏进程等）

当前死亡数、玩家位置的地图标记，标点分为普通(白色)、嫌疑人(粉色，进雾/开刀)、鸭子（暗红，隐身/变形），颜色和字体可自行设置

嫌疑人判定：某玩家死亡/被鹈鹕吃瞬间，离其最近的玩家

# 计划
添加每轮游戏回放功能，方便看清楚哪个老六混刀（GDI截屏在WIN10下遇到一些问题，不知道有没懂的大佬可以交流一下）

---

#Goose Goose Duck Assistant Tool
**Disclaimer: **The author reserves the right not to be responsible for any consequence of using the codes.

Only support 64-bit game process.

This program will not modify any memory of the game. If you wanna do so, it's strongly recommended to understand how anti-cheat module of the game works in advance. Please check: https://codestage.net/uas/actk

# Features
Using original functions of il2cpp library to get the memory addresses.

Supporting multiboxing.

Collecting and analyzing the player information including 'IsInvisible', 'InTelepathic', 'IsMorphed', etc. to help users determine the identity of others player.

Display some key attributes like DeadNum, RoomId, CurrentMap, CurrentProgress, etc.

Marking all living players on map with different colors which can be modified in settings page. (Default: White = Normal, Pink = Suspects, Red = Killer)

Identifying suspects: recoding the nearest player to one who is just eaten or killed.

# Comming Features
Picture replay.

<img src="https://user-images.githubusercontent.com/26305635/217161208-dbd99b39-a21f-443b-a77a-c40fc587efa1.png" alt="drawing" width="600"/>
<img src="https://user-images.githubusercontent.com/26305635/217161217-f74d9be9-b562-4814-9b9d-f8fa02b70b32.png" alt="drawing" width="600"/>