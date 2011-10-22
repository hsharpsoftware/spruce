=========Getting started=========

- Create an App pool that runs with .NET 4
- Make sure the Site has authentication set to Windows and anonymous is disabled (can this be done in the web.config?)
- Configure the app pool to allow 32 bit applications

For making it a little easier to edit views in the MSAgile project, without having to copy the View folder
each time, run this command in a console inside the Site directory:

mklink /d Template ..\Spruce.Templates.MSAgile\

NOTE: Mercurial sees this as an actual directory, so delete the link before any commits.

=========Building=========
Publishing using Visual Studio will usually fail - you will need to copy the output of the bin folder AND the Template folder for
Spruce to work correctly.

After building, delete the follow files from your bin folder:

Microsoft.TeamFoundation.WorkItemTracking.Client.Cache.dll
Microsoft.TeamFoundation.WorkItemTracking.Client.DataStore.dll
Microsoft.TeamFoundation.WorkItemTracking.Client.dll
Microsoft.TeamFoundation.WorkItemTracking.Client.RuleEngine.dll

One (or more) of the TFS assemblies copies these files automatically, but they can cause issues on x64 machines.
You may also get problem with the references on the project using the GAC. If this is the case, then remove all 
Microsoft.TeamFoundation.* references and re-add them from the lib folder.

=========Publishing=========
The Visual Studio publish will not copy the correct 3rd party assemblies (or create the template folder). Copy all 3rd party
assemblies from the bin folder to the publish folder.

Create a template folder and copy template.css, web.config and the View folder from the Template project.
Make sure the App_Data folder is empty before zipping, and delete all XML files except the Spruce ones. from the bin folder.