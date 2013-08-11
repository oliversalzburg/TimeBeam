# TimeBeam

A timeline control for .NET (which is still under development).

![Updated Look and Feel](https://i.imgur.com/kAzaej1.png)


## Input
 <kbd>Shift</kbd> | <kbd>Ctrl</kbd> | <kbd>Alt</kbd> | Button/Key     | Action
:----------------:|:---------------:|:--------------:|----------------|----------------------
                  |                 |                | **MouseWheel** | Scroll vertically
                  | <kbd>Ctrl</kbd> |                | **MouseWheel** | Scroll horizontally
                  |                 | <kbd>Alt</kbd> | **MouseWheel** | Zoom vertically
                  | <kbd>Ctrl</kbd> | <kbd>Alt</kbd> | **MouseWheel** | Zoom horizontally
                  |                 |                | **Middle**     | Pan
                  | <kbd>Ctrl</kbd> |                | <kbd>A</kbd>   | Select all
                  | <kbd>Ctrl</kbd> |                | <kbd>D</kbd>   | Deselect all

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