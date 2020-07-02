# ChemImage LCTF SDK
CI Status: <a href="https://github.com/ChemImageFT/ChemImageLctfSdk/actions?query=workflow%3ABuild">
<img alt="GitHub Actions Build Status"
src="https://github.com/ChemImageFT/ChemImageLctfSdk/workflows/Build/badge.svg"></a>

## Overview

ChemImage LCTF SDK is an installable .NET software development kit for ChemImage LCTFs devices. Alternatively, you can get started with development by referencing the nuget package as described here: [ChemImageLCTF Library](https://github.com/ChemImageFT/ChemImageLCTF#Installation).

### SDK Contents

The installer built from this repository includes:

| Item | Description |
|--|--|
|LCTF Commander| A program that can be used to control an LCTF device. |
|[ChemImageLCTF Library](https://github.com/ChemImageFT/ChemImageLCTF#Overview)| The DLL that is used to control LCTF devices. |
|[Example code](https://github.com/ChemImageFT/ChemImageLctfSdk/tree/master/src)| C# projects showing how an LCTF device can be integrated into your own .NET application. |

The example source code can also be acquired by downloading or cloning this repository.

### Installation

#### SDK Installation
The SDK can be downloaded and installed from the releases page:
[Latest](https://github.com/ChemImageFT/ChemImageLctfSdk/releases/latest).

#### Driver Installation
When an LCTF is connected to a Windows computer by the USB cable, drivers should automatically be installed by
Windows Update. You may need to enable Windows Update to automatically install drivers. Once installed, the LCTF
should appear in Device Manager under "Universal Serial Bus devices" as:
- "ChemImage Tunable Filter" in Windows 10
- "WinUSB Device" in Windows 7

### Basic Usage

#### LCTF Commander
A user manual for LCTF Commander is included in the SDK installer, and can also be found
[here](https://github.com/ChemImageFT/ChemImageLctfSdk/blob/master/Installer/SdkSetup/LCTFCommanderUserManual.pdf).

#### ChemImageLCTF Library
Basic usage of the library is described [here](https://github.com/ChemImageFT/ChemImageLCTF#Basic-Usage).

#### Example Code
The example code is copied to the same directory that the rest of the SDK is installed to. A shortcut to this directory is also installed to the start menu.

"LCTF SDK.sln" contains the example projects and can be built and run in Visual Studio. You can get Visual Studio Community [here](https://visualstudio.microsoft.com/) if you do not already have a copy.

The example code can also be obtained by cloning this repository through Git. The installed example code directory is just a copy of this repository's src directory.

## Support
If support for this SDK is needed, you can submit an issue
[here](https://github.com/ChemImageFT/ChemImageLctfSdk/issues/new) or email us at: [LCTFSupport@chemimage.com](mailto:LCTFSupport@chemimage.com).

## Licensing 
This project is licensed under the [MIT License](LICENSE). Copyright (c) 2020 ChemImage Corporation.

[LibUsbDotNet](https://github.com/LibUsbDotNet/LibUsbDotNet/) is licensed under the
[LGPL v3.0](https://github.com/LibUsbDotNet/LibUsbDotNet/blob/master/LICENSE).
Copyright (c) 2006-2010 Travis Robinson.

[WPF Toolkit](https://github.com/xceedsoftware/wpftoolkit/tree/3.6.0) from Xceed Software is licensed under the
[MS-PL](https://github.com/xceedsoftware/wpftoolkit/blob/3.6.0/license.md).
Copyright (c) 2007-2018 Xceed Software Inc.
