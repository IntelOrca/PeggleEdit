# Introduction #
PeggleEdit is written in CSharp uses the .NET Framework. This allows PeggleEdit to maintain a nice hierarchy of classes to implement all the features needed and make it easy to add new tools and functions.

# Main Assemblies #
**IntelOrca.PeggleEdit.Tools** contains all the classes need to read and save levels and level packs, display the level, supply any client that uses the assembly to all the properties of the level and the entries.

**IntelOrca.PeggleEdit.Designer** is the executable build of PeggleEdit which includes all the classes for designing and developing the levels. It includes the LevelEditor control and all the MDI components.

# Other Assemblies #
Other libraries are used to create PeggleEdit. PeggleEdit needs to be functional but it doesn't hurt to make it look nice and supply it with some more up to date controls. Thus other open source libraries are used.

**Crom.Controls** is a helpful library which contains the window docking control used in the main MDI window.

**System.Windows.Forms.Ribbon** contains all the classes for the Microsoft Office like ribbon control which is used as the main toolbar.

# Important Classes #

**LevelEntry** is an abstract class which all Peggle objects and PeggleEdit's own objects inherit. Override all the relevant members for the object to be usable.