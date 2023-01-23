## Enable local HTTPS for .NET
dotnet dev-certs https --trust

dotnet workload install wasm-tools
## Restore .NET packages and build the default solution
dotnet restore && dotnet build


