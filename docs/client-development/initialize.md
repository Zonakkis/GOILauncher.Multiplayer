# 初始化插件

在使用插件之前，需要通过`MultiplayerUnityCore`类初始化插件。

## 初始化
初始化有两种方式：
1. 普通初始化。此时将使用`MultiplayerUnityCore`中的`IUnityClient`和`IUnityServer`全局单例。
2. 带依赖注入的初始化。此时`IUnityClient`和`IUnityServer`实例可以被依赖注入到您的服务中。
::: tip
插件内置的依赖注入实现为Autofac。
:::

::: code-group

```csharp [普通初始化]
using GOILauncher.Multiplayer.Unity;

MultiplayerUnityCore.Initialize();

public interface IMyService { }
public class MyService : IMyService
{
    public void Foo() 
    {
        MultiplayerUnityCore.UnityClient. // Do something...
    }
}
```

```csharp [带依赖注入的初始化]
using GOILauncher.Multiplayer.Unity;

var container = MultiplayerUnityCore.Initialize(builder =>{
    builder.RegisterType<MyService1>()
        .AsSelf()
        .SingleInstance();
    builder.RegisterType<MyService>()
        .As<IMyService>()
        .As<IStartable>() 
        .SingleInstance();

});

public class MyService1
{
    public MyService1(IUnityClient unityClient) { }
}

public interface IMyService2 { }
public class MyService2 : IMyService2, IStartable
{
    public MyService2(IUnityServer unityServer, MyService1 service1) { }
    public void Start() 
    { 
        // Do something... 
    }
}
```
:::

## 开始使用
无论是通过全局单例还是依赖注入，现在都可以开始开发您的客户端了！

为了简单起见，接下来的所有示例将使用全局单例，且默认已经初始化。