.PHONY: build clear restore test prepare

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

# Run all tests with coverage
test:
	@echo "Note: No test projects found in solution. Skipping test execution."
