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
    public class LineTranslatorWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        const string subscriptionKey = "519fdc4xxxxxxxxxxxxxxxxxeee334";
        const string region = "eastasia";
        const string endpoint = "https://api.cognitive.microsofttranslator.com";

        [Route("api/TranslatorBot")]
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
                if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "text")
                {
                    var ret = MakeTranslator(LineEvent.message.text);
                    responseMsg = $"您說了 {LineEvent.message.text}";
                    foreach (var item in ret[0].translations)
                    {
                        responseMsg += $"\n 翻譯成 {item.to} --> {item.text}";
                    }
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

        static dynamic MakeTranslator(string msg)
        {
            HttpClient client = new HttpClient();
            string uri = endpoint + "/translate?api-version=3.0&to=ja&to=en&to=ko";

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);
            client.DefaultRequestHeaders.Add(
           "Ocp-Apim-Subscription-Region", region);

            var JsonString = "[{\"text\" : \"" + msg + "\"}]";
            var content =
               new StringContent(JsonString, System.Text.Encoding.UTF8, "application/json");
            var response = client.PostAsync(uri, content).Result;
            var JSON = response.Content.ReadAsStringAsync().Result;
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(JSON);
        }
    }
}