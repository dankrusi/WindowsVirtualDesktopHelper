# Init
$SCRIPTPATH = $PSScriptRoot
$VISUALSTUDIO_PATH = "C:\Program Files\Microsoft Visual Studio\2022\Community"
$MSBUILD = $VISUALSTUDIO_PATH + "\MSBuild\Current\Bin\msbuild.exe"
$DEVENV = $VISUALSTUDIO_PATH + "\Common7\IDE\devenv.com"


# Get Version (by parsing AssemblyInfo.cs)
# Read source into variable
$VERSION_REGEX = 'AssemblyVersion\("([0-9]*\.[0-9]*)'
select-string -Path ..\Source\AssemblyInfo.cs -Pattern $VERSION_REGEX -AllMatches | % { $_.Matches } | % { $_.Value } -outvariable VERSION
$VERSION = $VERSION.replace('AssemblyVersion("','')
$RELEASE_FOLDER = "G:\My Drive\Work\WindowsVirtualDesktopHelper\Releases\" + $VERSION


# Confirm
echo "Version: $VERSION"
$null = Read-Host "Press enter to continue..."

# Create releases folder
echo "================================================================"
echo "= Creating release folder..."
echo "================================================================"
If (!(test-path $RELEASE_FOLDER))
{
    md $RELEASE_FOLDER
}

# DisableOutOfProcBuild (required for Visual Studio Setup Proj)
echo "================================================================"
echo "= DisableOutOfProcBuild (required for Visual Studio Setup Proj)"
echo "================================================================"
$DISABLEOUTOFPROCBUILD_CMD = "& '" + $VISUALSTUDIO_PATH + "\Common7\IDE\CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild\DisableOutOfProcBuild.exe" + "'"
cd $VISUALSTUDIO_PATH
Invoke-Expression $DISABLEOUTOFPROCBUILD_CMD
cd $SCRIPTPATH


# Build Solution
echo "================================================================"
echo "= Building Solution..."
echo "================================================================"
$BUILD_EXE_CMD = "& '" + $MSBUILD + "' '..\WindowsVirtualDesktopHelper.sln' /t:Rebuild /p:Configuration=Release"
$BUILD_SETUP_CMD = "& '" + $DEVENV + "' '..\Setup\Setup.vdproj' /build Release"
$BUILD_SOLUTION_CMD = "& '" + $DEVENV + "' '..\WindowsVirtualDesktopHelper.sln' /rebuild Release"
Invoke-Expression $BUILD_SOLUTION_CMD


# Create Executable ZIP
echo "================================================================"
echo "= Creating Executable ZIP in releases..."
echo "================================================================"
$EXE_PATH = "..\Source\bin\Release\WindowsVirtualDesktopHelper.exe"
$CONFIG_PATH = "..\Source\bin\Release\WindowsVirtualDesktopHelper.exe.config"
$LICENSE_PATH = "..\LICENSE.md"
$README_PATH = "..\README.md"
$CHANGELOG_PATH = "..\CHANGELOG.md"
$EXE_DEST_PATH = $RELEASE_FOLDER + "\WindowsVirtualDesktopHelper Executable v"+$VERSION+".zip"
Compress-Archive -Force -Path $EXE_PATH, $CONFIG_PATH, $LICENSE_PATH, $README_PATH, $CHANGELOG_PATH -DestinationPath $EXE_DEST_PATH
$EXE_DEST_PATH_HASH = (Get-FileHash -Algorithm SHA256 -Path $EXE_DEST_PATH).Hash
$EXE_DEST_PATH_HASH | Set-Content -NoNewline -Force -Path ($EXE_DEST_PATH + ".sha256")
$EXE_DEST_URL = "https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/download/v"+$VERSION+"/WindowsVirtualDesktopHelper.Executable.v"+$VERSION+".zip"


# Copy Setup
echo "================================================================"
echo "= Copying setup to releases..."
echo "================================================================"
$SETUP_PATH = "..\Setup\Release\WindowsVirtualDesktopHelper Setup.msi"
$SETUP_DEST_PATH_RELEASES = $RELEASE_FOLDER + "\WindowsVirtualDesktopHelper Setup v"+$VERSION+".msi"
Copy-Item -Force -Path $SETUP_PATH -Destination $SETUP_DEST_PATH_RELEASES
$SETUP_DEST_PATH_RELEASES_HASH = (Get-FileHash -Algorithm SHA256 -Path $SETUP_DEST_PATH_RELEASES).Hash
$SETUP_DEST_PATH_RELEASES_HASH | Set-Content -NoNewline -Force -Path ($SETUP_DEST_PATH_RELEASES + ".sha256")
#$SETUP_DEST_PATH_WINDOWSSTORE = "..\Releases\WindowsStore\WindowsVirtualDesktopHelper.Setup.msi"
#Copy-Item -Force -Path $SETUP_PATH -Destination $SETUP_DEST_PATH_WINDOWSSTORE

# Scoop
echo "================================================================"
echo "= Creating Scoop Manifest "
echo "================================================================"
$SCOOP_MANIFEST_TEMPLATE = "..\Installers\Scoop\windows-virtualdesktop-helper.json.template"
$SCOOP_MANIFEST_DEST = "..\Installers\Scoop\windows-virtualdesktop-helper.json"
$SCOOP_MANIFEST = Get-Content -Path $SCOOP_MANIFEST_TEMPLATE
$SCOOP_MANIFEST = $SCOOP_MANIFEST -replace '{version}', $VERSION -replace '{zip-url}', $EXE_DEST_URL -replace '{zip-hash}', $EXE_DEST_PATH_HASH
$SCOOP_MANIFEST | Set-Content -Force -Path $SCOOP_MANIFEST_DEST

echo "================================================================"
echo "= Done!"
echo "================================================================"


$null = Read-Host "Press enter to open release folder and create a release at GitHub..."
Start-Process $RELEASE_FOLDER
Start-Process "https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/new"