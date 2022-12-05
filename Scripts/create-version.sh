#!/bin/bash

# Get Version (by parsing AssemblyInfo.cs)
# Read source into variable
{ IFS= read -rd '' ASSEMBLY_INFO <../Source/AssemblyInfo.cs;} 2>/dev/null
VERSION_REGEX='AssemblyVersion\("([0-9]*\.[0-9]*)'
[[ $ASSEMBLY_INFO =~ $VERSION_REGEX ]]
VERSION=${BASH_REMATCH[1]}
#VERSION="${VERSION%"${VERSION##*[![:space:]]}"}"  
echo "Detected Version: $VERSION"
 
# Build
MSBuild ../WindowsVirtualDesktopHelper.sln /t:Rebuild /p:Configuration=Release