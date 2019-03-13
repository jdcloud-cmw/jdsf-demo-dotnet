# JDSF DotNet Core Demo

## 环境准备

* 项目使用 dotnet core 2.2 版本进行编写，在运行时需要安装 dotnet core 2.2 的SDK

* 如果需要修改代码，推荐安装Visual Studio 2017 ,Visual Studio 2019 或者 Visual Studio Code 等开发工具
  
## 项目结构

|- OpenTracingDemo  
|- OpenTracingDemo.Common  
|- OpenTracingDemo.Server  
|- OpenTracingDemo.sln

其中 OpenTracingDemo.Server 是服务的生产者  
OpenTracingDemo 为服务的消费者  
OpenTracingDemo.Common 为 asp dotnet Core 通用类库，主要实现了负载、调用链、和注册中心响应的功能  
OpenTracingDemo.sln 为项目的解决方案

## 项目依赖类库说明

* consul ：注册中心使用类库

* OpenTracing.Contrib.NetCore ：基于OpenTracing 的NetCore 开源类库

* Jaeger : 链路跟踪的实现方案

* Refit.HttpClientFactory : Refit 实现的基于Rest接口的Api 调用，可以实现基于接口描述生成Http请求的类库[refit GitHub](https://github.com/reactiveui/refit)

## 配置及使用说明

* 项目在Programe 中使用了基于配置文件加载的启动 ip 和端口，如果不进行配置默认使用的地址为 http://localhost:5000 需要在 appsettings.Development.json 或者appsettings.json 配置 Server 属性，具体的配置如下：  

  ```json
   "Server": {
        "Host": "0.0.0.0",
        "Port": 5002
    }
  ```

* 在代码中需要配置应用的名称以及调用链采集服务的地址和端口 等信息，具体代码需要在`StartUp.cs` 中，详细代码如下

  ```csharp
    services.AddJaeger(new OpenTracingOptions { Sampler = new ConstSampler(true), SenderType = SenderType.UDPSender }); //添加 jaeger 配置信息，可以配置调用的地址 详细查看OpenTracingDemo.Common/ServiceCollectionExtensions/JaegerServiceCollectionExtensions 中的初始化Jaeger 代码
    services.AddOpenTracing(); // 开启OpenTracing 链路跟踪

    services.AddServiceRegistry(new ConsulConfigOption
    {
        ConsulHost = "10.12.209.43", // consul 注册中心地址
        ConsulPort = 8500, // consul 注册中心端口号
        ConsulSchame = "http" // consul 注册中心使用的协议
    }, new DiscoverOption
    {
        ServiceName = "db-service", // 当前服务的名称
        IpAddress = "10.12.140.173", // 服务要注册在注册中心的IP地址
        Port = 5002, // 服务要注册在注册中心的端口号
        InstanceId = "db-service-1", // 服务要注册在注册中心的实例id
        PreferIpAddress = true // 是否使用皮注册
    });
  ```

## 代码运行与调试

* 配置上面的配置文件，启动Consul 和 Jaeger 确认服务没有问题

* 如果在 windows 、linux 或者 mac 环境下 可以在安装dotnet core 2.2 版本的SDK后，在OpenTracingDemo.Server 文件夹下开启命令行、 bash shell或者 powershell 执行

  ```powershell
    dotnet restore
    dotnet run
  ```

    启动生产者，然后使用相同的命令在文件夹OpenTracingDemo 下执行启动消费者，然后访问URL `http://<host>:<port>/api/refit/gameInfo?gameId=111` 这样的链接，查看请求结果，在此过程中可以调试查看详细的代码调用逻辑
