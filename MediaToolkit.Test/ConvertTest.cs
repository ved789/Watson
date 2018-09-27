using MediaToolkit.Model;
using MediaToolkit.Options;
using NUnit.Framework;
using System;

namespace MediaToolkit.Test
{
    [TestFixture]
    public class ConvertTest
    {
        [TestCase]
        public void Can_CropVideo()
        {
            using (var engine = new Engine())
            {
                var mp4 = new MediaFile { Filename = @"D:\Softwares\Downloads\R.mp4" };

                engine.GetMetadata(mp4);

                var i = 0;
                while (i < mp4.Metadata.Duration.Seconds)
                {
                    var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(i) };
                    var outputFile1 = new MediaFile { Filename = string.Format("{0}\\image-{1}.jpeg", @"D:\Softwares\Downloads\R\", i) };
                    engine.GetThumbnail(mp4, outputFile1, options);
                    i++;
                }
            }
        }
    }
}