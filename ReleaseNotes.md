# Release Notes

## Version 1.0 
*Release Date: 2023-xx-xx*

* **bugfix:** If the discogs client reaches the request limit, the app does not crash but 
shows an error dialog
* **bugfix:** Filter textboxes accept now ENTER for setting the filter
* **bugfix:** Status line is now updated correctly when changes have been made
* **feature:** An appropriate icon is now displayed for the application (exe) and for all windows of the app
* **feature:** Some menu elements with Hotkeys for keyboard navigation added:
  * Create New Album: CTRL-N
  * Delete Selected Album: CTRL-D
  * Toggle track list locking: CTRL-T
  * Jump to Thumbnail Filter: CTRL+ALT-T
  * Jump to Interpret Filter: CTRL+ALT-I
  * Clear All Filters: CTRL+ALT-C
* **feature:** Weight between thumbnail panel and track list panel is saved when app exits and restored when the application starts again
* **feature:** Prices in status line and detail panel are now displayed with the user locale
* **feature:** Currencies are now shown in the context of the user locale
* **feature:** Authentication for Discogs now with OAuth access so that no personal user token of the user is required
* **feature:** Genrers can be managed now in a dialog: change names, create new genres and delete unused genres
* **feature:** Interprets can be managed now in a dialog: change names, create new interprets and delete unused interprets
* **feature:** Size of thumbnails can now be sized with a slider at the top of the thumbnail panel. The size is saved and restored at the next start of the app.
* **feature:** Advanced search dialog for albums (via menu, button over thumbnail panel or hot key CTRL-F) 
* **feature:** The albums which are currently shown in the thumbnail view can now be exported to Excel
 
 