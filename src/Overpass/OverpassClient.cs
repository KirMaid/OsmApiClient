using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OsmSharp.IO.API.Overpass
{
    public class OverpassClient
    {

        static IEnumerable<string> BaseUrls = new List<string>()
        {
            "https://maps.mail.ru/osm/tools/overpass/api/interpreter"
            // ...
        };

        HttpClient Client { get; } = new HttpClient();

        public Task<string> RequestJsonAsync(string overpassQuery)
        {

            var baseUrl = BaseUrls.GetEnumerator().Current;

            var url = baseUrl + overpassQuery;

            try
            {
                var json = Client.GetStringAsync(url);
                return json;
            }
            catch(Exception exc)
            {
                var hasNext = BaseUrls.GetEnumerator().MoveNext();

                if(!hasNext)
                {
                    return null;
                }

                return RequestJsonAsync(overpassQuery);
            }

        }
    }
}
