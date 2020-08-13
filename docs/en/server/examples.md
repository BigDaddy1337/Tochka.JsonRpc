# Server/Examples

Here are examples for different scenarios. Common things like default HTTP headers, calls to `AddMvc().SetCompatibilityVersion()` are omitted.

> For details beyond basic usage check [Configuration](configuration.md) page

## Request, Notification, Batch with default configuration

Examples of basic JSON Rpc calls with default configuration
<details>
<summary>Expand</summary>

> `Startup.cs`
```cs
.AddJsonRpcServer()
```

> `EchoController.cs`
```cs
public class EchoController : JsonRpcController
{
    public string ToLower(string value)
    {
        return value.ToLower();
    }
}
```

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Response
    </td>
</tr>

<tr>

<td valign="top">

JSON Rpc Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "echo.to_lower",
    "params": {
        "value": "TEST"
    }
}
```

</td>
<td valign="top">

Normal response
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "result": "test"
}
```

</td>
</tr>

<tr>

<td valign="top">

JSON Rpc Notification
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "jsonrpc": "2.0",
    "method": "echo.to_lower",
    "params": {
        "value": "TEST"
    }
}
```

</td>
<td valign="top">

No response content by specification
```HTTP
HTTP/1.1 200 OK
Content-Length: 0
```

</td>
</tr>

<tr>

<td valign="top">

JSON Rpc Batch
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS NUMBER"
        }
    },
    {
        "id": "abc",
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS STRING"
        }
    },
    {
        "id": null,
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH NULL ID"
        }
    },
    {
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "NOTIFICATION, NO RESPONSE EXPECTED"
        }
    }
]
```

</td>
<td valign="top">

Responses for all items, except for notifications
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "result": "request with id as number"
    },
    {
        "id": "abc",
        "jsonrpc": "2.0",
        "result": "request with id as string"
    },
    {
        "id": null,
        "jsonrpc": "2.0",
        "result": "request with null id"
    }
]
```

</td>
</tr>


</table>
</details>


## AllowRawResponses

Break protocol a bit and return bytes, HTTP codes, etc.
<details>
<summary>Expand</summary>

> `Startup.cs`
```cs
.AddJsonRpcServer(options => {
    options.AllowRawResponses = true;
});
```

> `DataController.cs`
```cs
public class DataController : JsonRpcController
{
    public ActionResult GetBytes(int count)
    {
        var bytes = Enumerable.Range(0, count).Select(x => (byte)x).ToArray();
        return new FileContentResult(bytes, "application/octet-stream");
    }

    public ActionResult Redirect(string url)
    {
        return RedirectPermanent(url);
    }
}
```

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Response
    </td>
</tr>

<tr>

<td valign="top">

GetBytes Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "data.get_bytes",
    "params": {
        "count": 100
    }
}
```

</td>
<td valign="top">

Unmodified bytes in response
```HTTP
HTTP/1.1 200 OK
Content-Type: application/octet-stream
Content-Length →100
```
```
�    

 !"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abc
```

</td>
</tr>

<tr>

<td valign="top">

Redirect Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "data.redirect_to",
    "params": {
        "url": "https://google.com"
    }
}
```

</td>
<td valign="top">

HTTP Redirect
```HTTP
HTTP/1.1 301 Moved Permanently
Content-Length: 0
Location: https://google.com
```

</td>
</tr>

<tr>

<td valign="top">

JSON Rpc Batch
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "method": "data.get_bytes",
        "params": {
            "count": 100
        }
    }
]
```

</td>
<td valign="top">

