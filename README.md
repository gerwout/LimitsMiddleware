Limits Middleware
===========

[![Build status](https://ci.appveyor.com/api/projects/status/o6bm02n8stya868r)](https://ci.appveyor.com/project/damianh/limitsmiddleware) [![NuGet Status](http://img.shields.io/nuget/v/LimitsMiddleware.svg?style=flat)](https://www.nuget.org/packages/LimitsMiddleware/) [![NuGet Status](http://img.shields.io/nuget/v/LimitsMiddleware.OwinAppBuilder.svg?style=flat)](https://www.nuget.org/packages/LimitsMiddleware.OwinAppBuilder/)

OWIN middleware to apply limits to an OWIN pipeline:

 - Max bandwidth
 - Max concurrent requests
 - Connection timeout
 - Max query string
 - Max request content length
 - Max url length
 
#### Installation

There are two nuget packages. The main one is pure owin and this has no dependencies.

`install-package StatusCodeHandlersMiddleware`

The second package provides integration with IAppBuilder, which is deprecated but provided here for legacy and compatability reasons.

`install-package StatusCodeHandlersMiddleware.OwinAppBuilder`

An asp.net vNext builder integration package will be forthcoming.

#### Using

Configuration values can be supplied as constants or with a delegate. The latter allows you to change the values at runtime. Use which ever you see fit. This code assumes you have the above `AppBuilderExtensions` class in your application. 


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

##### Developed with:

[![Resharper](http://neventstore.org/images/logo_resharper_small.gif)](http://www.jetbrains.com/resharper/)
[![TeamCity](http://neventstore.org/images/logo_teamcity_small.gif)](http://www.jetbrains.com/teamcity/)
[![dotCover](http://neventstore.org/images/logo_dotcover_small.gif)](http://www.jetbrains.com/dotcover/)
[![dotTrace](http://neventstore.org/images/logo_dottrace_small.gif)](http://www.jetbrains.com/dottrace/)

