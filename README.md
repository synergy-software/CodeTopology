# CodeTopology
A tool to visualise your codebase.

![TortoiseSVN install](https://github.com/synergy-software/CodeTopology/blob/master/doc/sample01.jpg?raw=true)

Supported repositories:
* Git
* SVN

Supported presentation modes:
- Code ownership
- Programming languages
- Commits count

##How to use it
###Prerequisites
1. CodeTopology is using svn.exe to generate svn log. It is a part of TortoiseSVN so you have to install TortoiseSVN with selected "command line client tools" option. This is required only for SVN repositories.

2. Script file dowloaded from the internet may be blocked. Before running script remember to unblock GenerateReport.ps1 file. In order to unblock script go to file options, find "Security" section on "General" tab and select "Unblock" option.

###Generate report
To generate CodeTopology report run the following script from powershell console

- For Git repository
```powershell
.\GenerateReport.ps1 -Verbose -VCS Git -CheckoutDir path_to_your_repository_checkout_dir
```
- For SVN repository
```powershell
.\GenerateReport.ps1 -Verbose -VCS SVN -CheckoutDir path_to_your_repository_checkout_dir
```

By default CodeTopology ignores the following directories: *"Bin","Obj","Lib","bin","obj","packages","lib","App_Data","node_modules", "bower_components"*. 

If you want to extend this list use **DirToExclude** parameter as follows:

- To ignore single *dist* directory
```powershell
.\GenerateReport.ps1 -VCS Git -DirToExclude dist  -CheckoutDir path_to_your_repository_checkout_dir
```
- To ignore *dist1* and *dist2* directories
```powershell
.\GenerateReport.ps1 -VCS Git -DirToExclude dist1, dist2  -CheckoutDir path_to_your_repository_checkout_dir
```


##TeamCity integration
To get know how to integrate CodeTopology with TeamCity please visit project's Wiki.
