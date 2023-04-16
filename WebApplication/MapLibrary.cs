using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
/*using OsmSharp;
using OsmSharp.API;
using OsmSharp.IO.API;*/

namespace WebApplication
{
    public class MapLibrary
    {
        private const string WayName = "way";
        private const string NodeName = "node";

        public List<HeatmapElement> calculateHeatmap()
        {
            var buildings = ParseXmlBuildings();
            var shops = ParseXmlShops();
            return CalkHeatmap(ParseXmlBuildings(), ParseXmlShops());
        }

        public double calculateTheDistance(Center c1, Center c2)
        {
            // перевести координаты в радианы
            var lat1 = c1.Latitude * Math.PI / 180;
            var lat2 = c2.Latitude * Math.PI / 180;
            var long1 = c1.Longitude * Math.PI / 180;
            var long2 = c2.Longitude * Math.PI / 180;

            // косинусы и синусы широт и разницы долгот
            var cl1 = Math.Cos(lat1);
            var cl2 = Math.Cos(lat2);
            var sl1 = Math.Sin(lat1);
            var sl2 = Math.Sin(lat2);
            var delta = long2 - long1;
            var cdelta = Math.Cos(delta);
            var sdelta = Math.Sin(delta);

            // вычисления длины большого круга
            var y = Math.Sqrt(Math.Pow(cl2 * sdelta, 2) + Math.Pow(cl1 * sl2 - sl1 * cl2 * cdelta, 2));
            var x = sl1 * sl2 + cl1 * cl2 * cdelta;

            var ad = Math.Atan2(y, x);
            var dist = ad * 6372795;
            return dist;
        }

        public List<HeatmapElement> CalkHeatmap(List<Node> addreses, List<Node> filter)
        {
            var list = new List<HeatmapElement>();
            foreach (Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                var score = 0.0;
                foreach (Node nodeFilter in filter)
                {
                    var dist = calculateTheDistance(node.Center, nodeFilter.Center);
                    if (dist < 200 && score < 1)
                    {
                        if(score > 0.75)
                            score = 1;
                        else
                            score += 0.25;
                    }
                    if (dist < 500 && score < 1)
                    {
                        if (score > 0.9)
                            score = 1;
                        else
                            score += 0.1;
                    }
                }
                heatmapElement.Coefficient = score;
                list.Add(heatmapElement);
                score = 0.0;
            }
            return list;
        }

        public List<Node> ParseXmlBuildings()
        {
            var queryAllLiveBuildings1 = "area[name = \"Волгоград\"];" +
                "(" +
                "way[\"building\" = \"apartments\"](area);" +
                "way[\"building\" = \"detached\"](area);" +
                "way[\"building\" = \"house\"](area);" +
                "); " +
                "out center; " +
                "out;";
            var request = WebRequest.Create(
                "https://overpass-api.de/api/interpreter?data=" + queryAllLiveBuildings1);
            request.Method = "GET";
            var requestStream = request.GetResponse().GetResponseStream();
            var list = new List<Node>();
            using (var reader = XmlReader.Create(requestStream))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == WayName)
                            {
                                var way = GetWay(reader);
                                list.Add(way);
                            }
                            break;
                    }
                }
            }
            return list;
        }

        public List<Node> ParseXmlShops()
        {
            var queryAllLiveBuildings = "area[name=\"Волгоград\"];" +
                "(" +
                "node[\"shop\" ~ \"supermarket|convenience|mall|general|department_store\"](area);" +
                "way[\"shop\" ~ \"supermarket|convenience|mall|general|department_store\"](area);" +
                "); " +
                "out center; " +
                "out;";

            var request = WebRequest.Create(
                "https://overpass-api.de/api/interpreter?data=" + queryAllLiveBuildings);
            request.Method = "GET";
            var requestStream = request.GetResponse().GetResponseStream();
            var ways = new List<Node>();
            using (var reader = XmlReader.Create(requestStream))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == WayName)
                            {
                                var way = GetWay(reader);
                                ways.Add(way);
                            }
                            if (reader.Name == NodeName)
                            {
                                var way = GetNode(reader);
                                ways.Add(way);
                            }
                            break;
                    }
                }
            }
            return ways;
        }


        private static Node GetWay(XmlReader reader)
        {
            var attributes = AttributesToDictionary(reader);
            var tags = new List<Tag>();
            Center center = new Center();
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

            while (reader.Read() && reader.Name != WayName)
            {
                switch (reader.Name)
                {
                    case "center":
                        var dictCoord = AttributesToDictionary(reader);
                        center.Latitude = double.Parse(dictCoord["lat"], formatter);
                        center.Longitude = double.Parse(dictCoord["lon"], formatter);
                        break;
                    case "tag":
                        var dict = AttributesToDictionary(reader);
                        tags.Add(new Tag() { K = dict["k"], V = dict["v"] });
                        break;
                }
            }

            return new Node
            {
                Id = Convert.ToInt64(attributes["id"]),
                Tags = tags,
                Center = center
            };
        }

        private static Node GetNode(XmlReader reader)
        {
            var attributes = AttributesToDictionary(reader);
            Center center = new Center();
            var tags = new List<Tag>();
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

            while (reader.Read() && reader.Name != NodeName)
            {
                switch (reader.Name)
                {
                    case "tag":
                        var dict = AttributesToDictionary(reader);
                        tags.Add(new Tag() { K = dict["k"], V = dict["v"] });
                        break;
                }
            }

            center.Latitude = double.Parse(attributes["lat"], formatter);
            center.Longitude = double.Parse(attributes["lon"], formatter);
            return new Node
            {
                Id = Convert.ToInt64(attributes["id"]),
                Center = center,
                Tags = tags
            };
        }

        private static Dictionary<string, string> AttributesToDictionary(XmlReader reader)
        {
            var attributes = new Dictionary<string, string>();
            while (reader.MoveToNextAttribute())
            {
                attributes.Add(reader.Name.ToLower(), reader.Value);
            }
            return attributes;
        }
    }

    public class Node
    {
        public long Id { get; set; }
        public List<Tag> Tags { get; set; }
        public Center Center { get; set; }
    }

    public class HeatmapElement
    {
        public Center Center { get; set; }
        public double Coefficient { get; set; }
    }

    public class Tag
    {
        public string K { get; set; }
        public string V { get; set; }
    }

    public class Center
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
