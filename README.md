# Aerolens Practice Form Automated Tests

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (required by Playwright)
- NuGet packages:
  - Microsoft.Playwright
  - NUnit
  - NUnit3TestAdapter
  - Bogus

## Setup

1. **Restore NuGet Packages**

2. **Install Playwright Browsers**

3. **Add Test Image**
- Place a file named `test-image.png` in the project root directory.

## Running the Tests

You can run the tests using the .NET CLI or from within Visual Studio's Test Explorer.

## Test Details

- Tests run on both Chromium and Firefox browsers.
- Test data is generated dynamically using the Bogus library.
- The tests validate form submission and confirmation modal content.

## Troubleshooting

- If browsers are not found, ensure you have run the Playwright install step.
- If `test-image.png` is missing, add it to the project root.