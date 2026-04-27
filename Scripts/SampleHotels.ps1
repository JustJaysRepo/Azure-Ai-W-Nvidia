# Push-SampleHotels.ps1
# Pushes sample hotel documents to an Azure AI Search index via REST API.
# Run this after recreating the search service between sessions.
#
# Usage:
#   .\Push-SampleHotels.ps1 -Endpoint "https://yourservice.search.windows.net" -ApiKey "YOUR_ADMIN_KEY"

param (
    [Parameter(Mandatory=$true)]
    [string]$Endpoint,

    [Parameter(Mandatory=$true)]
    [string]$ApiKey,

    [string]$IndexName = "hotels-sample-index"
)

$body = @{
    value = @(
        @{ "@search.action" = "upload"; HotelId = "1"; HotelName = "Fancy Stay Hotel"; Description = "Luxury hotel with rooftop pool and spa facilities"; Category = "Luxury"; Rating = 5.0 },
        @{ "@search.action" = "upload"; HotelId = "2"; HotelName = "Secret Point Motel"; Description = "Affordable motel with outdoor pool and free breakfast"; Category = "Budget"; Rating = 4.6 },
        @{ "@search.action" = "upload"; HotelId = "3"; HotelName = "Wellness Retreat"; Description = "Boutique hotel specializing in spa packages and massage therapy"; Category = "Boutique"; Rating = 4.8 }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod `
        -Uri "$Endpoint/indexes/$IndexName/docs/index?api-version=2024-07-01" `
        -Method POST `
        -Headers @{ "api-key" = $ApiKey; "Content-Type" = "application/json" } `
        -Body $body

    Write-Host "Success — documents indexed to '$IndexName'" -ForegroundColor Green
    Write-Host "Verify in portal: Search Explorer -> Select index -> Search"
}
catch {
    Write-Host "Error pushing documents: $_" -ForegroundColor Red
    Write-Host "Check your endpoint URL and admin key are correct."
}
