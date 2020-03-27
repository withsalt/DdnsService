# DDNS服务

DdnsService是一个检测当前运行环境外网IP以及自动设置DDNS的服务程序。支持IP变更短信提醒、邮件提醒、自动配置阿里云DDNS（其他接入商请自行适配）。

### Feature
1. 可作为Windows服务或Linux服务运行，也可以作为控制台程序运行。  
2. 可自选开启IP变化邮件通知或短信通知。  
3. 支持阿里云DDNS（其他服务商DDNS自行编写程序适配，欢迎pull）。  

### How to use
##### Windows:
1. 下载程序并用管理员打开ServiceInstaller文件夹命令行  

2. 安装服务  
```shell
DdnsServiceInstaller install
```
3. 启动服务
```shell
DdnsServiceInstaller start
```

4. 其他命令
卸载服务：`DdnsServiceInstaller uninstall`  
停止服务：`DdnsServiceInstaller stop`  
查看状态：`DdnsServiceInstaller status`  
Windows服务注册程序使用[WinSW](https://github.com/winsw/winsw "WinSW")。

##### Linux:  
1. 编辑服务配置文件  
```shell
nano ServiceInstaller\Linux\ddns.service
```
修改WorkingDirectory和ExecStart为当前程序路径

2. 注册服务  
```shell
sudo nano /etc/systemd/system/ddns.service
sudo chmod 775 /etc/systemd/system/ddns.service
```
3. 启动服务  
```shell
sudo systemctl start ddns.service
```

4. 设置开启启动
```shell
sudo systemctl enable ddns.service
```

4. 其他命令
卸载服务：`sudo systemctl disable ddns.service && sudo rm -rf /etc/systemd/system/ddns.service`  
停止服务：`sudo systemctl stop ddns.service`  
查看状态：`sudo systemctl status ddns.service`  
取消开机启动：`sudo systemctl disable ddns.service`  

### 如何开启短信提醒
1. 注册急速数据短信API，注册地址：https://www.jisuapi.com/
2. 获取短信API。  
3. 添加短信子账号（签名）  
![GiJIzQ.jpg](https://s1.ax1x.com/2020/03/27/GiJIzQ.jpg)
4. 点击模板，添加短信模板  
![GiJTMj.jpg](https://s1.ax1x.com/2020/03/27/GiJTMj.jpg)
5. 添加模板  
![GiJ7ss.jpg](https://s1.ax1x.com/2020/03/27/GiJ7ss.jpg)

短信格式：'IP地址变更提醒：IP地址已变更，当前IP[@]，历史IP[@]。【刚刚添加企业的时候使用的签名】'  
一定要严格按照短信格式填写，并把签名替换为自己申请的签名。  

6. 修改appsettings.json  
申请后将自己的短信API填写到配置文件中，并设置`IsEnableEmailNotice`为`true`  

### 如何开启邮件提醒  
1. 注册一个支持STMP的邮箱  
此处以163邮箱为例。  
2. 点击STMP服务  
![GiYh01.jpg](https://s1.ax1x.com/2020/03/27/GiYh01.jpg)
3. 按照提示开启STMP服务  
4. 开启服务后将参数填写到配置文件中。，并设置`IsEnableMessageNotice`为`true`  

### 如何开启DDNS
目前程序仅支持阿里云DDNS，如果想支持自定义服务商DDNS，请自行下载代码并完善。  
代码中DDNS服务独立为一个SDK，新建类后继承`IDdnsService`并重写相关服务。

1. 有自己的域名
2. 创建AccessKey，https://help.aliyun.com/document_detail/53045.html
需要注意的是，一定要去授予DDNS权限。
3. 将AccessKey和AccessKeySecret填写到配置文件中，并设置`IsEnableDdns`为`true`
