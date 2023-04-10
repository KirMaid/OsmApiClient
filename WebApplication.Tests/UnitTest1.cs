using NUnit.Framework;
using WebApplication;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using System.Globalization;

namespace WebApplication.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        private const string WayName = "way";
        private const string NodeName = "node";

        [Test]
        public Double calculateTheDistance(Center c1, Center c2)
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
            var dist = ad* 6372795;
            return dist;
        }


        [Test]
        public List<HeatmapElement> CalkHeatmap(List<Node> addreses, List<Node> filter)
        {
            var list = new List<HeatmapElement>();
            foreach(Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                var score = 0.25;
                foreach (Node nodeFilter in filter)
                {
                    var dist = calculateTheDistance(node.Center, nodeFilter.Center);
                    if (dist < 1000 && score < 1)
                    {
                        score += 0.25;
                    }
                }
                heatmapElement.Coefficient = score;
                list.Add(heatmapElement);
            }
            return list;
        }

            [Test]
        public List<Node> ParseXmlBuildings()
        {
            var queryAllLiveBuildings1 = "area[name = \"Волгоград\"];(node[\"building\" = \"apartments\"](area);node[\"building\" = \"detached\"](area);node[\"building\" = \"house\"](area);); out center; out;";
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


        [Test]
        public List<Node> ParseXmlShops()
        {
            var queryAllLiveBuildings1 = "area[name = \"Волгоград\"];(node[\"shop\" = \"department_store\"](area);node[\"shop\" = \"general\"](area);node[\"shop\" = \"mall\"](area);node[\"shop\" = \"supermarket\"](area);node[\"shop\" = \"convenience\"](area);node[\"shop\" = \"department_store\"](area);node[\"shop\" = \"general\"](area);node[\"shop\" = \"mall\"](area);node[\"shop\" = \"supermarket\"](area);node[\"shop\" = \"convenience\"](area);); out center; out;";
            var request = WebRequest.Create(
                "https://overpass-api.de/api/interpreter?data=" + queryAllLiveBuildings1);
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
                        center.Latitude = double.Parse(dictCoord["lat"],formatter);
                        center.Longitude = double.Parse(dictCoord["lon"], formatter);
                        break;
                    case "tag":
                        var dict = AttributesToDictionary(reader);
                        tags.Add(new Tag() {K = dict["k"], V = dict["v"]});
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
        public double Coefficient{ get; set; }
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