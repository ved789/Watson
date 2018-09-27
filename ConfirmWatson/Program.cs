using System;
using System.IO;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;

namespace ConfirmWatson
{
  class Program
  {
    private const string DirPath = @"D:\Softwares\Downloads\";

      static void Main(string[] args)
      {
          DocUploader.UploadDoc("", DirPath + "13.jpeg");
          ConvertVideoToImage();
          Console.WriteLine("All Done!!!");
          Console.ReadKey();
      }

      private static void ConvertVideoToImage(string fileName = DirPath + "R.mp4")
    {
      using (var engine = new Engine())
      {
        Console.WriteLine("Started Converting video to frames...");
        var mp4 = new MediaFile {Filename = fileName};

        engine.GetMetadata(mp4);

        var i = 0;
        var totalSeconds = mp4.Metadata.Duration.Minutes * 60;
        totalSeconds += mp4.Metadata.Duration.Seconds;

        DirectoryInfo di = new DirectoryInfo(DirPath + @"R\");

        foreach (FileInfo file in di.GetFiles())
          file.Delete();

        while (i < totalSeconds)
        {
          var options = new ConversionOptions {Seek = TimeSpan.FromSeconds(i)};
          var outputFile = new MediaFile
          {
            Filename = $"{DirPath + @"R\"}{i}.jpeg"
          };
          engine.GetThumbnail(mp4, outputFile, options);
          i++;
        }

        Console.WriteLine(@"Video frames created!");

        Console.WriteLine(@"Started Verifying video frames with IBM Watson...");
        i = 0;
        while (i < totalSeconds)
        {
          string file = $"{DirPath + @"R\"}{i}.jpeg";
          var response = DefectVerifier.VerifyDefectUsingWatson(file);
          if (response)
          {
            //Raise defect in Confirm
            var enquiryNo = EnquiryRaiser.Create(file);
            DocUploader.UploadDoc(enquiryNo, file);

            Console.WriteLine(@"Issue found and raised in Confirm @ {0} second...", i);
            return;
          }
          i++;
        }
      }
    }
  }
}