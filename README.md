# StatsdInterceptor
Interceptor for CodeCop (http://getcodecop.com) that automatically integrates <b>Statsd</b> onto intercepted methods. 

This interceptor automatically adds these metrics to each intercepted method:
- "times_called" - Counter for the number of times that method has been called.
- "num_errors" - Counter for the number of exceptions that method had.
- "total_executiontime" - Counter for how many time it took for that method to execute (ms).


# Instructions
To place this Interceptor on all intercepted methods of a type, just insert "StatsdInterceptor" in the GlobalInterceptors array of your copconfig.json file, like so:

```
{
    "Types": [
        {
            "TypeName": "WebApplication1.Controllers.HomeController, WebApplication1",
            "Methods": [
                {
                    "MethodSignature": "*",
                    "Interceptors": [ ]
                }
            ],
           "GenericArgumentTypes": []

        }

    ],
    "GlobalInterceptors": ["StatsdInterceptor"],
    "Key":"Your product key or leave empty for free product mode"
}
```
If you want to use this Interceptor on just some methods, inside the copconfig.json file insert "StatsdInterceptor" in the Interceptors array of those methods, like so:
```
{
    "Types": [
        {
            "TypeName": "WebApplication1.Controllers.HomeController, WebApplication1",
            "Methods": [
                {
                    "MethodSignature": "Index",
                    "Interceptors": ["StatsdInterceptor" ]
                },
                {
                    "MethodSignature": "About",
                    "Interceptors": ["StatsdInterceptor" ]
                },
                
            ],
           "GenericArgumentTypes": []

        }

    ],
    "GlobalInterceptors": [],
    "Key":"Your product key or leave empty for free product mode, limited to intercepting 25 methods"
}
```
<b>Configuration</b>

Add these entries to your appsettings section in web.config:
```
<add key="StatsdServerName" value="localhost" />
<add key="Prefix" value="AnyPrefixHereWillBePrependedToStatsName" />
<add key="StatsdServerPort" value="8125" />
```


# Nuget Package
https://www.nuget.org/packages/Statsd.CodeCop/

Make sure you read the CodeCop wiki page at https://bitbucket.org/codecop_team/codecop/wiki/Home to get started using this powerful method interception tool for .NET .

