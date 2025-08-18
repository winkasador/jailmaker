# Content Management

# Dynamic Content Management
It's possible for Scenes to declare the specific content they want to use in Content Groups.

A Scene can define multiple content groups, for example:
There are objects in the editor that always need to be loaded, the editor lists these under 'scene/editor/content/universal'.
<br>There are also some objects in the editor which are only needed for certain maps, like the christmas ones. These would be listed under 'scene/editor/content/ss/'.

When loading, the editor will request 'universal' because it will always be needed, but it will not load 'ss' unless one of the christmas maps are loaded. Jailbreak will dynamically unload 'ss' when a christmas map is unloaded to save memory usage.

There might be cases where multiple groups have the same objects. 

For instance, 'ss' and 'ccl' both have the sleigh object, but they also differ with certain objects. When swapping between 'ss' and 'ccl', the editor will request the Jailbreak to Compare & Load, meaning Jailbreak will unload everything from 'ccl' unless it is also in 'ss', and will then load the remaining missing objects from 'ss'.

Certain bits of content are linked together - object definitions will specify the textures they want, and Jailbreak will automatically load these when loading the object definition, so you don't need to specify everything within the content list.