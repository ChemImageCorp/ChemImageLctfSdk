# ChemImage LCTF SDK
CI Status: <a href="https://github.com/ChemImageFT/ChemImageLctfSdk/actions?query=workflow%3ABuild">
<img alt="GitHub Actions Build Status"
src="https://github.com/ChemImageFT/ChemImageLctfSdk/workflows/Build/badge.svg"></a>

This project is an SDK for controlling ChemImage LCTFs. 

## Support
If support for this SDK is needed, you can submit an issue
[here](https://github.com/ChemImageFT/ChemImageLctfSdk/issues/new) or email us at: [LCTFSupport@chemimage.com](mailto:LCTFSupport@chemimage.com).

## Instructions

### SDK Installation
The SDK can be downloaded and installed from the releases page:
[Latest](https://github.com/ChemImageFT/ChemImageLctfSdk/releases/latest).

Once installed, LCTF Commander can be run to check that the LCTF is working correctly.

A user manual for LCTF Commander is included in the installer, and can also be found
[here](https://github.com/ChemImageFT/ChemImageLctfSdk/blob/master/Installer/SdkSetup/LCTFCommanderUserManual.pdf).

### Driver Installation
When an LCTF is connected to a Windows computer by the USB cable, drivers should automatically be installed by
Windows Update. You may need to enable Windows Update to automatically install drivers. Once installed, the LCTF
should appear in Device Manager under "Universal Serial Bus devices" as:
- "ChemImage Tunable Filter" in Windows 10
- "WinUSB Device" in Windows 7

## Contents
The installer built from this repository includes:
- The [ChemImageLCTF library](https://github.com/ChemImageFT/ChemImageLCTF)
- [LCTF Commander](https://github.com/ChemImageFT/ChemImageLctfSdk/tree/master/src/LCTFCommander), a utility
program with simple controls for an LCTF device.
- [Example code](https://github.com/ChemImageFT/ChemImageLctfSdk/tree/master/src) for integrating an LCTF
into your .NET application.

The example source code can also be acquired by downloading or cloning this repository.

## Licensing 
This project is licensed under the [MIT License](LICENSE). Copyright (c) 2020 ChemImage Corporation.

[LibUsbDotNet](https://github.com/LibUsbDotNet/LibUsbDotNet/) is licensed under the
[LGPL v3.0](https://github.com/LibUsbDotNet/LibUsbDotNet/blob/master/LICENSE).
Copyright (c) 2006-2010 Travis Robinson.

[WPF Toolkit](https://github.com/xceedsoftware/wpftoolkit/tree/3.6.0) from Xceed Software is licensed under the
[MS-PL](https://github.com/xceedsoftware/wpftoolkit/blob/3.6.0/license.md).
Copyright (c) 2007-2018 Xceed Software Inc.
