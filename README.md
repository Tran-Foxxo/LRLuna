# About LRLuna

LRLuna is an open source modification to Line Rider Advanced // https://github.com/jealouscloud/linerider-advanced
# Instructions

You can download the latest version on the releases page
Download gwen-lra here: https://github.com/jealouscloud/gwen-lra (it's necessary, put it in the dedicated folder)

Afterwards, use visual studio to build the thing.

## Windows

If you can't run the application, you probably need to install .net 4.6 which is a requirement for running LRA.
## Mac/Linux

LRLuna isn't available for Mac or Linux currently. If you wish to use LRLuna, I'd recommend to use WineBottler.
# Features

    Fast
    Features that aren't available in Line Rider Advanced
    Most features are LRA compatible

There are a lot of new features, I'll list them somewhere eventually
# Issues

Be sure to post any issues found in LRLuna here: https://github.com/AetherGaming/LRAether/issues
# Build

Run nuget restore in src (Visual Studio will do this for you) Build src/linerider.sln with msbuild or Visual Studio

This project requires .net 4.6 and C# 7 support.
# Libraries

This project uses binaries, sources, or modified sources from the following libraries:

    ffmpeg https://ffmpeg.org/
    NVorbis https://github.com/ioctlLR/NVorbis
    gwen-dotnet https://code.google.com/archive/p/gwen-dotnet/
    OpenTK https://github.com/opentk/opentk

You can find their license info in LICENSES.txt

The UI is a modified version by jealouscloud of gwen-dotnet
# License

Line Rider - Luna is licensed under GPL3.
