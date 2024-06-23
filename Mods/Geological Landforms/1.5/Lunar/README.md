
# Lunar Framework

### What is this?

I created this framework to make my mods more modular and fully self-contained. This way, they have no hard dependencies and can be used standalone, but also work when installed together with my other mods.
Basically each mod consist of individual components, and if multiple mods are present that contain the same component, only the newer version of that component is loaded.
In addition, the framework also contains some common code that is used by all my mods.

If you have any questions or would like to use the framework for your own mods, you can contact me on Discord: m00nl1ght#6339

### What are the .lfc files and why are they needed?

Those files contain a simple checksum that is used to verify the integrity of the mod on startup.
The reason for this is that Steam sometimes messes up when updating mods, for example by randomly stopping half way through, or leaving behind some old files.
This can cause mods to break in unpredictable ways, especially when the assembly files are affected.
To detect issues like this, Lunar Framework uses the checksum files to verify that no files are broken.
If there are, it displays a warning to the user, asking them to redownload/resubscribe the mod.
Unsubscribing and then resubscribing forces Steam to delete and then redownload the mod's files, which fixes the issue.
