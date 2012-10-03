using System.Text;
using System.Net;
using System.IO;
using System.ComponentModel;

namespace Locke_HTTPHelper
{
    public class HTTPHelper
    {
        CookieContainer m_cookieContainer;
        int m_timeout;
        string m_userAgentStr = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; )";

        public HTTPHelper(int timeout)
        {
            m_cookieContainer = new CookieContainer();
            m_timeout = timeout;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
        }

        bool CheckValidationResult(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        {
            return true;
        }

        public static byte[] GetResponseBytes(HttpWebRequest request)
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream RawStream = response.GetResponseStream();

            if (response.ContentLength <= 0)
                return new byte[0];
            byte[] data = new byte[response.ContentLength];
            RawStream.Read(data, 0, (int)response.ContentLength);

            RawStream.Close();

            return data;
        }

        public static string GetResponseString(HttpWebRequest request)
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream RawStream = response.GetResponseStream();

            StreamReader reader;
            Encoding encode;
            if (response.CharacterSet.ToLowerInvariant().Contains("utf-8"))
                encode = Encoding.UTF8;
            else
                encode = Encoding.Default;
            reader = new StreamReader(RawStream, encode);

            string html = reader.ReadToEnd();

            reader.Close();
            RawStream.Close();

            return html;
        }

        public HttpWebRequest CreateHTTPGetRequest(string url, bool addCharsetToHeader)
        {
            HttpWebRequest request;
            string requestUrl = url;
            while (true)
            {
                request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.Method = "GET";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                if (addCharsetToHeader)
                    request.Headers.Add("Accept-Charset", "GB2312,utf-8");
                request.UserAgent = m_userAgentStr;
                request.Timeout = m_timeout;
                request.CookieContainer = m_cookieContainer;

                if (request.RequestUri != request.Address)//转向
                    requestUrl = request.Address.AbsoluteUri;
                else
                    break;
            }

            return request;
        }

        public HttpWebRequest CreateHTTPPOSTRequest(string url, string poststring, bool addCharsetToHeader)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.UserAgent = m_userAgentStr;
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            if(addCharsetToHeader)
                request.Headers.Add("Accept-Charset", "GB2312,utf-8");
            request.CookieContainer = m_cookieContainer;
            request.Timeout = m_timeout;
            request.ContentType = "application/x-www-form-urlencoded";

            byte[] postdata = Encoding.ASCII.GetBytes(poststring);
            request.ContentLength = postdata.Length;
            Stream RequestStream = request.GetRequestStream();
            RequestStream.Write(postdata, 0, postdata.Length);
            RequestStream.Close();

            return request;
        }

        public string HttpPost_String(string url, string postStr)
        {
            HttpWebRequest request = CreateHTTPPOSTRequest(url, postStr, true);
            return GetResponseString(request);
        }

        public string HttpGet_String(string url)
        {
            HttpWebRequest request = CreateHTTPGetRequest(url, true);
            return GetResponseString(request);
        }
    }
}
