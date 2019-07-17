FROM mcr.microsoft.com/windows/servercore:1809
EXPOSE 80
EXPOSE 808
EXPOSE 8000
EXPOSE 8001
WORKDIR /app
COPY bin/debug .
COPY localhost.pfx .
RUN netsh http add urlacl url=https://+:8001/WCFX/WCFX.Common.IDossierService/ user="NT AUTHORITY\NETWORK SERVICE"
USER "NT AUTHORITY\NETWORK SERVICE"
CMD WCFX.Server.exe