JSON Rpc Error
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "error": {
            "code": -32001,
            "message": "Server error",
            "data": {
                "internal_http_code": null,
                "message": "Raw responses are not allowed by default and not supported in batches, check JsonRpcOptions",
                "details": null,
                "type": "Tochka.JsonRpc.Server.Exceptions.JsonRpcInternalException"
            }
        }
    }
]
```

</td>
</tr>


</table>
</details>


## DetailedResponseExceptions

Hide or reveal exception information
<details>
<summary>Expand</summary>

> `Startup.cs`
```cs
.AddJsonRpcServer(options => {
    options.DetailedResponseExceptions = /*true or false*/;
});
```

> `ErrorController.cs`
```cs
public class ErrorController : JsonRpcController
{
    public string Fail()
    {
        throw new NotImplementedException("not ready yet, come here later!");
    }
}
```

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Response
    </td>
</tr>

<tr>

<td valign="top">

Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "error.fail",
    "params": null
}
```

</td>
<td valign="top">

No details when `DetailedResponseExceptions` is **false**
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "internal_http_code": null,
            "message": "not ready yet, come here later!",
            "details": null,
            "type": "System.NotImplementedException"
        }
    }
}
```

</td>
</tr>

<tr>

<td valign="top">

Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "error.fail",
    "params": null
}
```

</td>
<td valign="top">

`ExceptionInfo` object when `DetailedResponseExceptions` is **true**
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "internal_http_code": null,
            "message": "not ready yet, come here later!",
            "details": "System.NotImplementedException: not ready yet, come here later!\r\n   at WebApplication1.Controllers.ErrorController.Fail() in C:\\Users\\rast\\source\\repos\\WebApplication1\\WebApplication1\\Controllers\\ValuesController.cs:line 73\r\n   at lambda_method(Closure , Object , Object[] )\r\n   at Microsoft.AspNetCore.Mvc.Internal.ActionMethodExecutor.SyncObjectResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.InvokeActionMethodAsync()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.InvokeNextActionFilterAsync()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.Rethrow(ActionExecutedContext context)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.InvokeInnerFilterAsync()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.InvokeNextResourceFilter()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.Rethrow(ResourceExecutedContext context)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.InvokeFilterPipelineAsync()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.InvokeAsync()\r\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.Invoke(HttpContext httpContext)\r\n   at Microsoft.AspNetCore.Routing.EndpointRoutingMiddleware.Invoke(HttpContext httpContext)\r\n   at Tochka.JsonRpc.Server.Services.RequestHandler.SafeNext(IUntypedCall call, HandlingContext context, Boolean allowRawResponses)",
            "type": "System.NotImplementedException"
        }
    }
}
```

</td>
</tr>


</table>
</details>


## Routes

Override routing with global setting or attribute
<details>
<summary>Expand</summary>

Change default route and override it with custom route in controller or action
> `Startup.cs`
```cs
.AddJsonRpcServer(options => {
    options.DefaultMethodOptions.Route = "/public_api";
});
```

> `UsersController.cs`
```cs
/*[Route] override is also possible here*/
public class UsersController : JsonRpcController
{
    public List<string> GetNames()
    {
        return new List<string> { "Alice", "Bob" };
    }

    [Route("/admin_api")]
    public Guid Create(string name)
    {
        // add user to DB and return ID
        return Guid.NewGuid();
    }
}
```

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Response
    </td>
</tr>

<tr>

<td valign="top">

Request to GetNames at default route
```http
POST /public_api HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
	"id": 1,
    "jsonrpc": "2.0",
    "method": "users.get_names",
    "params": null
}
```

</td>
<td valign="top">

Normal response
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "result": [
        "Alice",
        "Bob"
    ]
}
```

</td>
</tr>

<tr>

<td valign="top">

Request to Create at default route 
```http
POST /public_api HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
	"id": 1,
    "jsonrpc": "2.0",
    "method": "users.create",
    "params": {
    	"name": "Charlie"
    }
}
```

</td>
<td valign="top">

Error response
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "error": {
        "code": -32601,
        "message": "Method not found",
        "data": null
    }
}
```

</td>
</tr>

<tr>

<td valign="top">

Request to Create at overridden route 
```http
POST /admin_api HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
	"id": 1,
    "jsonrpc": "2.0",
    "method": "users.create",
    "params": {
    	"name": "Charlie"
    }
}
```

</td>
<td valign="top">

Normal response
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "result": "ad355447-ee5e-4418-96b4-171e36fa994b"
}
```

