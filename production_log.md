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

# 29/01/2020
## Overview
In an Agile fashion, it was felt that the features most important to the project would be the game as this is what the end user would experience. This provides the necessary context for refining the impostor system, which was taking a while to develop due to a lack focus.

We found and downloaded a scene for the game yesterday to begin this context. To get the game up and running, the character movement and slingshot ability were implemented, but collision wasn't finished.

We initially tried to create acceleration and friction curves for character movement. This required an InverseEvaluate function for curves. Unfortunately, after much time and unit testing (UnitTests.cs) the reliability of these functions proved too low. Furthermore, the behaviour of an acceleration curve when accelerating in the opposite direction was quite undefined in concept. Eventually, we didn't achieve properly-functioning curve-based movement and decided to return to a standard linear acceleration/friction model, noting the time investment to returns ratio.

## Next steps
The next priority is to fix the collision and add the balloon enemies. Once the game is playable, we still need to add the end states and questionnaires.

The impostor system will take massive amounts of refinement to get right, and we feel it would be best to commit this early time period to the game and surveys as we are able to finish those in a comparatively short time span.

# 04/02/2020
## Overview
A test of the game in the main level shows that the collision system is functional enough to continue to the next stage of the game development. We can reliably move through the environment and while in special cases we will fall through the floor, we are in a good position to iterate on the core game. However, these bugs are highly problematic - sometimes falling through the floor - and once the game is done some of the remaining time will be committed to fixing these.

## Today
We created the balloon enemies, spawners, spawn triggers and basic editor tools to bring this all together.

The UI was also implemented with minimal issues. There is now a timer and balloon tally.

We added the data file recording functionality today. To ensure the data formatting was functioning properly, we iterated with a unit test in UnitTests.cs. We checked the file data manually as well to confirm it did indeed contain comma-separated values.

Surveying functionality was also added, but needs some tweaks to be ready.

## Next steps
Finish surveying and round sequence. Update the shooting to be accurate to the animation.

# 05/02/2020
## Overview
A presentation of the game shows it to be in a buggy state. This is partly because the survey system was under testing in the last iteration.

- The balloons didn't spawn
- The survey showed immedately (for testing)
- The impostor system was not implemented.

The impostor system is being put on hold while we address the requirements of the game. Once the game has been outlined, it would then be sensible to continue the impostor system.

## Next steps
Finish the survey system, making sure it restarts the game at the next impostor configuration. Keep the game time limit customisable, so we can test the survey system by letting the game end immediately after beginning. Ensure the balloon spawn correctly and the end of the level functions.

# 11/02/2020
## Current stage: Game (and a bit of data collection for unit test refactoring)
## Overview
This week's session brought our attention to testing and bugtracking. As the game grows, it has become necessary to test more elements than were initially expected. We have written a specific requirements and stress test plan in test_plan.md to address this.

Unity has its own unit test runner, which we'll be using here onwards for our unit tests. We migrated the DataFile test code along the way (result: looking good!).

Other than that, the focus remains on the gameplay. We still have some requirements that are not quite satisfied; for example, being able to reliably shoot balloons. We aim to complete the requirements today.

## Next steps
- Clarify the test plan
- Complete core game loop MVP
- Conclude the day with a requirements test as specified in the Game stage of the test plan

# 12/02/2020
Yesterday's requirements test found everything working except the player was not stopped when the game ended. However, this is not a priority for next week's playtest.

The core game loop is there and functioning fairly well. Provisionary stress testing shows issues with the player clipping through objects. However, the biggest missing piece is the lack of a finished level. Finishing the level has become the next largest priority that should be absolutely achievable today. After that, it will be useful to return to the data collection phase so we can complete that as fast as possible, before using the remainder of development time on stress testing and impostor system refinement.

## Next steps
- Complete the level

# 18/02/2020
A requirements test of the previously completed version found the game to be complete with only the following areas missing:

* Is the player stopped when the game ends?

Following the correction of this small detail, the Game stage is complete asides from the minor bugs recorded in the GitHub Issue Tracker.

The Data collection stage needs to be fully requirements-tested. A provisional test showed that the data wasn't being recorded. The unit tests were still working. Thankfully, this issue was found to be the result of a minor oversight where the file path was missing. The recorded data now works, but a requirements test would do good to assure that the recorded data is valid.

Following this requirements test, the impostor system will be applied to multiple map objects. We will then run an ad-hoc stress test, playing the game with the impostor system in various random configurations to identify the most prominently visible flaws in the current iteration. The notes and bugs recorded from this test will form the groundwork for the Impostor Stage's next iteration. The expectation is that the systsem may be slow and inaccurate in a large scale, but this requires deeper observation to ascertain.

## Next steps
* Fix player being able to move after the game
* Verify data collection system
* Stress-test the impostor system