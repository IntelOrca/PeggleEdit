# PAK Format

A `.pak` file is an archive of files, like a simple zip with no compression. Peggle's `main.pak` and level packs both use it.

## Encryption
Every byte of the file is XORed with `0xF7`. Undo this before reading anything else.

## Structure
All numbers are little endian.

### Header
* 4 byte integer, magic number `0xBAC04AC0`.
* 4 byte integer, version, `0`.

### File list
A list of records, one for each file:
* 1 byte, flags. If `0x80` is set, this is the end of the list and nothing else in the record follows.
* 1 byte, length of the file name.
* The file name, one byte per character, with `\` between folders.
* 4 byte integer, size of the file.
* 8 byte integer, the file's time, as a Windows FILETIME.

### File data
The data of every file, one after another, in the same order as the list.

## Level packs
A level pack holds:
* `<pack>.cfg`, named after the `.pak` file: the pack's name and description, its levels, and its challenges.
* `levels\<level>.dat`: each level. See [Level Format](LevelFormat.md).
* `levels\<level>.jpg`, `.png` or `.jp2`: each level's background.
* `levels\cached_thumbnails\<level>.jpg`, `.png` or `.jp2`: each level's thumbnail.
* Any other images that the levels use.

A game's `main.pak` instead lists its levels in `levels\stages.cfg` and its challenges in `levels\trophy.cfg`.
