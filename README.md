[![.NET Core Desktop Test](https://github.com/esseivan/PictureSorter/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/esseivan/PictureSorter/actions/workflows/dotnet-desktop.yml)
[![Validate PR](https://github.com/esseivan/PictureSorter/actions/workflows/validate-style.yml/badge.svg)](https://github.com/esseivan/PictureSorter/actions/workflows/validate-style.yml)
[![Release](https://github.com/esseivan/PictureSorter/actions/workflows/release.yml/badge.svg)](https://github.com/esseivan/PictureSorter/actions/workflows/release.yml)

# PictureSorter
App to help sort pictures

## Install

You can download the ClickOnce application (Windows) at https://esseivan.github.io/PictureSorter/PictureSorter.application.

## Preview

![image](https://user-images.githubusercontent.com/14168019/217786677-fa23f1e1-9eed-4670-beb1-53defe1a526c.png)
![image](https://user-images.githubusercontent.com/14168019/217786767-3e5c3395-bb74-424b-bbeb-3da5716ac18f.png)


## How to use it

1. Open the folder containing the pictures (or a picture in that folder) with open folder (resp. open)
2. Validate the pictures ones you want to keep (green) and the one to discard (red).
   Change are automatically saved in a hidden file "pictureSorter.pssave" in that folder
   A backup of that save is made with the general Check (or uncheck) all action and will not overwrite the save until another change is made
3. Once the pictures are seleted, go to File - Export to copy them in a new folder (with suffix "tri <n>").
   NONE of the original picture are modified nor overwritten, you'll have to delete them manually if you want.
   The app then open that folder in the windows explorer


## Build yourself

Just open the solution in Visual Studio


## Others

Image from : <a href="https://www.flaticon.com/free-icons/picture" title="picture icons">Picture icons created by Freepik - Flaticon</a>
