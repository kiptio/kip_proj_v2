using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Kip.Utils.Core
{
    public static class HttpUtils
    {
        #region [文件上传]
        public static string HttpUploadFile(string url, string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException();
            }

            FileInfo fileInfo = new FileInfo(filepath);

            string result = string.Empty;
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;

            var stream = request.GetRequestStream();
            stream.Write(boundarybytes, 0, boundarybytes.Length);

            var headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            var header = string.Format(headerTemplate, fileInfo.Name, filepath, GetContentType(fileInfo));
            var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            stream.Write(headerbytes, 0, headerbytes.Length);

            var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            var buffer = new byte[4096];
            var bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                stream.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            var trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            stream.Write(trailer, 0, trailer.Length);
            stream.Close();

            WebResponse wresp = null;
            try
            {
                wresp = request.GetResponse();

                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);

                result = reader2.ReadToEnd();
            }
            finally
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }

            return result;
        }
        #endregion

        #region [获取ContentType]
        private static string GetContentType(FileInfo fileInfo)
        {
            var contentType = "";
            switch (fileInfo.Extension.ToLower())
            {
                case ".jpg":
                    contentType = "image/jpeg";
                    break;
                case ".mp3":
                    contentType = "audio/mp3";
                    break;
                case ".amr":
                    contentType = "audio/amr";
                    break;
                case ".mp4":
                    contentType = "video/mp4";
                    break;
                default:
                    throw new NotSupportedException("文件格式不支持");
            }

            return contentType;
        }
        #endregion

        #region [HttpGet]
        public static string HttpGet(string url)
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;

            if (request == null)
                throw new ArgumentException();
            request.Method = "GET";

            return ResponseResult(request);
        }
        #endregion

        #region [HttpPost]
        public static string HttpPost(string url, string content)
        {
            HttpWebRequest request = HttpWebRequest.Create(url)
                         as HttpWebRequest;

            if (request == null)
                throw new ArgumentException();
            var postBytes = Encoding.UTF8.GetBytes(content);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.ContentLength = postBytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(postBytes, 0, postBytes.Length);
            }

            return ResponseResult(request);
        }
        #endregion

        #region [HttpGetFile]
        internal static string HttpGetFile(string url)
        {
            HttpWebRequest request = HttpWebRequest.Create(url)
                     as HttpWebRequest;

            if (request == null)
                throw new ArgumentException();
            request.Method = "GET";

            return ResponseResult(request);
        }
        #endregion

        #region [HttpPostXml]
        public static string HttpPostXml(string url, string content)
        {
            HttpWebRequest request = HttpWebRequest.Create(url)
                     as HttpWebRequest;

            if (request == null)
                throw new ArgumentException();
            var postdate = content;
            var postBytes = Encoding.UTF8.GetBytes(postdate);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.ContentLength = postBytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(postBytes, 0, postBytes.Length);
            }

            return ResponseResult(request);
        }
        #endregion

        #region [ResonseResult]
        internal static string ResponseResult(HttpWebRequest request)
        {
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new WebException("code" + response.StatusCode);
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                    {
                        var result = reader.ReadToEnd();
                        reader.Close();
                        stream.Close();

                        return result;
                    }
                }
            }
        }
        #endregion
    }
}
