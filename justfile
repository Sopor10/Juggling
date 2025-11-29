# use PowerShell instead of sh:
set shell := ["pwsh", "-c"]

publish-mcp:
    dotnet publish Siteswaps.Mcp.Server/Siteswaps.Mcp.Server.csproj /t:PublishContainer
run-mcp:
    docker run -p 5005:8080 siteswaps-mcp-server 