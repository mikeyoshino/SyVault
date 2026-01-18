#!/bin/bash
echo "🛑 Stopping all DigitalVault servers..."

# Kill API server
pkill -f "DigitalVault.API.dll" && echo "✓ Killed API server" || echo "  API server not running"

# Kill Gateway server
pkill -f "DigitalVault.Gateway.dll" && echo "✓ Killed Gateway server" || echo "  Gateway server not running"

# Kill Client server
pkill -f "DigitalVault.Client.dll" && echo "✓ Killed Client server" || echo "  Client server not running"

# Kill any remaining dotnet processes running from this project
pkill -f "syDigitalVault" && echo "✓ Killed remaining processes" || echo "  No remaining processes"

echo "Done! All servers stopped."
