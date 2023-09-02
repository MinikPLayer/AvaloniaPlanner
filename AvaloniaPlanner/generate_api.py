import os
import subprocess
import shutil
from colorama import Fore, Back, Style

if os.path.exists('AvAPI'):
    shutil.rmtree('AvAPI')
    
if os.path.exists('AvAPI.dll'):
    os.remove('AvAPI.dll')

# Generate API folder
print(Fore.GREEN + "[GENERATE] Generating openAPI documentation..." + Fore.WHITE)
os.system("openapi-generator-cli generate -g csharp-netcore --additional-properties=nullableReferenceTypes=true,packageName=AvAPI,targetFramework=net6.0;net7.0 -i http://localhost:5072/swagger/v1/swagger.json -o AvAPI")

# Build it with msbuild
print(Fore.GREEN + "[GENERATE] Building project with msbuild" + Fore.WHITE)
subprocess.call(["C:/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/amd64/msbuild.exe", "AvAPI/AvAPI.sln", "-t:restore,build", "-p:RestorePackagesConfig=true", "-p:Configuration=Release"])

# Copy target dll
print(Fore.GREEN + "[GENERATE] Copying target DLL..." + Fore.WHITE)
shutil.copy("AvAPI/src/AvAPI/bin/Release/net7.0/AvAPI.dll", "AvAPI.dll")

# Remove leftover folder
print(Fore.GREEN + "[GENERATE] Removing leftover folder..." + Fore.WHITE)
shutil.rmtree('AvAPI')