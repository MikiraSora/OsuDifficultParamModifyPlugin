## DifficultParamModifyPlugin ~ 基于osu文件修改的自定义难度插件

**本插件还在实现当中，但核心功能已经完成并且可用**

### 简介
本插件是Sync插件,借助于ORTDP插件可以在打图加载osu文件前对osu文件修改diff难度值,达到方便打图的效果.

### 原理
通常打图前是还要一次加载铺面文件，可以赶在加载铺面前对osu文件进行修改。

### 声明
理论上这个插件是安全的(打图结算应该再会对铺面文件做效验防止上传)。此插件仅仅是在打图加载铺面前对铺面文件修改，并没有任何修改osu!内存等操作,但以防万一，后果自负。

### 用法
编译，并放在Sync/Plugins文件夹即可.<br/>
`modify_diff listen <true/false>`开启文件监听<br>
`modify_diff set <param>`设置自定义修改难度值,比如钦定要打的图都是ar10以及hp=10,那么可以<br>
>
> modify_diff set ar:10;hp:10;<br>
> 恢复成铺面默认值可以随便乱打值:<br>
> modify_diff set ar:defualt;hp:5.5; //将把ar设置成铺面默认的，hp改成5.5
>

### 最后
辣鸡HR，怎么还tm改CS的，呸
