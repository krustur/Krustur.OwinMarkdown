FROM microsoft/aspnetcore:2.0
ARG source
WORKDIR /app
EXPOSE 80
#COPY ${source:-obj/Docker/publish} .
COPY bin/release/netcoreapp2.0/publish/ .
ENTRYPOINT ["dotnet", "Krustur.OwinMarkdown.Host.dll"]
