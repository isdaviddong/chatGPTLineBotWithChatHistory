using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Web;

namespace isRock.Template
{
    public class LineComputerVisionWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        const string key = "f330xxxxd49341xxxxdc7e7396xxxx7a";
        const string endpoint = "https://southeastasia.api.cognitive.microsoft.com/";

        [Route("api/ComputerVision")]
        [HttpPost]
        public IActionResult POST()
        {
            var AdminUserId = "_______U5e60294b8c__AdminUserId__02d6295b621a_____";

            try
            {
                //設定ChannelAccessToken
                this.ChannelAccessToken = "_____________ChannelAccessToken___________________";
                //配合Line Verify
                if (ReceivedMessage.events == null || ReceivedMessage.events.Count() <= 0 ||
                    ReceivedMessage.events.FirstOrDefault().replyToken == "00000000000000000000000000000000") return Ok();
                //取得Line Event
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                var responseMsg = "";
                //準備回覆訊息
                if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "image")
                {
                    var Bytes = this.GetUserUploadedContent(LineEvent.message.id);
                    var ret = MakeRequest(endpoint, key, Bytes);
                    responseMsg = $"captions : {ret.description.captions[0].text} \n JSON: {ret}";
                }
                else if (LineEvent.type.ToLower() == "message")
                    responseMsg = $"收到 event : {LineEvent.type} type: {LineEvent.message.type} ";
                else
                    responseMsg = $"收到 event : {LineEvent.type} ";
                //回覆訊息
                this.ReplyMessage(LineEvent.replyToken, responseMsg);
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //回覆訊息
                this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }

        static dynamic MakeRequest(string endpoint, string subscriptionKey, byte[] byteData)
        {
            HttpClient client = new HttpClient();
            string uriBase = endpoint + "vision/v2.1/analyze";

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

            string requestParameters =
                "visualFeatures=Description";

            // Assemble the URI for the REST API method.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            // Add the byte array as an octet stream to the request body.
            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses the "application/octet-stream" content type.
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                // Asynchronously call the REST API method.
                response = client.PostAsync(uri, content).Result;
            }

            // Asynchronously get the JSON response.
            string JSON = response.Content.ReadAsStringAsync().Result;

            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(JSON);
        }
    }
}