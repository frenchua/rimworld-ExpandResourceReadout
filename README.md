This is a small mod for RimWorld that automatically expands the resource readout (the list of resources in the upper left corner of the screen) when a save is loaded (assuming the resource readout is sorted by categories).

# Installing the mod
## MacOS
Clone the repository into the `Mods` directory inside `RimWorldMac.app`. Then enter this directory and run `dotnet build ./ExpandResourceReadoud.csproj`. If the project is not in the `Mods` directory it will not be able to find and link to the relevant RimWorld asssemblies.

## Windows or Linux
You will have to modify the `ExpandResourceReadoud.csproj` file to build this on Windows or Linux. Uncomment the relevant `<RimWorldPath>` tags inside of it. It must be inside the RimWorld `Mods` folder to build.

# TODO
- [ ] Make `.csproj` file work on all platforms
- [ ] Add option to remember resource readout expansion state between game loads.
- [ ] Add mod settings dialog.
