# use PowerShell instead of sh:
set shell := ["pwsh", "-c"]

change-context-to-diskstation:
    docker context use Diskstation
publish-mcp:
    dotnet publish Siteswaps.Mcp.Server/Siteswaps.Mcp.Server.csproj /t:PublishContainer
stop-mcp:
    $containers = @(docker ps --filter "publish=5005" -q) | Select-Object -Unique; foreach ($c in $containers) { docker stop $c; docker rm $c }
run-mcp:
    docker run -d --name siteswaps-mcp-server --restart unless-stopped -p 5005:8080 siteswaps-mcp-server 
    
deploy-mcp-to-diskstation: change-context-to-diskstation publish-mcp stop-mcp run-mcp

format:
    dotnet csharpier format .