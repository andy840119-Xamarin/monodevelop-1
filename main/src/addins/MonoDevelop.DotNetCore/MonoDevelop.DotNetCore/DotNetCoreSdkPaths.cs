﻿//
// DotNetCoreSdkPaths.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Linq;
using MonoDevelop.Core;

namespace MonoDevelop.DotNetCore
{
	class DotNetCoreSdkPaths
	{
		public void FindSdkPaths (string sdk)
		{
			var dotNetCorePath = new DotNetCorePath ();
			if (dotNetCorePath.IsMissing)
				return;

			string rootDirectory = Path.GetDirectoryName (dotNetCorePath.FileName);
			string sdkRootPath = Path.Combine (rootDirectory, "sdk");
			if (!Directory.Exists (sdkRootPath))
				return;

			string[] directories = Directory.GetDirectories (sdkRootPath);
			string sdkDirectory = directories.OrderBy (directory => directory).LastOrDefault ();
			if (sdkDirectory == null)
				return;

			MSBuildSDKsPath = Path.Combine (sdkDirectory, "Sdks");

			// HACK: Set MSBuildSDKsPath environment variable so MSBuild will find the
			// SDK files when building and running targets.
			Environment.SetEnvironmentVariable ("MSBuildSDKsPath", MSBuildSDKsPath + Path.DirectorySeparatorChar);

			string sdkMSBuildTargetsDirectory = Path.Combine (MSBuildSDKsPath, sdk, "Sdk");
			ProjectImportProps = Path.Combine (sdkMSBuildTargetsDirectory, "Sdk.props");
			ProjectImportTargets = Path.Combine (sdkMSBuildTargetsDirectory, "Sdk.targets");

			Exist = File.Exists (ProjectImportProps) && File.Exists (ProjectImportTargets);

			if (Exist) {
				CheckIsSupportedSdkVersion (sdkDirectory);
				Exist = !IsUnsupportedSdkVersion;
			} else {
				IsUnsupportedSdkVersion = true;
			}
		}

		public bool IsUnsupportedSdkVersion { get; private set; }
		public bool Exist { get; private set; }
		public string ProjectImportProps { get; private set; }
		public string ProjectImportTargets { get; private set; }
		public string MSBuildSDKsPath { get; private set; }

		/// <summary>
		/// .NET Core SDK version needs to be at least 1.0.0-preview5-004460
		/// </summary>
		void CheckIsSupportedSdkVersion (string sdkDirectory)
		{
			try {
				string sdkVersion = Path.GetFileName (sdkDirectory);
				int buildVersion = -1;
				if (DotNetCoreSdkVersion.TryGetBuildVersion (sdkVersion, out buildVersion)) {
					if (buildVersion < DotNetCoreSdkVersion.MinimumSupportedBuildVersion) {
						IsUnsupportedSdkVersion = true;
						LoggingService.LogInfo ("Unsupported .NET Core SDK version installed '{0}'. Require at least 1.0.0-preview5-004460. '{1}'", sdkVersion, sdkDirectory);
					}
				} else {
					LoggingService.LogWarning ("Unable to get version information for .NET Core SDK. '{0}'", sdkDirectory);
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Error checking sdk version.", ex);
			}
		}
	}
}
