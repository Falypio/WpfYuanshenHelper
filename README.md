# WpfYuanshenHelper
原神签到 原神米友社签到系统，是用WPF编写的，可以定时签到，可以最小化到系统托盘。
### 效果如下
![image](/WpfYuanshenHelper/88888.jpg)
## 使用方法
签到是通过接口模拟请求达成目的，因此需要cookie信息来作为第一步

### 1.1 第一步，获取自己的Cookie信息
- 通过浏览器登录米哈游论坛 https://bbs.mihoyo.com/ys/
- 按```F12```，打开```开发者工具 -> Network``` 点击进入
- 刷新网页，找到以下的位置,复制Cookie后放在记事本或其它可以保存的地方
- ![image](/WpfYuanshenHelper/5555.png)
