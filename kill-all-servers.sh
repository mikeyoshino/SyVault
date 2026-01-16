#!/bin/bash
echo "🛑 Stopping all DigitalVault servers..."

# Kill API server
pkill -f "DigitalVault.API.dll" && echo "✓ Killed API server" || echo "  API server not running"

# Kill BFF server
pkill -f "DigitalVault.Web.dll" && echo "✓ Killed BFF server" || echo "  BFF server not running"

# Kill Blazor server
pkill -f "DigitalVault.BlazorApp.dll" && echo "✓ Killed Blazor server" || echo "  Blazor server not running"

# Kill any remaining dotnet processes running from this project
pkill -f "syDigitalVault" && echo "✓ Killed remaining processes" || echo "  No remaining processes"

echo "Done! All servers stopped."
