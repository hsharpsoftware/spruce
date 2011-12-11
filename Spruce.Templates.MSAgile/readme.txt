[Building]
Copy the template.css file and the Views folder to the Spruce.Site/Template folder, the binaries will automatically be copied.
Everything else can be ignored.

[Notes]
This MS Agile Version 4/5 plugin only supports Bugs, Tasks and Issues. 

Not currently supported:
* Shared Steps are not support, as they require extra work to enable adding/editing/deleting the steps you can have for a work item.
* Test Cases are similar to Shared Steps and have the same list of steps.
* User Storeys are similar to tasks, but include story point risk fields, as well as extra implementation field that is a set of links.

The plugin should be fairly compatible with Bugs and Tasks in CMMI projects.

This plugin currently has a lot of duplicated code, which in future should probably be refactored into more streamlined and generic model. Send a patch 
if you want to improve it :)