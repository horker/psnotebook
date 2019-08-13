﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Horker.Notebook.Models;

namespace Horker.Notebook.Cmdlets
{
    [Cmdlet("Get", "WpfImage")]
    [OutputType(typeof(Image))]
    public class GetWpfImage : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string Source { get; set; }

        protected override void BeginProcessing()
        {
            var fullPath = Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, Source);

            Image image = null;
            CurrentState.Dispatcher.Invoke(() => {
                image = new Image()
                {
                    Source = GetImageSource(fullPath),
                    MaxHeight = Configuration.MaxOutputHeight
                };
            });

            WriteObject(image);
        }

        private ImageSource GetImageSource(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var decoder = BitmapDecoder.Create(
                    fs,
                    BitmapCreateOptions.None,
                    BitmapCacheOption.OnLoad);
                var bmp = new WriteableBitmap(decoder.Frames[0]);
                bmp.Freeze();
                return bmp;
            }
        }
    }
}
