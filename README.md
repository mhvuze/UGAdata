# UGAdata
Unpacker for Uncharted: Golden Abyss gamedata.bin container.

The archive contains file path strings longer than 170 characters on its own so I decided to use `Delimon.Win32.IO` for unpacking to work around the `System.IO` char limit of 260.

Long paths may cause issues with some apps however, so preferably unpack the gamedata.bin to the root of a drive to keep the length as short as possible.

Just drag and drop the gamedata.bin on the executable or use `UGAdata.exe gamedata.bin` in your cmd.
