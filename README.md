# SpaceRunner

This tool enables the compilation of a C# program that will execute arbitrary PowerShell code, without launching PowerShell processes through the use of runspace.

AMSI is patched using [@\_xpn_](https://twitter.com/_xpn_/status/1170852932650262530) and [@_RastaMouse](https://github.com/rasta-mouse/AmsiScanBufferBypass) technique.

## Disclaimer

This project can only be used for authorized testing or educational purposes. Using this software against target systems without prior permission is illegal, and any damages from misuse of this software will not be the responsibility of the author.

## Compilation

To compile the binary, just type the following command in the repo folder:
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /unsafe /platform:x64 /preferreduilang:en-US /filealign:512 /out:spacerunner.exe /target:exe spacerunner.cs
```

## Usage

Execute the binary with the following parameters:
```
-i (--input)         Full path to the PowerShell input script file
-o (--output)        Full path to the generated output binary file
-h (--hide)          Optional, set the specified window's show state -> Default = False
-b (--beacon)        Optional, type of script provided (Cobalt Strike beacon) -> Default = PowerShell script
-f (--functions)     Optional, set PowerShell function to call (function1,function2,...)
-a (--arguments)     Optional, set PowerShell function arguments to pass ("function1Arg1 function1Arg2 ...#function2Arg1 function2Arg2 ...")
```

Examples:  
- Compile a [Cobalt Strike](https://www.cobaltstrike.com/) beacon binary:
```
spacerunner.exe -i bin\beacon.ps1 -o bin\beacon.exe -b -h
```
- Compile [Inveigh](https://github.com/Kevin-Robertson/Inveigh) binary with `Invoke-Inveigh` function and `'-ConsoleOutput Y'` arguments parameters:
```
spacerunner.exe -i bin\Inveigh.ps1 -o bin\inveigh.exe -f Invoke-Inveigh -a "-ConsoleOutput Y"
```
- Compile [Sherlock](https://github.com/rasta-mouse/Sherlock) binary with `Find-AllVulns` function parameter:
```
spacerunner.exe -i bin\Sherlock.ps1 -o bin\sherlock.exe -f Find-AllVulns
```
- Compile [PowerUp](https://github.com/PowerShellMafia/PowerSploit/blob/dev/Privesc/PowerUp.ps1) binary with `Find-PathDLLHijack` function parameter:
```
spacerunner.exe -i bin\Powerup.ps1 -o bin\Powerup.exe -f Find-PathDLLHijack
```
- Compile [PowerView](https://github.com/PowerShellMafia/PowerSploit/blob/dev/Recon/PowerView.ps1) binary with `Get-DomainGroupMember` function and `-Identity 'Admins du domaine' -Recurse` parameters:
```
spacerunner.exe -i bin\Powerview.ps1 -o bin\Powerview.exe -f Get-DomainGroupMember -a "-Identity 'Admins du domaine' -Recurse"
```
- Compile [PowerView](https://github.com/PowerShellMafia/PowerSploit/blob/dev/Recon/PowerView.ps1) binary with `Get-DomainGroupMember` and `Get-DomainUser` functions with respective arguments `-Identity 'Admins du domaine' -Recurse` and `user`:
```
spacerunner.exe -i bin\Powerview.ps1 -o bin\Powerview.exe -f Get-DomainGroupMember,Get-DomainUser -a "-Identity 'Admins du domaine' -Recurse#'user'"
```

## Credits

This project is based on the work done by :

- Adam Chester, @\_xpn_
- Casey Smith, @subTee
- Lee Christensen, @tifkin_
- Matt Graeber, @mattifestation
- Rasta Mouse, @_RastaMouse
