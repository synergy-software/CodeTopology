# CodeTopology
A tool to visualise your codebase.

Currently there is a support only for svn repository.

##How to use it
To generate CodeTopology report run the following script from powershell console

```powershell
.\GenerateReport.ps1 -Verbose -CheckoutDir path_to_your_repository_checkout_dir
```

###Prerequisites
1. CodeTopology is using svn.exe to generate svn log. It is a part of TortoiseSVN so you have to install TortoiseSVN with selected "command line client tools" option.

2. Script file dowloaded from the internet may be locked. Before running script remember to unlock file in file's options.


##TeamCity integration
To get know how to integrate CodeTopology with TeamCity please visit project's Wiki.