</td>
</tr>

</table>
</details>


## MethodStyle

Change how `method` property is matched to controllers and actions
<details>
<summary>Expand</summary>

Request's `method` property can be sent in different formats depending on global setting: as `controller.action` or as `action`
> `Startup.cs`
```cs
.AddJsonRpcServer(options => {
    options.DefaultMethodOptions.MethodStyle = /* MethodStyle.ControllerAndAction or MethodStyle.ActionOnly*/;
});
```

> `EchoController.cs`
```cs
/*[JsonRpcMethodStyle] override is also possible here*/
public class EchoController : JsonRpcController
{
    /*[JsonRpcMethodStyle] override is also possible here*/
    public string ToLower(string value)
    {
        return value.ToLower();
    }
}
```

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Response
    </td>
</tr>

<tr>

<td valign="top">

Request with method with `controller.action` (MethodStyle.ControllerAndAction)
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "echo.to_lower",
    "params": {
        "value": "TEST"
    }
}
```

</td>
<td valign="top">

Response from `EchoController.ToLower`
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "result": "test"
}
```

</td>
</tr>

<tr>

<td valign="top">

Request with method with `action` (MethodStyle.ActionOnly)
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "to_lower",
    "params": {
        "value": "TEST"
    }
}
```

</td>
<td valign="top">

Response from `EchoController.ToLower`
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "result": "test"
}
```

</td>
</tr>


</table>
</details>


## Serialization

Change default JSON serializer/deserializer or override it for controller/action. See [Serialization](serialization.md) for details.
<details>
<summary>Expand</summary>

Note how changing serialization affects `params` and `method`.
> `Startup.cs`
```cs
.AddJsonRpcServer(options => {
    options.DefaultMethodOptions.RequestSerializer = typeof(CamelCaseJsonRpcSerializer);
});

services.TryAddJsonRpcSerializer<CamelCaseJsonRpcSerializer>();
```

> `SimpleCalcController.cs`
```cs
/*[JsonRpcSerializer] override is also possible here*/
public class SimpleCalcController : JsonRpcController
    {
        public object SubtractIntegers(int firstValue, int secondValue)
        {
            var result = firstValue - secondValue;
            return new
            {
                firstValue,
                secondValue,
                firstMinusSecond = result
            };
        }

        [JsonRpcSerializer(typeof(SnakeCaseJsonRpcSerializer))]
        public object AddIntegers(int firstValue, int secondValue)
        {
            var result = firstValue + secondValue;
            return new
            {
                firstValue,
                secondValue,
                firstPlusSecond = result
            };
        }
    }
```

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Response
    </td>
</tr>

<tr>

<td valign="top">

Request with camelCase
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
	"id": 1,
    "jsonrpc": "2.0",
    "method": "simpleCalc.subtractIntegers",
    "params": {
    	"firstValue": 42,
    	"secondValue": 38
    }
}
```

</td>
<td valign="top">

Response with camelCase
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "result": {
        "firstValue": 42,
        "secondValue": 38,
        "firstMinusSecond": 4
    }
}
```

</td>
</tr>

<tr>

<td valign="top">

Request with snake_case
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
	"id": 1,
    "jsonrpc": "2.0",
    "method": "simple_calc.add_integers",
    "params": {
    	"first_value": 42,
    	"second_value": 38
    }
}
```

</td>
<td valign="top">

Response with snake_case
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "result": {
        "first_value": 42,
        "second_value": 38,
        "first_plus_second": 80
    }
}
```

</td>
</tr>


</table>
</details>


## Binding

Change how `params` are bound to method arguments. See [Binding](binding.md) for details.
<details>
<summary>Expand</summary>

### Default behavior: `params` are bound to method arguments. `params` can be `[]` or `{}` by specification.

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Action method
    </td>
</tr>

<tr>

<td valign="top">

