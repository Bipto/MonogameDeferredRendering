# MonogameDeferredRendering
A simple deferred renderer built using MonoGame. Models used were from Mixamo and the code in this repository was based upon the tutorials available at Catalin Zima's deferred rendering tutorial, RBWhitaker's shader tutorials and Riemer's XNA 3D tutorials.

![screenshot](https://user-images.githubusercontent.com/63370393/133943864-f8ae6ae7-54fc-4504-84b5-ab54e8f1042b.png)

# Features
- Model and texture loading
- Diffuse, specular and ambient lighting using the Blinn-Phong model
- Skyboxes
- Direction, point and spot lights applied efficiently only to relevant area of framebuffer and with no hardcoded counts
- 3D depth reconstruction for lighting calculations using a depth buffer

# Links

Catalin Zima deferred rendering : (link no longer available)

RBWhitaker tutorials : http://rbwhitaker.wikidot.com/

Riemer's tutorials (ported to MonoGame) : https://github.com/simondarksidej/XNAGameStudio/wiki/RiemersArchiveOverview

Please note that this repository was created as a learning exercise and does not show the best way to create a deferred renderer.
