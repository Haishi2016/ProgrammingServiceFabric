@powershell -ExecutionPolicy Bypass -Command "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))" && SET PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin
@powershell -ExecutionPolicy Bypass -Command "choco install nodejs --force --yes --acceptlicense --verbose --allow-empty-checksums"  && SET PATH=%PATH%;"C:\Program Files\nodejs"
refreshEnv
npm install
EXIT 0
