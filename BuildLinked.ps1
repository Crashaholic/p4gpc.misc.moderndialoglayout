# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/p4gpc.misc.moderndialoglayout/*" -Force -Recurse
dotnet publish "./p4gpc.misc.moderndialoglayout.csproj" -c Release -o "D:/Modding/Persona 4 Golden/P4G Mods/.Reloaded II/Mods/p4gpc.misc.moderndialoglayout" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location
