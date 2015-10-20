### Build Status:
| Linux / OSX | Windows |
| ----- | ------- |
| [![Build Status](https://travis-ci.org/SpagAachen/Ballz.svg?branch=master)](https://travis-ci.org/SpagAachen/Ballz) | [![Build status](https://ci.appveyor.com/api/projects/status/exmgex28ay9v20k8/branch/master?svg=true)](https://ci.appveyor.com/project/LukasBoersma/ballz/branch/master) |

#### Prerequisits:
You need to have the "Monogame Content Pipeline" installed as well as an IDE that supports MonoGame.

On Windows you may use Visualstudio or MonoDevelop both with the MonoGame Addin installed. 

On Linux you should use MonoDevelop with the MonoGame Addin in combination with Mono-4.0 or higher.


#### Compiling the Project:
The Project consists of one Solution called "Ballz.sln" and several projects in this solution

("Ballz.Content", "Ballz.Core", "Ballz.Windows", "Ballz.Linux")

In the Ballz.Content project you will find a file called Content.mgcb. Before you build the Ballz solution you have to open the Content.mgcb file with the MonoGame Content Pipeline and click build in the Content Pipeline tool.
This should create .xnb files for the ressources in the Content project.

After you generated the .xnb files you can close the mgc project and select a build configuration in your IDE that corresponds to your Plattform and build type (Windows should work with the "mixedPlattforms" profile while on linux you need to use the "linux" profile). After that you should be able to build and run the project as you are used to. 

note: You cannot build the Ballz.windows solution on a linux system as there would be conflicts in the references. therefore the "linux" profile only builds the Ballz.linux project and the content project.

the content project in the IDE is only used to copy generated .xnb files to the output directory of the other projects. You do not need to copy an raw ressources like jpg or ttf files manually as you only need the .xnb files which are copied automatically by the ballz.content project.

As the plattform dependant projects Ballz.linux and Ballz.windows are not built on all plattforms, you should incorporate common gamecode in the Ballz.Core project.
