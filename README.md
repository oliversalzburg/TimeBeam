# TimeBeam

> [!CAUTION]
> This is still a pretty solid, and clear implementation of the principles of a timeline control. A lot of things are custom implementations that explain the _ideas_ of a timeline control to a certain degree.
> While it was fun to work on it at the time, I could no longer accept changes to this repository today. Thus, it has been **archived as of 2024-07-29**. I hope the archived code can still be useful somehow :smile:

A timeline control for .NET.

![Updated Look and Feel](https://i.imgur.com/kAzaej1.png)


## Input
 <kbd>Shift</kbd> | <kbd>Ctrl</kbd> | <kbd>Alt</kbd> | Button/Key     | Action
:----------------:|:---------------:|:--------------:|----------------|----------------------
|                 |                 |                | **MouseWheel** | Scroll vertically
|                 | <kbd>Ctrl</kbd> |                | **MouseWheel** | Scroll horizontally
|                 |                 | <kbd>Alt</kbd> | **MouseWheel** | Zoom vertically
|                 | <kbd>Ctrl</kbd> | <kbd>Alt</kbd> | **MouseWheel** | Zoom horizontally
|                 |                 |                | **Middle**     | Pan
|                 | <kbd>Ctrl</kbd> |                | <kbd>A</kbd>   | Select all
|                 | <kbd>Ctrl</kbd> |                | <kbd>D</kbd>   | Deselect all

### Selection
By default, anything that *intersects* with the selection rectangle is selected.  
If you hold <kbd>Alt</kbd>, anything that is *contained* within the selection rectangle is selected.  
Holding <kbd>Ctrl</kbd> allows you pick single tracks in and out of the selection.  
Clicking the labels at the start of a track will select all track elements on that track.

### Resizing / Moving
When grabbing a track item near the left or right edge and dragging that edge, the start or end of that track item is moved respectively.  
When grabbing the track at any other place, the whole track item is moved.  
Both resizing and moving operations always snap to full units, unless <kbd>Alt</kbd> is being held.

---

### Older screenshots
![Added multi-part tracks](https://i.imgur.com/sxSXtJp.png)

![Zooming enabled](https://i.imgur.com/mK9GXug.png)

![Added a playhead](https://i.imgur.com/MvPK02C.png)

![Added track labels](https://i.imgur.com/QKG6M3V.png)

![Old version](https://i.imgur.com/c2c1C38.png)
