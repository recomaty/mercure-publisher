.PHONY: build clear restore test prepare coverage

# Install required tools
prepare:
	dotnet tool install -g dotnet-reportgenerator-globaltool

# Build the solution
build:
	dotnet build R3Polska.Sse.Mercure.sln

# Clean build artifacts
clear:
	dotnet clean R3Polska.Sse.Mercure.sln
	rm -rf R3Polska.Sse.Mercure/bin R3Polska.Sse.Mercure/obj
	rm -rf coverage

# Restore NuGet packages
restore:
	dotnet restore R3Polska.Sse.Mercure.sln

# Run all tests
test:
	dotnet test R3Polska.Sse.Mercure.Tests/R3Polska.Sse.Mercure.Tests.csproj

# Run tests with coverage and generate HTML report
coverage:
	rm -rf R3Polska.Sse.Mercure.Tests/TestResults
	dotnet test R3Polska.Sse.Mercure.Tests/R3Polska.Sse.Mercure.Tests.csproj --collect:"XPlat Code Coverage" --settings coverlet.runsettings
	dotnet tool restore || dotnet tool install -g dotnet-reportgenerator-globaltool
	reportgenerator -reports:"./R3Polska.Sse.Mercure.Tests/TestResults/**/coverage.cobertura.xml" -targetdir:"./coveragereport" -reporttypes:Html
	@echo "Coverage report generated at coveragereport/index.html"