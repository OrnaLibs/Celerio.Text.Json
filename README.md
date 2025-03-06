# Celerio.Text.Json
Celerio.Text.Json - это форк веб-фреймворка **[Celerio](https://github.com/Oxule/Celerio)**, в котором была **вырезана библиотека [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)**. 

## Использование
1. Создайте экземпляр пайплайна

```csharp
    var pipeline = new Pipeline();
```

2. (Опционально) Добавьте аутентификацию с использованием собственного секретного ключа

```csharp
    pipeline.Authentification = new DefaultAuthentification("Your Unknown Secret Key");
```

3. Настройте пайплайн (например: измените схему аутентификации или добавьте черный список IP)

4. Создайте конечную точку

```csharp
    [Route("GET", "/sum")]
    public static int Sum(int a, int b)
    {
        return a+b;
    }
```

5. Создайте и запустите экземпляр сервера

```csharp
    Server server = new Server(pipeline);
    await server.StartListening(5000);
```
