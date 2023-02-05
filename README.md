# 鹅鸭杀辅助工具
仅支持64位游戏进程，因为注入的汇编代码只写了64位的，懒得再写32位，毕竟2023年了...

仅供学习交流，不对使用产生的后果负责

本项目代码不对游戏数值进行修改，如需修改要同时修改隐藏的加密数值，游戏的反作弊模块原理请查看：https://codestage.net/uas/actk

支持玩家信息(位置、是否可见、是否进雾等)、游戏信息（当前地图、房间号、当前游戏进程等）读取

使用il2cpp函数读取地址，游戏版本更新也基本无需更新代码

玩家位置的地图标记，标点分为普通(白色)、嫌疑人(粉色，进雾/开刀)、鸭子（暗红，隐身/变形）

# Comming Features
完善信息统计，包括死亡数显示，进雾玩家、开刀玩家、死者周围玩家等汇总信息

添加每轮游戏回放功能，方便看清楚哪个老六混刀

<img src="https://user-images.githubusercontent.com/26305635/216545084-3525d8ed-213e-48a7-abad-d7cc036b9cd2.png" alt="drawing" width="600"/>
<img src="https://user-images.githubusercontent.com/26305635/216838783-24b2af75-dc2d-4e16-ae62-f06a62a033ec.png" alt="drawing" width="600"/>