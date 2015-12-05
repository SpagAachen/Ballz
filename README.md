### Build Status:
| Linux / OSX | Windows |
| ----- | ------- |
| [![Build Status](https://travis-ci.org/SpagAachen/Ballz.svg?branch=master)](https://travis-ci.org/SpagAachen/Ballz) | [![Build status](https://ci.appveyor.com/api/projects/status/exmgex28ay9v20k8/branch/master?svg=true)](https://ci.appveyor.com/project/LukasBoersma/ballz/branch/master) |

#### Prerequisits:
You need to have the "Monogame Content Pipeline" installed as well as an IDE that supports MonoGame.

On Windows you ~~have to~~ can use Visualstudio 2015 or Xamarinstudio, both with the MonoGame Addin installed. Since we are using C# 6.0 older versions of Vistualstudio are not supported. ~~Also Monodevelop is currently no option on windows as there is a bug with MD and MSBuild when using the mono runtime (which is required since the default Microsoft.Net runtime does not support C# 6.0)~~ If you want to use Xamarinstudio, you have to install msbuild tools 2015 and use "Windows" as platform configuration.

MSBuild Tools 2015:
https://www.microsoft.com/en-us/download/details.aspx?id=48159

On Linux you should use MonoDevelop with the MonoGame Addin in combination with Mono-4.0 or higher.


#### Compiling the Project:
The Project consists of one Solution called "Ballz.sln" and several projects in this solution

("Ballz.Core", "Ballz.Windows", "Ballz.Linux", "Ballz.Test")

The actual programming takes place in Ballz.Core. The Platform projects are used only for configuration purposes, as the Core project will automatically import the corresponding platform project to inherit the necessary references. Therefore building the platform projects will do nothing, you should build and run the Cor or Test projects.

Content still has to be precompiled manually via monogame content pipeline. we are currently working on the integration of mgcb and mg2fx in the project so you will hopefully soon be able to just hit the compile button.
For now the essential content is just copied raw to the output directory as a temporal workaround.

Everything you do should be done in the Core project so the configuration for other platforms is not compromised. Therefore you should add references only to the Core project and use preferably platform independant assemblies.
