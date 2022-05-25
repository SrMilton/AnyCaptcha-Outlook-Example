
/*
Create your selenium session and answers everything till funcaptcha appears in the screen
*/

Console.WriteLine("FunCaptcha appears..");

string token = await anycaptchaSolve(driver.Url); //You need to pass the actual URL to anycaptcha solve it

string jsscript = "var anyCaptchaToken = '" + token + "';" +
                    "var enc = document.getElementById('enforcementFrame');" +
                    "var encWin = enc.contentWindow || enc; " +
                    "var encDoc = enc.contentDocument || encWin.document; " +
                    "let script = encDoc.createElement('SCRIPT'); " +
                    "script.append('function AnyCaptchaSubmit(token) { parent.postMessage(JSON.stringify({ eventId: \"challenge-complete\", payload: { sessionToken: token } }), \"*\") }'); " +
                    "encDoc.documentElement.appendChild(script); " +
                    "encWin.AnyCaptchaSubmit(anyCaptchaToken);"; //This code will submit your captcha with solved token

IJavaScriptExecutor js = (IJavaScriptExecutor)drive;
js.ExecuteScript(jsscript); //Then execute the JS

/*
After this you'll be ready to continue your registration, captcha is already solved
*/


public static async Task<string> anycaptchaSolve(string pageurl)
        {

            /*
            This part will submit our captcha to AnyCaptcha solution
            */
            string id = "";
            try
            {
                var parser = new FileIniDataParser();
                IniData data2 = parser.ReadFile("config.ini");
                string apikey = data2["apikey"]["anycaptcha"]; //My APIKEY is on a INI File, you can change this to hardcode if you want

                string url = "https://api.anycaptcha.com/createTask";
                var baseAddress = new Uri(url);
                var cookieContainer = new CookieContainer();
                using (var handler2 = new HttpClientHandler { UseCookies = false })
                using (var client2 = new HttpClient(handler2) { BaseAddress = baseAddress })
                {
                    string str = "{ \"clientKey\": \""+apikey+"\",\"task\": { \"type\": \"FunCaptchaTaskProxyless\", \"websitePublicKey\": \"B7D8911C-5CC8-A9A3-35B0-554ACEE604DA\",  \"websiteURL\": \"" + pageurl + "\" } }";//ps: this "websitePublicKey" is from outlook. If you are using other service you'll need to change this.
                    JObject json = JObject.Parse(str); 

                    var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                    var result = client2.PostAsync("", content).Result;
                    string responseString = await result.Content.ReadAsStringAsync();
                    dynamic jsonemail = JObject.Parse(responseString);
                    id = jsonemail.taskId;
                    Console.WriteLine(responseString);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            /*
            Here we'll wait for our solved funcaptcha token
            */

            TryAgain:
            try
            {
                var parser = new FileIniDataParser();
                IniData data2 = parser.ReadFile("config.ini");
                string apikey = data2["apikey"]["anycaptcha"];//My APIKEY is on a INI File, you can change this to hardcode if you want

                string url = "https://api.anycaptcha.com/getTaskResult";
                var baseAddress = new Uri(url);
                var cookieContainer = new CookieContainer();
                using (var handler2 = new HttpClientHandler { UseCookies = false })
                using (var client2 = new HttpClient(handler2) { BaseAddress = baseAddress })
                {

                    var values = new Dictionary<string, string>
                    {
                        {"clientKey", apikey},
                        {"taskId", id}
                    };
                    var content = new FormUrlEncodedContent(values);

                    //Colocar cookies dinamicamente aqui
                    client2.DefaultRequestHeaders.Clear();
                    var result = client2.PostAsync("", content).Result;
                    string responseString = await result.Content.ReadAsStringAsync();

                    dynamic jsonemail = JObject.Parse(responseString);
                    Console.WriteLine(responseString);
                    string status = jsonemail.status;
                    if(status.Equals("processing") == true)//If processing, try again..
                    {
                        goto TryAgain;
                    }
                    string token = jsonemail.solution.token;
                    return token;//If ready, gotcha
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return "";
        }