Request has object with two properties
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": "test"
    }
}
```

</td>
<td valign="top">

`params` are bound to method arguments by names
```cs
public void Foo(int bar, string baz){
    // bar == 1
    // baz == "test"
}
```

</td>
</tr>

<tr>

<td valign="top">

Request has array with two items
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": [
        1,
        "test"
    ]
}
```

</td>
<td valign="top">

`params` are bound to method arguments by indices
```cs
public void Foo(int bar, string baz){
    // bar == 1
    // baz == "test"
}
```

</td>
</tr>


</table>

### Bind whole `params` object into one model, eg. when model has lots of properties

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Action method
    </td>
</tr>

<tr>

<td valign="top">

Request has object with two properties
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": "test"
    }
}
```

</td>
<td valign="top">

`params` are bound to single method argument
```cs
public class Data
{
    public int Bar { get; set; }
    public string Baz { get; set; }
}

public void Foo([FromParams(BindingStyle.Object)] Data data){
    // data.Bar == 1
    // data.Baz == "test"
}
```

</td>
</tr>

<tr>

<td valign="top">

Request has array with two items
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": [
        1,
        "test"
    ]
}
```

</td>
<td valign="top">

Error because array items can not be bound to object properties
```cs
public class Data
{
    public int Bar { get; set; }
    public string Baz { get; set; }
}

public void Foo([FromParams(BindingStyle.Object)] Data data){
    // does not work for `params` array
}
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "data": [
                "Bind error. Can not bind array to object parameter. Json key [0]"
            ]
        }
    }
}
```

</td>
</tr>


</table>

</details>


### Bind `params` array into one collection, eg. when request has variable count of parameters

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Action method
    </td>
</tr>

<tr>

<td valign="top">

Request has object with two properties
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": 2
    }
}
```

</td>
<td valign="top">

Error because object properties can not be bound to array items
```cs
public void Foo([FromParams(BindingStyle.Array)] List<int> data){
    // does not work for `params` object
}
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "data": [
                "Bind error. Can not bind object to collection parameter. Json key [data]"
            ]
        }
    }
}
```

</td>
</tr>

<tr>

<td valign="top">

Request has array with two items
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": [
        1,
        2
    ]
}
```

</td>
<td valign="top">

Array items are bound to collection
```cs
public void Foo([FromParams(BindingStyle.Array)] List<int> data){
    // data[0] == 1
    // data[1] == 2
}
```

</td>
</tr>


</table>

</details>


### Bind whole `params` object into one model, eg. when model has lots of properties

<table>
<tr>
    <td>
        Request
    </td>
    <td>
        Action method
    </td>
</tr>

<tr>

<td valign="top">

Request has object with two properties
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": "test"
    }
}
```

</td>
<td valign="top">

`params` are bound to single method argument
```cs
public class Data
{
    public int Bar { get; set; }
    public string Baz { get; set; }
}

public void Foo([FromParams(BindingStyle.Object)] Data data){
    // data.Bar == 1
    // data.Baz == "test"
}
```

</td>
</tr>

<tr>

<td valign="top">

Request has array with two items
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": [
        1,
        "test"
    ]
}
```

</td>
<td valign="top">

Error because array items can not be bound to object properties
```cs
public class Data
{
    public int Bar { get; set; }
    public string Baz { get; set; }
}

public void Foo([FromParams(BindingStyle.Object)] Data data){
    // does not work for `params` array
}
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "data": [
                "Bind error. Can not bind array to object parameter. Json key [0]"
            ]
        }
    }
}
```

</td>
</tr>


</table>

</details>


### Mix different binding sources

Also try default params, object, dynamic and custom serialization!
```cs
public void Foo1(object bar, dynamic baz, [FromParams(BindingStyle.Object)] Data data, [FromServices]ICustomService service, CancellationToken token){
    // bar, baz are bound by default
    // data is bound with specified behavior
    // service and token are bound by framework as usual
}

public void Foo2(int? bar, string baz="default_value"){
    // "params" can have:
    // "bar": null
    // and omit baz entirely
}
```

## Access extra information

TODO

## Errors and exceptions

TODO