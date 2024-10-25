# Init
$SCRIPTPATH = $PSScriptRoot
$WINSDK_PATH = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x86"
$SIGNTOOL = $WINSDK_PATH + "\signtool.exe"

$EXE_PATH = $SCRIPTPATH + "\..\Setup\Release\WindowsVirtualDesktopHelper Setup.msi"

# See https://www.files.certum.eu/documents/manual_en/Code-Signing-signing-the-code-using-tools-like-Singtool-and-Jarsigner_v2.3.pdf
$SIGN_CMD = "& '" + $SIGNTOOL + "' sign /n 'Open Source Developer, Daniel Krüsi' /t http://time.certum.pl/ /fd sha256 /v '"+$EXE_PATH + "'"
echo $SIGN_CMD 
Invoke-Expression $SIGN_CMD