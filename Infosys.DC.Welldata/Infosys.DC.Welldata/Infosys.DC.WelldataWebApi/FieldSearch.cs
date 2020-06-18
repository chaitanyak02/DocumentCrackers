using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Dynamic;

namespace Infosys.DC.WelldataWebApi
{
    public static class FieldSearch
    {
        [FunctionName("FieldSearch")]
        public static OkObjectResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req,ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                log.LogInformation("Req url1 {1}", req.Headers.ToString());
                log.LogInformation("Req url2 {0}", req.RequestUri.ToString());
                //Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //request.Headers.Add("api-key", _settings.SearchServiceKey);


                var uri = new Uri(req.RequestUri.ToString());
                var baseUri = uri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);

                var query = QueryHelpers.ParseQuery(uri.Query);

                Microsoft.Extensions.Primitives.StringValues key;
                string vals = query.TryGetValue("search",out key).ToString();

                string searchkey = key[0].ToString();
                string val;
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://welldatasearch.search.windows.net/");
                HttpRequestMessage Requestmessage= new HttpRequestMessage();
                if (!searchkey.Contains(":"))
                {
                    val = "indexes/wellindex/docs" + req.RequestUri.Query;
                    Requestmessage = new HttpRequestMessage(HttpMethod.Get, val);
                    Requestmessage.Headers.Add("api-key", "62E684D6674E61A69D7D2137D91AA763");

                    //var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    
                    //var responseResult = JsonConvert.DeserializeObject<ExpandoObject>(content);
                }
                else
                {
                    string requrl =HttpUtility.UrlDecode(req.RequestUri.Query);
                    string[] fields = searchkey.Split(':');
                    string filter = string.Format("&$filter={0}/any(t: t eq {1})", fields[0], fields[1]);
                    val = "indexes/wellindex/docs" + requrl.Replace(searchkey, "'*'") + filter;
                    Requestmessage = new HttpRequestMessage(HttpMethod.Get, val);
                    Requestmessage.Headers.Add("api-key", "62E684D6674E61A69D7D2137D91AA763");
                    //string param = "{'count': true, 'select': '*','skip': 0,'top': 20,'searchMode': 'all','queryType': 'full','search':" + searchkey + "}";
                    //val = "indexes/wellindex/docs/search?api-version=2017-11-11-Preview";
                    //Requestmessage = new HttpRequestMessage(HttpMethod.Post, val);
                    //Requestmessage.Headers.Add("api-key", "62E684D6674E61A69D7D2137D91AA763");

                    //string docsCollection = JsonConvert.SerializeObject(param);
                    //Requestmessage.Content = new StringContent(docsCollection, Encoding.UTF8, "application/json");
                }



                HttpResponseMessage response = client.SendAsync(Requestmessage).GetAwaiter().GetResult();
                var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var value = JsonConvert.DeserializeObject<dynamic>(content);

                log.LogInformation("Status {0}", response.StatusCode);
                log.LogInformation("Content {0}", response.Content.ToString());
                return new OkObjectResult(value);
               // return req.CreateResponse(HttpStatusCode.OK, response.Content);
            }
            catch(Exception ex)
            {
                log.LogInformation("Error {0}", ex.Message.ToString());
                throw;
                //return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message.ToString());
            }
        }
    }
}
