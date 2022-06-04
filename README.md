# MMR-Tracker
An Item, Location and Entrance Tracker for the Majoras Mask Randomizer

## Basic Functionality:
You will supply this tracker the logic file used to generate your seed and it will show you every location that is available to check based on the logic and items you have obtained.
Double clicking a location will ask you to select what item was found there, or you can import your Text spoiler log to have this done automatically.
If you know what item a location contains, you can set the location to display what item is located there without actually checking it.
All checked locations will be displayed in a separate box displaying the location and item that was found there.

## Item Settings:
You can customize what locations are Randomized, unrandomized or junk. Unrandomized items will be automatically obtained when they are available and junk locations will be hidden from the tracker.
You can set items as starting items to have the tracker always consider them obtained.
These settings can be imported directly from the Majoras Mask Randomizer using either the settings.json or custom item strings.

## Entrance Randomizer Features:
When a version of the randomizer that includes entrance rando is used, the tracker will have a seperate box to keep track of available entrances.
When checking an entrance, the tracker will automatically mark the entrance in reverse assuming you have the option enabled.
You can use the pathfinder to show you available paths from one entrance to another.

## Extra Features: 
The tracker conatins a number of not exactly tracker related tools for use with the randomzier.

#### Seed Checker
You can use the Seed Checker to check if your seed can obtain a number of selected items based on your logic.
You can tell the seed checker to ignore certain checks you want to avoid.

#### Playthrough Generator
You can use the playthrough generator to create a guide that will tell you what items you need to obtain, where they are, and in what order you need to obtain them to beat the game. You can define what constitutes "Beating the game" by adding a fake item to your logic called MMRTGameClear and setting it to require what is neccesary to beat the game.
