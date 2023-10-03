using isRock.LineBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;

namespace test.Controllers
{
    //一組對談訊息
    public class Message
    {
        public DateTime Time { get; set; }
        public string UserID { get; set; }
        public string UserMessage { get; set; }
        public string ResponseMessage { get; set; }
    }

    //對話紀錄處理
    public class ChatHistoryManager
    {
        const string fileName = "messages.json"; //儲存到 IsolatedStorage 的檔案名稱

        /// <summary>
        /// 將所有對話紀錄刪除
        /// </summary>
        public static void DeleteIsolatedStorageFile()
        {
            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (isolatedStorage.FileExists(fileName))
                {
                    isolatedStorage.DeleteFile(fileName);
                }
            }
        }

        /// <summary>
        /// 取得對談紀錄(依照 UserID)
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public static List<Message> GetMessagesFromIsolatedStorage(string UserID)
        {
            var messages = new List<Message>();
            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (isolatedStorage.FileExists(fileName))
                {
                    using (var fileStream = new IsolatedStorageFileStream(fileName, FileMode.Open, isolatedStorage))
                    {
                        using (var reader = new StreamReader(fileStream))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(line);
                                messages.Add(message);
                            }
                        }
                    }
                }
            }

            return messages.Where(c => c.UserID == UserID).OrderBy(c => c.Time).ToList();
        }

        /// <summary>
        /// 儲存對談紀錄到IsolatedStorage
        /// </summary>
        /// <param name="time"></param>
        /// <param name="userID"></param>
        /// <param name="userMessage"></param>
        /// <param name="responseMessage"></param>
        public static void SaveMessageToIsolatedStorage(
            DateTime time, string userID, string userMessage, string responseMessage)
        {
            // 建立 JSON 物件
            var messageObject = new
            {
                Time = time,
                UserID = userID,
                UserMessage = userMessage,
                ResponseMessage = responseMessage
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(messageObject);

            // 讀取 Isolated Storage 中的資料
            List<string> messages = new List<string>();
            var fileName = "messages.json";

            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (isolatedStorage.FileExists(fileName))
                {
                    using (var fileStream = new IsolatedStorageFileStream(fileName, FileMode.Open, isolatedStorage))
                    {
                        using (var reader = new StreamReader(fileStream))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                messages.Add(line);
                            }
                        }
                    }
                }
            }

            // 加上新的 JSON 物件
            messages.Add(json);

            // 寫回 Isolated Storage 中
            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                using (var fileStream = new IsolatedStorageFileStream(fileName, FileMode.Create, isolatedStorage))
                {
                    using (var writer = new StreamWriter(fileStream))
                    {
                        foreach (var message in messages)
                        {
                            writer.WriteLine(message);
                        }
                    }
                }
            }
        }
    }
}
