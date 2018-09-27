using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ConfirmWatson
{
    public static class WebRequestManager
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public static string GetRequest(string url, string username = null, string password = null)
        {
            return SendRequest(url, WebRequestMethods.Http.Get, null, username, password);
        }

        public static string PostRequest(string url, object data, string username = null, string password = null)
        {
            return SendRequest(url, WebRequestMethods.Http.Post, data, username, password);
        }

        private static string SendRequest(string url, string method, object data = null, string username = null,
            string password = null)
        {
            bool isAuthenticationRequired = !string.IsNullOrWhiteSpace(username);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
            if (isAuthenticationRequired)
            {
                NetworkCredential myNetworkCredential = new NetworkCredential(username, password);
                CredentialCache myCredentialCache = new CredentialCache
                {
                    {
                        new Uri(url), "Basic", myNetworkCredential
                    }
                };
                httpWebRequest.UseDefaultCredentials = true;
                httpWebRequest.Credentials = myCredentialCache;
                httpWebRequest.PreAuthenticate = true;
            }

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Tls;
            httpWebRequest.Timeout = 300000; // 5 minutes
            if (method == WebRequestMethods.Http.Post)
            {
                httpWebRequest.Method = WebRequestMethods.Http.Post;

                if ((data is byte[] bytes && bytes.Length > 0))
                {
                    httpWebRequest.ContentType = "application/octet-stream";
                    using (var streamWrite = new BinaryWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWrite.Write(bytes);
                        streamWrite.Flush();
                        streamWrite.Close();
                    }
                }
                else
                {
                    string stringData = data as string;
                    if (!string.IsNullOrWhiteSpace(stringData))
                    {
                        httpWebRequest.ContentType = "text/json";
                        using (StreamWriter streamWrite = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            streamWrite.Write(stringData);
                            streamWrite.Flush();
                            streamWrite.Close();
                        }
                    }
                    else
                    {
                        httpWebRequest.ContentLength = 0;
                    }
                }
            }
            else
            {
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                httpWebRequest.ContentType = "text/json";
            }

            HttpWebResponse httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            string result;
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent,
            Dictionary<string, object> postParameters, string username = null, string password = null)
        {
            string formDataBoundary = $"----------{Guid.NewGuid():N}";
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

            return PostForm(postUrl, userAgent, contentType, formData, username, password);
        }

        private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData,
            string username = null, string password = null)
        {
            if (!(WebRequest.Create(postUrl) is HttpWebRequest request))
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            bool isAuthenticationRequired = !string.IsNullOrWhiteSpace(username);
            if (isAuthenticationRequired)
            {
                NetworkCredential myNetworkCredential = new NetworkCredential(username, password);
                CredentialCache myCredentialCache = new CredentialCache
                {
                    {
                        new Uri(postUrl), "Basic", myNetworkCredential
                    }
                };
                request.UseDefaultCredentials = true;
                request.Credentials = myCredentialCache;
                request.PreAuthenticate = true;
            }

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Tls;
            request.Timeout = 300000; // 5 minutes

            // Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {
                // Add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter)
                {
                    FileParameter fileToUpload = (FileParameter) param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header =
                        $"--{boundary}\r\nContent-Disposition: form-data; name=\"{param.Key}\"; filename=\"{fileToUpload.FileName ?? param.Key}\"\r\nContent-Type: {fileToUpload.ContentType ?? "application/octet-stream"}\r\n\r\n";

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData =
                        $"--{boundary}\r\nContent-Disposition: form-data; name=\"{param.Key}\"\r\n\r\n{param.Value}";
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }
    }

    public class FileParameter
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public FileParameter(byte[] file) : this(file, null)
        {
        }

        public FileParameter(byte[] file, string filename) : this(file, filename, null)
        {
        }

        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }
    }
}