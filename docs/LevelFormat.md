The level format is a non-compressed file containing level entries of variable length.

## Structure ##
  * 0x00 - Header
  * 0x09 - Entries

### Header ###
  * 0x00 - 4 byte integer, file version.
  * 0x04 - 1 byte integer
  * 0x05 - 4 byte integer, number of level entries to read.

### Level Entry ###
  * 4 byte integer, normally set to 1
  * 4 byte integer, entry type (e.g. 6 = brick)
  * 4 byte integer, generic flags
  * generic data
  * entry data

## Generic Flags & Data ##
Each level entry has a generic integer stating flags such as **Visible**. Some flags tell the file reader to read additional data after the flags such as the movement info.

## PeggleEdit data ##
The game reads only the number of entries given in the header, and ignores the rest of the file. PeggleEdit keeps its
own data about a level there, after the last entry, so that the level stays playable:
  * 8 bytes, `PEGLEDIT`
  * 4 byte integer, format version, 1
  * 8 byte integer, FNV-1a hash of the file before this data. If the level has been changed by another program, the
    hash does not match and PeggleEdit does not use the data.
  * 4 byte integer, size of the chunks that follow
  * chunks: a 4 character tag, a 4 byte integer size, then the chunk's data. Unknown chunks are skipped.

The `GENS` chunk holds the level's generators: a 4 byte integer count, then for each generator the index of the first
entry it generated and the number of entries it generated (4 byte integers), then the generator written as a level
entry. The entries a generator generated are in the level in its place, so that the game plays them; when PeggleEdit
opens the level it replaces them by the generator again.
