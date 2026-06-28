# nBMSC

nBMSC is an improved version of µBMSC designed to balance convenience and simplicity.
It supports Base62 definitions.

See README.md.old for the original µBMSC README.
See README.md.old2 for the README from iBMSC, the earlier upstream project.

# Changes

- Base62 definition support
- Layout adjustments
  - Rebuilt the formerly complex screen layout into a simpler one
  - Moved grid settings from the right panel to the toolbar for easier access
  - Changed the right panel to a tabbed layout for better usability
- Chart editing
  - Added a BMSE-style right-click menu with support for inserting and deleting measures, mirror placement, and more
  - Expanded the Undo/Redo history, now limited by available memory
  - BGM lanes expand automatically, so you no longer need to manage the lane count manually
  - Horizontal scrolling with Shift+mouse wheel
  - The view now follows notes when they are moved with the arrow keys
  - Long notes can now be resized with Shift+arrow keys
  - Adjusted scrolling so the position is less likely to drift away from measure lines
  - Relative paths are supported for audio and image files
  - Added landmine note conversion
  - Rebuilt the splitter behavior to make it easier to use
- Other minor adjustments
