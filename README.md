# EpicSteam
Launch Epic Store games (via Epic Launcher) from Steam

This is based on the idea of "Sean Z", so please go to https://seanzwrites.com/posts/how-to-play-epic-games-on-steam-and-steamlink/ for credits!

I just wanted a bit more convenience in creating links from Steam to Epic Launcher and I wanted the Epic Launcher to be terminated after I quit the game.


# Limitations
Limitation #1 is like "I don't write Windows applications and I've no idea what I am doing!" - so please bear with me.

I's not guaranteed that the Cloud Sync can complete it's work after you exit the game. I could not find a way to gracefully terminate the
Epic Launcher or to get an idea on when the Could Sync is completed.


# How to use
* In the Epic Launcher, select the "Create Shortcut" option for the game you want to add to Steam.
* In the shortcuts properties, select the "Web Document" tab and copy the URL. (Shortcut can now be deleted)
* In Steam, add a non Steam game to your library
* Select the epicsteam.exe and add the URL from step two as parameter (`epicsteam.exe "com.epicgames.launcher://..."`)
