# Crossy-Road-Hack-Code-Cave
Quick external Crossy Road trainer I made that modifies the function used to add points to your score and forces it to add 9999 to your score every time you move up. Includes a memory driver that has signature scanning and a method to fetch the base address for games that get handled by dll's (ie. Crossy Road.exe is the main exe, but everything gets handled by Crossy Road.dll).

# USAGE
Compile in x86 Release mode, run it and make sure Crossy Road is open. Input 1 to activate and 2 to deactivate, it runs off sig scans and the method I use is sometimes a bit iffy so if you get an error just wait for the main message to appear and try again, it should work.
