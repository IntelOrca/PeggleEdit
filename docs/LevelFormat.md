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