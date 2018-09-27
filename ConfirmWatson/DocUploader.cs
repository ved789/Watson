using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ConfirmWatson
{
    public class DocUploader
    {
        public static bool UploadDoc(string enquiryNo, string imagePath)
        {
            CentralDocLink docLink = new CentralDocLink
            {
                documentName = Path.GetFileName(imagePath),
                blobData = GetBase64(imagePath),
                documentNotes = "Uploaded from IBM Watson!",
                documentDate = DateTime.Now
            };
            IList<CentralDocLink> centralDocLinks = new List<CentralDocLink>();
            centralDocLinks.Add(docLink);

            DocJson docJson = new DocJson
            {
                key = enquiryNo,
                centralDocLinks = centralDocLinks
            };
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(docJson);
            

            using (var client = new WebClient())
            {
                client.Headers.Add("Authorization", "Bearer ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnJhV1FpT2lJeWNHZHhUVGcxTTNack5GQnVOREZDVGxoak5UaFJQVDBpTENKMWMyVnlibUZ0WlNJNklsTkNVeUlzSW14cFkyVnVZMlVpT2lKRFQwNUdNREV5TXpRaUxDSnpaWE56YVc5dUlqb2lOVE15TXpFeklpd2lkWE5sVEdsalpXNXpaU0k2SWxSeWRXVWlMQ0pxZEdraU9pSmFVMXBCVVVkSFJpNVdXRmRFVlUxR1MwNUdJaXdpYm1KbUlqb3hOVE0yT1RFeU5EZ3hMQ0psZUhBaU9qRTFNemsxTURRM09ERXNJbWxoZENJNk1UVXpOamt4TWpjNE1Td2lhWE56SWpvaVkyOXVabWx5YlM1d1lpNWpiMjBpZlEucmFTMkJiM2VxVXJ6a05aY2EzRHN3SzE4a0ZOUk5KSVB5MGJXVXk1U05Qdw==");
                client.Headers.Add("Content-Type", "application/json");
                var resp = client.UploadString(@"http://10.120.64.58/Confirm/ConfirmWeb/api/QAUtility_noida/centralEnquiries", "POST", jsonString);
            }

            return true;
        }

        private static string GetBase64(string imagePath)
        {
            byte[] imageArray = File.ReadAllBytes(imagePath);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            return base64ImageRepresentation;
        }
    }

    public class CentralDocLink
    {
        public string documentName { get; set; }
        public string blobData { get; set; }
        public string documentNotes { get; set; }
        public DateTime documentDate { get; set; }
    }

    public class DocJson
    {
        public string key { get; set; }
        public IList<CentralDocLink> centralDocLinks { get; set; }
    }

}