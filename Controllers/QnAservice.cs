using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace isRock.MsQnAMaker
{
    public class Client
    {
        public string Endpoint { get; set; }
        public string SubscriptionKey { get; set; }
        public string KnowledgeBaseID { get; set; }
        public string domain { get; set; }


        /// <summary>
        /// beta Version API
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="KnowledgeBaseID"></param>
        /// <param name="SubscriptionKey"></param>
        [Obsolete("for preview version api")]
        public Client(string domain, string KnowledgeBaseID, string SubscriptionKey)
        {
            this.domain = domain;
            this.KnowledgeBaseID = KnowledgeBaseID;
            this.SubscriptionKey = SubscriptionKey;
        }

        /// <summary>
        /// GA Version API
        /// </summary>
        /// <param name="Endpoint">ex. https://{yourapp}.azurewebsites.net/qnamaker/knowledgebases/{guid}/generateAnswer </param>
        /// <param name="EndpointKey">Endpoint Key {guid}</param>
        public Client(Uri Endpoint, string EndpointKey)
        {
            this.Endpoint = Endpoint.ToString();
            this.SubscriptionKey = EndpointKey;
        }

        /// <summary>
        /// 取得QnA Services best response
        /// </summary>
        /// <param name="query">User Question</param>
        /// <returns></returns>
        public QnAresponse GetResponse(string query)
        {
            try
            {
                var Endpoint = "";
                QnAresponse res;

                //避免呼叫失敗
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                query = query.Trim();
                if (string.IsNullOrEmpty(this.Endpoint))
                    Endpoint = $"https://{domain}.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/{KnowledgeBaseID}/generateAnswer";
                else
                    Endpoint = this.Endpoint;
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
                wc.Headers.Add("Authorization", $"EndpointKey {SubscriptionKey}");
                string JSON = "{'question':'" + query + "'}";
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(JSON);
                byte[] result = wc.UploadData(Endpoint, byteArray);
                var ret = System.Text.Encoding.UTF8.GetString(result);
                res = Newtonsoft.Json.JsonConvert.DeserializeObject<QnAresponse>(ret);

                return res;
            }
            catch (WebException ex)
            {
                string responseString;
                using (Stream stream = ex.Response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    responseString = reader.ReadToEnd();
                }
                throw new Exception(responseString, ex);
            }
        }
    }

    public class Answer
    {
        public string answer { get; set; }
        public List<string> questions { get; set; }
        public decimal score { get; set; }
    }

    public class QnAresponse
    {
        public List<Answer> answers { get; set; }
    }
}
