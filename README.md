# Dynamic Impostor Layering Study (COMP320/COMP360)
This is a simple shooting game utilising an impostor-based rendering system in the background to improve performance and gain an understanding of the player response to incongruencies as a result of flatenning sections of the 3D image.

That is my dissertation proposal in less than 1% of the words. Inflation is high in this market!

# Project Status
See below the testing requirements of the project. A **+** means the correct functionality occurs. A **-** means the feature or issue needs fixing or changing. A **?** means that the test has not been conducted, or is out of date and need retesting.

## Stage: Impostor Engine
### Requirements testing
* ? Does the impostor system visibly divide the scene into layers when viewed from an alternative angle?
* ? Are the layers correctly ordered with no overlaps/duplicates of objects?
* ? Do the layers adequately frame the objects within them?
* ? Does the impostor system advance when the player completes a level?
* ? Does the impostor system exhaustively advance to every state?

### Stress testing
* ? Does the impostor system perform above or at least on par with regular rendering?

## Stage: Game
### Requirements testing
* + Can the player shoot all balloons?
* + Can the player play all rounds?
* + Do the balloons spawn when the player enters the trigger area?
* + Does the player's slingshot shoot reliably?
* + Does the score count increase as the player shoots balloons?
* + Does the next round begin after the player completes a level?
* + Can the player complete the level?
* + Is the player stopped when the game ends?

### Stress testing
* + Do spawners reliably activate if the player attempts to jump around the invisible triggers?
* + Can the player climb walls?
* + Can the player escape the level?
* + Can the player break the slingshot functionality by spamming Shoot?
* + Does the slingshot reliably hit balloons at high speed?
Last test: 26/02/2020

## Stage: Data collection
### Requirements testing
* + Do the questions all appear in sequence after each round?
* + Does the balloon lifetime data match the player's gameplay performance?
* - Does the frame rate data match the game's performance?
* ? Is the data associated with the active impostor configuration correctly?
* + Does the questionnaire data match the user's responses?

### Stress testing
* + Does the log file continue to collate the correct data when the game is run two times or more?

### Unit tests
* + Do all unit tests pass?

See Scripts/Test_PlayMode/PlayModeTests source code for descriptively commented unit tests.