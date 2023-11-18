# VinylStudio

VinylStudio is an open source software that helps you managing your vinyl record collection.
It is not meant to be a audio player, but only a managing software which gives you an overview,
statistics, and search functionality in order to quickly find your desired piece of black gold.

![image](images/screenshot.png)

## Installation
*to be done*

### Importing Data from Legacy System (VinylShelf)
If you have used the legacy system "VinylShelf" before, you can migrate your old database to
the new format of VinylStudio. Choose *File* -> *Migration* -> *Migrate from VinylShelf 0.2* 
from the main menu.

Be aware: if you have already entered data into VinylStudio, the migration will delete all
these data and overwrite it with the migrated data!

After this security warning a file selection dialog opens up. Select the file *database.json*
from your VinylShelf installation. Most probably you'll find this file in the directory

    ~\.VinylShelf\data\database.json

whereat the tilde symbolizes your home directory.

Typically this should work without any problems. After the migration you have all your data
and cover thumbnails copied (and converted) to the directory where you have installed
VinylStudio.

## Navigation

Navigation in VinylStudio is more or less self-explaining.

The main window of the application is divided into four sections.

### Thumbnail Panel

In the top middle of the window you'll see the thumbnail panel.

![image](images/thumbnailPanel.png)

If you already have some albums entered into your database, you'll find  the thumbnails
of these album in the thumbnail panel.

Click on a thumbnail in order to show the details of the album in the detail panel. For
[editing](#editing-existing-albums)
an existing album, you have to double click on the thumbnail.

There are numerous ways of filter and ordering options that affect the thumbnails shown
in the thumbnail panel. These options are described in the chapter 
[Sorting and Filtering Options](#sorting-and-filtering-options) in more detail.

The orange button in the top left corner of the thumbnail panel is used to 
[create a new album](#creating-new-albums)

### Interpret Panel
In the top right you can see the interpret panel.

![image](images/interpretPanel.png)

The interpret panel initially shows you all interprets present in your database, regardless
if there are albums for this interpret stored or not.

However, the interpret panel can be filtered, too. This is described in the chapter
[Filtering Interprets](#filtering-interprets)

### Detail Panel
In the top left you can see the detail panel.

![image](images/detailPanel.png)

If you have selected an album in the thumbnail panel, the details are automatically
shown in the detail panel. 

This panel might automatically change its size, when it needs more room to show a property, e.g.
the title of the album.

### Track List Panel
In the bottom middle you'll see the track list panel.

![image](images/tracklistPanel.png)

This table shows you the tracks and some usefull information of the currently selected album.
In the chapter [Editing Track Lists](#editing-track-lists) you find some information on how
to edit and create track lists for an album.

## Sorting and Filtering Options

### Filtering Interprets

### Filtering Thumbnails

### Sorting options

## HowTo

### Creating new Albums

### Editing existing Albums

### Deleting albums

### Editing Track Lists

### Discogs Support

#### A Word about Discogs

#### Rertrieving Track Lists by Discogs

#### Retrieving Cover Images by Discogs