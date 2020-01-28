# 28/01/2020
## Overview
Testing today, the impostor system appears to be working mostly as planned in the mushroom environment. They update at the given rate and at the right resolution. However, there are a couple of caveats.

## Issues
* Depth issues, particularly when looking from above, cause several of the objects to vanish as the plane crosses the ground.
* The frame rate seems to unavoidably spike during impostor re-renders. It would be useful to test this in a complete environment.
* The height of the impostor does not appear to be accurate and expands to extremely large sizes while the camera is pointed up or down.

Furthermore, the impostor system needs to surround the player, but it currently only updates at the front.

## Next steps
The top priority is to find a scene to test the system in. A realistic scenario would be a highly detailed scene that threatens the frame rate.

The next priority depends on which of the above issues is most visually noticeable. We'll need to fix them in that order. Some of the bugs might prove too difficult in which case it may be worth continuing with the actual game's development to avoid stalls.