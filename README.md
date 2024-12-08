# DevL2

## 技术栈

- .NET8
- ASP.NET Core WebAPI
- RESTful API
- Swagger
- Global Exception Handling 全局异常处理
- Health Checkup 健康检查
- Unit Test 单元测试

## 全局异常处理

采用中间件的方式，进行全局的异常处理

```csharp
using System.Net;
using System.Text.Json;
using DevL2.WebAPI.Common;

namespace DevL2.WebAPI.Middleware;

/// <summary>
/// Exception middleware
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };
        
        var response = new Response<object>(context.Response.StatusCode, ex.Message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

## 健康检查

- 支持多种类型数据库健康检查 SqlServer、MySQL ...
- 支持多个第三方服务健康检查
- 提供 /api/Health/health 接口，可根据接口响应报文，观察健康检查结果

/api/Health/health 接口响应报文

```json
{
    "status": "Unhealthy",
    "results": {
        "DatabaseHealthCheck": {
            "status": "Unhealthy",
            "description": "One or more databases are unhealthy.",
            "details": {
                "SqlServer": {
                    "SqlServerDB": "SQL Server database 'SqlServerDB' connection failed: A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)"
                },
                "MySql": {
                    "MySqlDB": "MySQL database 'MySqlDB' is healthy."
                }
            }
        },
        "ThirdPartyServiceHealthCheck": {
            "status": "Unhealthy",
            "description": "One or more third-party services are unhealthy.",
            "details": {
                "ServiceA": "Error: The SSL connection could not be established, see inner exception.",
                "ServiceB": "Error: The SSL connection could not be established, see inner exception."
            }
        }
    }
}
```

appsetting.json 配置文件内容

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DatabaseConnections": [
    {
      "DatabaseType": "SqlServer",
      "Name": "SqlServerDB",
      "ConnectionString": "Server=127.0.0.1;Database=myDataBase;User Id=sa;Password=sa;"
    },
    {
      "DatabaseType": "MySql",
      "Name": "MySqlDB",
      "ConnectionString": "Server=127.0.0.1;Port=13306;Database=myDataBase;User=root;Password=123456;"
    }
  ],
  "ThirdPartyServices": [
    {
      "Name": "ServiceA",
      "Url": "https://api.example.com/health"
    },
    {
      "Name": "ServiceB",
      "Url": "https://api.anotherexample.com/status"
    }
  ],
  "AllowedHosts": "*"
}
```