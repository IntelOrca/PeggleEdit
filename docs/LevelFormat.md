# Level Format

A level file (`.dat`) is an uncompressed file holding a list of level entries (objects) of varying length.

All numbers are little endian.

## Structure
  * 0x00: header
  * 0x09: entries

### Header
  * 0x00: 4 byte integer, file version.
  * 0x04: 1 byte integer.
  * 0x05: 4 byte integer, number of level entries to read.

### Level Entry
  * 4 byte integer, normally set to 1
  * 4 byte integer, entry type: 2 line, 3 poly, 5 ball, 6 brick, 8 hole, 9 emitter. PeggleEdit's generators use 1001 peg generator, 1002 brick generator, 1003 Peg Pen and 1004 Brick Pen; PeggleEdit now stores these only in its own data (see below), but levels saved by older versions may have them among the entries.
  * 4 byte integer, generic flags
  * generic data
  * entry data

## Generic Flags & Data
Each level entry has a generic integer stating flags such as **Visible**. Some flags tell the file reader to read additional data after the flags such as the movement info.

## PeggleEdit data
The game reads only the number of entries given in the header, and ignores the rest of the file. PeggleEdit keeps its own data about a level there, after the last entry, so that the level stays playable:
  * 8 bytes, `PEGLEDIT`
  * 4 byte integer, format version, 1
  * 8 byte integer, FNV-1a hash of the file before this data. If the level has been changed by another program, the hash does not match and PeggleEdit does not use the data.
  * 4 byte integer, size of the chunks that follow
  * chunks: a 4 character tag, a 4 byte integer size, then the chunk's data. Unknown chunks are skipped.

The `GENS` chunk holds the level's generators: a 4 byte integer count, then for each generator:
  * 4 byte integer, the index of the first entry it generated (not counting null entries)
  * 4 byte integer, the number of entries it generated
  * 8 byte integer, FNV-1a hash of those entries as written
  * the generator, written as a level entry

The entries a generator generated are in the level in its place, so that the game plays them. When PeggleEdit opens the level it replaces them by the generator again, if they still match the hash; otherwise it keeps them as ordinary entries, so that nothing changed by another program is lost.

The game has no generators, so an object whose mover links to a generator's mover (or to one of its parent movers) is written with its own copy of that mover, which moves the same way. The `LINK` chunk turns those copies back into links: a 4 byte integer count, then for each link five 4 byte integers:
  * the id of the copy, as the game numbers objects, when an entry holds it (otherwise any value)
  * the index of the generator holding the copy, or -1 when an entry holds it
  * how many owned movers down the holding generator's movers the copy is
  * the index of the generator linked to
  * how many owned movers down that generator's movers the mover linked to is (0 for its own mover)

The `THMB` chunk, which is empty, says that the level's thumbnail is one the user chose, so PeggleEdit keeps it when the level changes, rather than making a new one.

The `OBJS` chunk holds which objects are in a group, or locked: a 4 byte integer count, then for each such object three 4 byte integers:
  * the index of the object among the objects as PeggleEdit has them, with each generator in place of the entries it generated (not counting null entries)
  * the group it is in, or 0 for none
  * flags: 1, the object is locked

If the level has been changed by another program, the indexes may no longer be right, so the groups and locks are not used.

The `GENS` chunk is always the first chunk, even when the level has no generators.

Peg Pen and Brick Pen generators store, after the generic data, a 2 byte point count whose top bits are flags: 0x4000, the position (two floats) follows; 0x8000, the interval (4 byte integer) follows the points. Each point is two floats, a kind byte and a padding byte.
