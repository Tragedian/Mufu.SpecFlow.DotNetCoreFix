# .NET Core fix for SpecFlow

This is a fix for anybody trying to use [SpecFlow](http://specflow.org/) with .NET Core 2.0, inspired by the work of [SpecFlow.DotNet](https://raw.githubusercontent.com/stajs/SpecFlow.NetCore) and [SpecFlow.xUnitAdapter](https://github.com/gasparnagy/SpecFlow.xUnitAdapter).

Currently, SpecFlow doesn't officially support .NET Core, despite ongoing work in that area. Until that offical support arrives, this fix allows you to run SpecFlow as part of a .NET Core test suite.

## Installation
Install the latest release candidate from NuGet:

    Install-Package Mufu.SpecFlow.DotNetCoreFix

If you're running SpecFlow on anything other than NUnit, you need to use version 2.2.0 of SpecFlow or newer and have a `specflow.json` file in your test project. This file must be set to be copied to your output folder on every build and is used to configure your test runner (e.g. xunit):

```json
{
    "specflow": {
        "unitTestProvider": {
            "name": "xunit"
        }
    }
}
```

You must also add this property your test project to make sure your copy of the SpecFlow assembly sits in your output folder alongside the `specflow.json` file.

```xml
<PropertyGroup>
    <!-- ... -->
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <!-- ... -->
</PropertyGroup>
```

## How it works

This project is an extension to MSBuild which invokes the `specflow.exe` tool during the build process. The tool generates the `*.feature.cs` files normally produced by the SpecFlow extension to Visual Studio.

The `specflow.exe` tool runs using the .NET Framework and only works on .NET Framework projects. The DotNetCoreFix package generates a shadow project using the classic project structure, including an `app.config` file configured to the test framework you're using, and copies in all the `*.feature` files from your project. The output `*.feature.cs` files are added to the source files for the build, so the unit tests appear in the compiled assembly.
