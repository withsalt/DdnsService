# DDNS服务

DdnsService是一个检测当前运行环境外网IP以及自动设置DDNS的服务程序。支持IP变更短信提醒、邮件提醒、自动配置腾讯云DDNS和阿里云DDNS（其他接入商请自行适配）。

### Feature
1. 可作为Windows服务或Linux服务运行，也可以作为控制台程序运行。  
2. 可自选开启IP变化邮件通知或短信通知。  
3. 同时支持腾讯云DDNS和阿里云DDNS。  

### How to use
##### Windows:
1. 下载程序解压  

2. 编辑配置文件  
按照后面的提示配置短信、邮件和DDNS。在编辑配置文件时请检查相关配置是否填写正确。  
```shell
appsettings.json   #用记事本或者Notpad++打开并编辑配置文件，推荐Notepad++
```

3. 安装服务  
管理员权限打开Powershell，然后进入ServiceInstaller文件夹后执行安装命令。  
```shell
cd ServiceInstaller
DdnsServiceInstaller install  #服务安装
```

4. 启动服务  
```shell
DdnsServiceInstaller start
```

5. 其他命令  
卸载服务：`DdnsServiceInstaller uninstall`  
停止服务：`DdnsServiceInstaller stop`  
查看状态：`DdnsServiceInstaller status`  
Windows服务注册程序使用[WinSW](https://github.com/winsw/winsw "WinSW")。

##### Linux:  
1. 获取服务程序
```shell
wget https://github.com/withsalt/DdnsService/releases/download/1.3/DdnsService_Linux_AMD_64.zip && unzip DdnsService_Linux_AMD_64.zip && cd DdnsService_Linux_AMD_64 && sudo chmod +x DdnsService
```

2. 编辑配置文件
按照后面的提示配置短信、邮件和DDNS。在编辑配置文件时请检查相关配置是否填写正确。
```shell
nano appsettings.json
```

3. 编辑服务配置文件  
```shell
nano ServiceInstaller/Linux/ddns.service
```
  修改`WorkingDirectory`和`ExecStart`为当前程序路径

4. 注册服务  
```shell
sudo cp ServiceInstaller/Linux/ddns.service /etc/systemd/system
sudo chmod 775 /etc/systemd/system/ddns.service
```

5. 启动服务  
```shell
sudo systemctl start ddns.service
```

6. 设置开启启动
```shell
sudo systemctl enable ddns.service
```

7. 其他命令  
卸载服务：`sudo systemctl disable ddns.service && sudo rm -rf /etc/systemd/system/ddns.service`  
停止服务：`sudo systemctl stop ddns.service`  
查看状态：`sudo systemctl status ddns.service`  
取消开机启动：`sudo systemctl disable ddns.service`  

### 如何开启短信提醒（目前仅支持企业用户申请）  
1. 注册急速数据短信API，注册地址：https://www.jisuapi.com/  
2. 获取短信API。  
3. 添加短信子账号（签名）  
![01.jpg](https://github.com/withsalt/DdnsService/blob/master/doc/01.jpg)
4. 点击模板，添加短信模板  
![03.jpg](https://github.com/withsalt/DdnsService/blob/master/doc/03.jpg)
5. 添加模板  
![04.jpg](https://github.com/withsalt/DdnsService/blob/master/doc/04.jpg)

短信格式：  
```shell
IP地址变更提醒：IP地址已变更，当前IP[@]，历史IP[@]。【刚刚添加企业的时候使用的签名】
```
一定要严格按照短信格式填写，并把签名替换为自己申请的签名。  

6. 修改appsettings.json  
申请后将自己的短信API填写到配置文件中，并设置`IsEnableEmailNotice`为`true`  

### 如何开启邮件提醒  
1. 注册一个支持STMP的邮箱  
此处以163邮箱为例。  
2. 点击STMP服务  
![05.jpg](https://github.com/withsalt/DdnsService/blob/master/doc/05.jpg)
3. 按照提示开启STMP服务  
4. 开启服务后将参数填写到配置文件中，并设置`IsEnableMessageNotice`为`true`  

### 如何开启DDNS
目前程序仅支持阿里云DDNS和腾讯云DDNS，如果想支持自定义服务商DDNS，请自行下载代码并完善。  
代码中DDNS服务独立为一个SDK，新建类后继承`IDdnsService`并重写相关服务。

1. 有自己的域名
2. 创建AccessKey，https://help.aliyun.com/document_detail/53045.html
需要注意的是，一定要去授予DDNS权限。
3. 将AccessKey和AccessKeySecret填写到配置文件中，并设置`IsEnableDdns`为`true`
4. 配置域名DDNS信息
```json
"DdnsConfig": {
  "IsEnableDdns": true,
  "DdnsServiceProviders": [  //同时支持多个域名并支持不同的域名使用不同的DDNS提供商
    {
      "Id": "1",
      "Type": "Aliyun",
      "AccessKey": "****************",
      "AccessKeySecret": "***************************"
    },
    {
      "Id": "2",
      "Type": "TencentCloud",
      "AccessKey": "****************",
      "AccessKeySecret": "***************************"
    }
  ],
  "Domains": [
    {
      "Provider": "1", //上面的DdnsServiceProviders条目ID
      "Domain": "xxx.xxxx.com",
      "TTL": 600
    },
	{
      "Provider": "2", //上面的DdnsServiceProviders条目ID
      "Domain": "xxx.xxxx.com",
      "TTL": 600
    }
  ]
}
```
在`DdnsConfig`配置结点中，`DdnsServiceProviders`为服务提供者信息，只需要修改AccessKey和AccessKeySecret即可。然后将自己的域名填写到`Domains`结点中，`Provider`为上方`DdnsServiceProviders`条目中的ID值，程序会根据此ID查找对应的服务商来设置DDNS。  
