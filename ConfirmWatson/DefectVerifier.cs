using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace ConfirmWatson
{
    public class DefectVerifier
    {
        public static bool VerifyDefectUsingWatson(string filePath)
        {
            string getUrl = "https://gateway.watsonplatform.net:443/visual-recognition/api/v3/classifiers/ElectricityCableCondition_697907586?version=2018-03-19";
            string postUrl = "https://gateway.watsonplatform.net/visual-recognition/api/v3/classify?version=2018-03-19";
            string userName = "apikey";
            string password = "m0gxu9Q_Jgw4Khh1JYPfM2mI35rx678Q7NnrOeVAunAU";
            string fileName = filePath;

            // Read file data
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>
            {
                {"images_file", new FileParameter(data, fileName)},
                {"classifier_ids", "ElectricityCableCondition_697907586"}
            };

            // Create request and receive response
            string userAgent = "Someone";
            HttpWebResponse webResponse =
                WebRequestManager.MultipartFormDataPost(postUrl, userAgent, postParameters, userName, password);

            StreamReader responseReader =
                new StreamReader(webResponse.GetResponseStream() ?? throw new InvalidOperationException());
            string fullResponse = responseReader.ReadToEnd();

            Watson result = JsonConvert.DeserializeObject<Watson>(fullResponse);

            if (result.images[0].classifiers[0].classes[0].@class == "Voltage_Normal_Condition")
            {
                var score = result.images[0].classifiers[0].classes[0].score;
                if (score > .8)
                    return false;
            }

            return true;
        }
    }
}
