Limits Middleware
===========

[![Join the chat at https://gitter.im/damianh/LimitsMiddleware](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/damianh/LimitsMiddleware?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build status](https://ci.appveyor.com/api/projects/status/o6bm02n8stya868r)](https://ci.appveyor.com/project/damianh/limitsmiddleware) [![NuGet Status](http://img.shields.io/nuget/v/LimitsMiddleware.svg?style=flat)](https://www.nuget.org/packages/LimitsMiddleware/) [![NuGet Status](http://img.shields.io/nuget/v/LimitsMiddleware.OwinAppBuilder.svg?style=flat)](https://www.nuget.org/packages/LimitsMiddleware.OwinAppBuilder/)

OWIN middleware to apply limits to an OWIN pipeline:

 - Max bandwidth
 - Max concurrent requests
 - Connection timeout
 - Max query string
 - Max request content length
 - Max url length
 - Min response delay
 
#### Installation

There are two nuget packages. The main one is pure owin and this has no dependencies.

`install-package LimitsMiddleware`

The second package provides integration with IAppBuilder, which is deprecated but provided here for legacy and compatability reasons.

`install-package LimitsMiddleware.OwinAppBuilder`

An asp.net vNext builder integration package will be forthcoming.

#### Using

Configuration values can be supplied as constants or with a delegate. The latter allows you to change the values at runtime. Use which ever you see fit. This code assumes you have installed 'LimitsMiddleware.OwinAppBuilder'


```csharp
public class Startup
{
    public void Configuration(IAppBuilder builder)
    {
        //static settings
        builder.Use()
            .MaxBandwidth(10000) //bps
            .MaxConcurrentRequests(10)
            .ConnectionTimeout(TimeSpan.FromSeconds(10))
            .MaxQueryStringLength(15) //Unescaped QueryString
            .MaxRequestContentLength(15)
            .MaxUrlLength(20)
            .MinResponseDelay(200ms)
            .UseEtc(..);
            
        //dynamic settings
        builder.Use()
            .MaxBandwidth(() => 10000) //bps
            .MaxConcurrentRequests(() => 10)
            .ConnectionTimeout(() => TimeSpan.FromSeconds(10))
            .MaxQueryStringLength(() => 15)
            .MaxRequestContentLength(() => 15)
            .MaxUrlLength(() => 20)
            .UseEtc(..);
    }
}
```

Questions or suggestions? Create an issue or [@randompunter] on twitter.

#### Help

Bugs? Create an issue. Questions [@randompunter](https://twitter.com/randompunter) or [OWIN room on Jabbr](https://jabbr.net/#/rooms/owin)

