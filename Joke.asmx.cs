using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace JohnNorrisService
{
    /// <summary>
    /// Gets random name from http://uinames.com/api/
    /// Gets random joke from http://api.icndb.com/jokes/random using the random name
    /// </summary>
     
    // TODO change Namespace to production namespace
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    [System.Web.Script.Services.ScriptService]
    public class Joke : System.Web.Services.WebService
    {
        static readonly string RANDOM_NAME_URL = @"http://uinames.com/api/";
        static readonly string RANDOM_JOKE_URL = @"http://api.icndb.com/jokes/random";

        [WebMethod]
        public Response JohnNorris()
        {
            Response response = new Response();
            response.type = "success";
            response.joke = GetJoke();

            return response;
        }

        private string GetJoke()
        {
            RandomName name = GetRandomName();
            string joke = GetRandomJoke(name);

            return joke;
        }

        private string GetRandomJoke(RandomName name)
        {
            string url = String.Format("{0}?firstName={1}&lastName={2}&limitTo=[nerdy]", RANDOM_JOKE_URL, name.name, name.surname);
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "text/html";

            HttpWebResponse response = TryGetResponse(request);

            return TryParseJoke(response);
        }

        private string TryParseJoke(HttpWebResponse response)
        {
            try
            {
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                JObject jo = JObject.Parse(responseString);
                string joke = (string)jo["value"]["joke"];

                return joke;
            }
            catch (NullReferenceException e)
            {
                return @"Response from api.icndb.com/jokes was Null";
            }
        }

        private HttpWebResponse TryGetResponse(WebRequest request)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private RandomName GetRandomName()
        {
            using (WebClient client = new WebClient())
            {
                string nameJson = client.DownloadString(RANDOM_NAME_URL);
                RandomName randomName = JsonConvert.DeserializeObject<RandomName>(nameJson);
                
                return randomName;
            }
        }

        private struct RandomName
        {
            public string name;
            public string surname;
        }

        public struct Response
        {
            public string type;
            public string joke;
        }
    }
}
