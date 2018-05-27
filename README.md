# SpecFlow with .NET Core

This is a solution for anybody trying to use [SpecFlow](http://specflow.org/) with .NET Core 2.0, inspired by the work of [SpecFlow.DotNet](https://raw.githubusercontent.com/stajs/SpecFlow.NetCore) and [SpecFlow.xUnitAdapter](https://github.com/gasparnagy/SpecFlow.xUnitAdapter).

Currently, SpecFlow doesn't officially support .NET Core, despite ongoing work in that area. Until that [offical support](https://github.com/techtalk/SpecFlow/projects/2) arrives, this package allows you to generate SpecFlow tests as part of a .NET Core test suite.

## Installation
Assuming you are using Visual Studio, start by disabling the non-functioning code-behind generation provided by the SpecFlow tooling by removing the generator properties from the `.feature` files. For every feature file in your test project, you should remove these lines:

```xml
<ItemGroup>
	<Compile Update="MyFeature.feature.cs">
		<DesignTime>True</DesignTime>
		<AutoGen>True</AutoGen>
		<DependentUpon>MyFeature.feature</DependentUpon>
	</Compile>
</ItemGroup>

<ItemGroup>
	<None Update="MyFeature.feature">
		<Generator>SpecFlowSingleFileGenerator</Generator>
		<LastGenOutput>MyFeature.feature.cs</LastGenOutput>
	</None>
</ItemGroup>
```

You should then remove any `*.feature.cs` files left in your project.

### Install and configure the package:

    Install-Package Rhubarb.SpecFlow.NetCore

If you're running SpecFlow on anything other than NUnit, you need to have a `specflow.json` file in your test project. This file must be set to be copied to your output folder on every build and is used to configure your test runner (e.g. xunit):

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

The `specflow.exe` tool runs using the .NET Framework and only works on .NET Framework projects. The MSBuild tasks generate a "shadow" project using the classic project structure, including an `app.config` file configured to the test framework you're using, and copies in all the `*.feature` files from your project. The `*.feature.cs` files create by the tool are then are added to the build so the unit tests appear in the compiled assembly.
