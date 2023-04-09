using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
/*using OsmSharp;
using OsmSharp.API;
using OsmSharp.IO.API;*/

namespace WebApplication
{
    public class MapLibrary
    {

       /* public static readonly Bounds Volgograd = new Bounds()
        {
            MinLongitude = 44.56869230f,
            MaxLongitude = 44.64228496f,
            MinLatitude = 48.47511490f,
            MaxLatitude = 48.75869656f

 *//*           MinLongitude = -77.0671918f,//44.64228496f
            MinLatitude = 38.9007186f,//48.75869656f
            MaxLongitude = -77.00099990f,//44.56869230f
            MaxLatitude = 38.98734f //48.47511490f*//*
        };*/
      /*  public static async Task<OsmGeo[]> getAllAddressAsync()
        {
        var clientFactory = new ClientsFactory(null, new HttpClient(),
        "https://master.apis.dev.openstreetmap.org/api/");
        var client = clientFactory.CreateNonAuthClient();
            
        var map = await client.GetMap(Volgograd);
            return await client.GetElements(
                map.Nodes.Select(n => new OsmGeoKey(n)).ToArray());
            //map.Nodes.Select(n => new OsmGeoKey(n)).Concat(map.Ways.Select(n => new OsmGeoKey(n))).ToArray());
        }*/
    }
}
