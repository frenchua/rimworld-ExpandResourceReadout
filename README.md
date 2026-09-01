# About
A small mod to the video game RimWorld to remember the Resource readout (the list of resources in the upper left corner of the screen), so that it remembers if each entry is expanded or collapsed either on a
per-save or global basis (assuming the resource readout is set to sort itself by category).

It also adds a right-click context menu to the resource readout so that you can easily expand/contract a certain category (including all of it's subcategories), or expand/contract everything.

## Screenshots
<img width="244" height="170" alt="Screenshot 2026-09-01 005459" src="https://github.com/user-attachments/assets/ed46a271-a396-4132-b51f-cfab2d845b4e" />
<img width="920" height="186" alt="Screenshot 2026-09-01 005110" src="https://github.com/user-attachments/assets/3dff32f5-cc28-4ecb-b6c1-ccaf985ccdf4" />


# Installing the mod
1. Clone the repository into the `Mods` directory of your RimWorld install (on windows with Steam this is often `C:\Program Files (x86)\Steam\steamapps\common\RimWorld`)
2.  Run `dotnet build` from within this directory (or open the `.csproj` file in visual studio and build if from there) and you should be good to go.

# TODO
- [x] Make `.csproj` file work on all platforms
- [x] Add option to remember resource readout expansion state between game loads.
- [x] Add mod settings dialog.
- [ ] Upload to Steam workshop to allow for easy user installation
- [ ] Upload binary .zip releases on github so potential users don't have to build from source
- [ ] Add Github workflow to automatically publish to Steam workshop and create binary .zip distribution on each new release
- [ ] Figure out how to use Harmony to add tests (Harmony is apparently used at Microsoft to test WPF GUI apps, so maybe it can be leveraged here to test UI modifications? Not sure If I want to spend the time trying to figure this out).
