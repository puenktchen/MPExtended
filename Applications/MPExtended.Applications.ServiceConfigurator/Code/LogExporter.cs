﻿#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Strings;
using ServicesConfig = MPExtended.Libraries.Service.Config.Services;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal class LogExporter
    {
        private const string PASSWORD_SUBSTITUTE = "Removed by ServiceConfigurator export";

        public static void Export(string savePath)
        {
            // save configuration, as we're going to change it
            Configuration.Save();

            // create zipfile
            using (var zipFile = ZipPackage.Open(savePath, FileMode.Create))
            {
                // copy log files
                DirectoryInfo logDir = new DirectoryInfo(Installation.GetLogDirectory());
                foreach (FileInfo file in logDir.GetFiles("*.log").Concat(logDir.GetFiles("*.bak")))
                {
                    var logPart = zipFile.CreatePart(new Uri("/" + file.Name, UriKind.Relative), "", CompressionOption.Maximum);
                    File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).CopyTo(logPart.GetStream());
                }

                // copy WebMediaPortal configuration (these don't contain any passwords, so we can copy them literally if they exist)
                CopyConfigurationFile(zipFile, "WebMediaPortal.xml");
                CopyConfigurationFile(zipFile, "StreamingPlatforms.xml");

                // copy literal config files
                WriteConfigFile(zipFile, (IConfigurationSerializer<MediaAccess>)Configuration.GetSerializer(ConfigurationFile.MediaAccess));
                WriteConfigFile(zipFile, (IConfigurationSerializer<StreamingProfiles>)Configuration.GetSerializer(ConfigurationFile.StreamingProfiles));

                // copy Authentication.xml without the passwords
                var authPart = zipFile.CreatePart(new Uri("/Authentication.xml", UriKind.Relative), "", CompressionOption.Maximum);
                var authSerializer = (IConfigurationSerializer<Authentication>)Configuration.GetSerializer(ConfigurationFile.Authentication);
                foreach (var user in authSerializer.Get().Users)
                    user.SetPasswordFromPlaintext(PASSWORD_SUBSTITUTE);
                authSerializer.Save(authSerializer.Get(), authPart.GetStream());

                // copy Services.xml without network password
                var servicePart = zipFile.CreatePart(new Uri("/Services.xml", UriKind.Relative), "", CompressionOption.Maximum);
                var serviceSerializer = (IConfigurationSerializer<ServicesConfig>)Configuration.GetSerializer(ConfigurationFile.Services);
                serviceSerializer.Get().NetworkImpersonation.SetPasswordFromPlaintext(PASSWORD_SUBSTITUTE);
                serviceSerializer.Save(serviceSerializer.Get(), servicePart.GetStream());

                // copy Streaming.xml without watch sharing password
                var streamingPart = zipFile.CreatePart(new Uri("/Streaming.xml", UriKind.Relative), "", CompressionOption.Maximum);
                var streamingSerializer = (IConfigurationSerializer<Streaming>)Configuration.GetSerializer(ConfigurationFile.Streaming);
                streamingSerializer.Get().WatchSharing.FollwitConfiguration["passwordHash"] = PASSWORD_SUBSTITUTE;
                streamingSerializer.Get().WatchSharing.TraktConfiguration["passwordHash"] = PASSWORD_SUBSTITUTE;
                streamingSerializer.Save(streamingSerializer.Get(), streamingPart.GetStream());
            }

            // reset the configuration after the changes we made to the passwords
            Configuration.Reset();
        }

        private static void CopyConfigurationFile(Package zipFile, string name)
        {
            var path = Path.Combine(Installation.GetConfigurationDirectory(), name);
            if (File.Exists(path))
            {
                var part = zipFile.CreatePart(new Uri("/" + name, UriKind.Relative), String.Empty, CompressionOption.Maximum);
                File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).CopyTo(part.GetStream());
            }
        }

        private static void WriteConfigFile<T>(Package file, IConfigurationSerializer<T> serializer) where T : class, new()
        {
            var fileName = serializer.Filename;
            var part = file.CreatePart(new Uri("/" + fileName, UriKind.Relative), String.Empty, CompressionOption.Maximum);
            serializer.Save(serializer.Get(), part.GetStream());
        }

        public static void ExportWithFileChooser()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".zip";
            dlg.Filter = UI.LogAndConfigurationArchive + "|*.zip";
            if (dlg.ShowDialog() == true)
            {
                Export(dlg.FileName);
                MessageBox.Show(String.Format(UI.ExportedLogsAndConfig, dlg.FileName), "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}