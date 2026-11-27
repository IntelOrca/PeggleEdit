# Getting Started

## What you need
* Windows 7 or later. Windows 10 and 11 already have everything PeggleEdit needs. On older versions, install the .NET Framework 4.7.2 or later.
* PeggleEdit also runs on Linux with Mono or Wine.
* Peggle Nights, to play the levels you make. PeggleEdit can also open the levels of Peggle, Peggle Deluxe and Pego.

## Installing
1. Download **peggleedit.zip** from the [latest release](https://github.com/IntelOrca/PeggleEdit/releases/latest).
2. Make a new folder and extract everything in the zip into it.
3. Open **peggleedit.exe**.

## Staying up to date
PeggleEdit checks for a new version when it starts and offers to update itself.

Under **PeggleEdit Options > Application**, you can choose an update ring:
* **Release**: tested versions only. Best for most people.
* **Nightly**: a new build for every change. You get new features and fixes first, but they may have bugs.

## Your first level pack
1. PeggleEdit starts with a new, empty pack open in the **Pack Explorer**. It already has one level. (To start again later, click **New** in the orca menu at the top left.)
2. Double-click the level to open it.
3. Right-click the level and choose **Properties** to give it a name.
4. Right-click the level and choose **Background > Import** to pick an image. It is shown behind the game's frame, scaled to cover 800 x 600, unless it is made to fit already (800 x 600, or 646 x 543 like Peggle's own).
5. Pick a tool from the **Tools** tab, such as **Peg**, and click on the level to place objects.
6. Click **Save** and save the pack in the `levelpacks` folder inside your Peggle Nights folder. Create the folder if it is not there.
7. Start Peggle Nights to play it. Close the game before you save again, as it keeps the pack open.

To add more levels, right-click **Levels** and choose **Add Level**.

See the [Editor Guide](EditorGuide.md) for what each tool does, and [Level Packs](LevelPacks.md) for setting up levels, images and challenges.

## Building from source
The source code is on [GitHub](https://github.com/IntelOrca/PeggleEdit). Install the [.NET SDK](https://dotnet.microsoft.com/download) and run `dotnet build` in the folder you cloned it to. You can also open `PeggleEdit.sln` in Visual Studio 2022 or later.

## Need help?
Have a look at the [FAQ](FrequentlyAskedQuestions.md), or ask on the [issues page](https://github.com/IntelOrca/PeggleEdit/issues).
