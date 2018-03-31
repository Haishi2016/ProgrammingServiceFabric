@powershell -NoProfile -ExecutionPolicy Bypass -Command "iex ((new-object net.
webclient).DownloadString(‘https://chocolatey.org/install.ps1’))" && SET
PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin
cinst nodejs.install --force -y
SET PATH=%PATH%;C:\Program Files\nodejs
npm install