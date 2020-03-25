# DdnsService

DdnsService是一个检测当前运行环境外网IP以及自动设置DDNS的服务程序。  

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