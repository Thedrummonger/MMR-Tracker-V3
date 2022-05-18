using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class PasteBinApi
    {
        private const string _apiPostUrl = "https://pastebin.com/api/api_post.php";

        public string SetAPIKey(Func<string> GetAPIKeyFunc)
        {
            var Key = GetAPIKeyFunc();
            if (Key is null) { return null; }
            if (!File.Exists(References.Globalpaths.UserData))
            { 
                var newfile = File.Create(References.Globalpaths.UserData);
                newfile.Close();
                File.WriteAllText(References.Globalpaths.UserData, Newtonsoft.Json.JsonConvert.SerializeObject(new MiscData.userData { PastebinAPIKey = Key }));
            }
            else
            {
                MiscData.userData userData = Newtonsoft.Json.JsonConvert.DeserializeObject<MiscData.userData>(File.ReadAllText(References.Globalpaths.UserData));
                if (userData is null) { userData = new MiscData.userData(); }
                userData.PastebinAPIKey = Key;
                File.WriteAllText(References.Globalpaths.UserData, Newtonsoft.Json.JsonConvert.SerializeObject(userData));
            }
            return Key;
        }

        public string GetAPIKey(Func<string> GetAPIKeyFunc)
        {
            if (ReadAPIKeyFromUserFile() is not null) { return ReadAPIKeyFromUserFile(); }
            else
            {
                var APIKey = SetAPIKey(GetAPIKeyFunc);
                if (APIKey is not null) { return APIKey; }
            }
            return null;
        }
        public string ReadAPIKeyFromUserFile()
        {
            if (File.Exists(References.Globalpaths.UserData))
            {
                MiscData.userData userData = Newtonsoft.Json.JsonConvert.DeserializeObject<MiscData.userData>(File.ReadAllText(References.Globalpaths.UserData));
                if (userData is null) { return null; }
                if (userData.PastebinAPIKey is not null) { return userData.PastebinAPIKey; }
            }
            return null;
        }

        public string CreatePaste(string Paste, Func<string> GetAPIKeyFunc)
        {
            var Key = GetAPIKey(GetAPIKeyFunc);
            if (Key is null) { return "Paste Failed. Could not get API Key"; }
            NameValueCollection args = new NameValueCollection();
            args["api_dev_key"] = Key;
            args["api_option"] = "paste";
            args["api_paste_code"] = Paste;

            try
            {
                WebClient wb = new WebClient();
                byte[] bytes = wb.UploadValues("https://pastebin.com/api/api_post.php", args);
                string resp = ReadWebResponse(bytes);
                return resp;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetPaste(string Code)
        {
            string URL = $"https://pastebin.com/raw/{Code}";
            WebClient wc = new WebClient();
            try
            {
                string Paste = wc.DownloadString(URL);
                return Paste;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private static string ReadWebResponse(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            using (var reader = new StreamReader(ms)) { return reader.ReadToEnd(); }
        }
    }
}
