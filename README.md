# 1151VR-HW2-412401484-ChinYiu
HW2 - 2D腳色操作

<img width="1919" height="1019" alt="image" src="https://github.com/user-attachments/assets/295d379a-7bda-48f6-8790-0539532bf6f3" />





Youtube 連結︰



說明製作流程和相關操作 ︰
● import 人物、背景2D Asset + 建立場景與地圖所需的 GameObjects
● 創建不同array 來分類人物不同動作的圖片idleSprites、runSprites、jumpSprites
● 設定移動、跳躍、地面偵測、圖片播放速度等參數
● 人物加入collider、Rigidbody 地板加入collider
● 設定Freeze Rotation Z 防止人物翻倒
● 建立 Ground Layer 標記地形方便detect 玩家是否著地
● 在角色腳底新增 Empty GameObject 作為偵測點 (groundChecker)
● 用spriteRenderer flip功能把人物往左時圖片水平翻轉
● 利用Vector2 設定WASD + space鍵來控制人物前後左右+跳起動作
● 增加人物在空中時能左右移動的功能 (也是用Vector)
● 跳起時顯示Array第0張圖，下降中切換為第2張，著地時切換第1張
● 增加人物跑步動畫、待機動畫(眨眼)
● 修復BUG, 人物跳起後會卡在第2格動畫。增加參數jumpCooldownTimer防止人物能無限跳
