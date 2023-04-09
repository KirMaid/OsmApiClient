using NUnit.Framework;
/*using OsmSharp.IO.API;*/
using WebApplication;
using System.Threading.Tasks;
/*using OsmSharp.IO.API.Overpass;
using OsmSharp.API;*/
using System.Net.Http;
using System.Net;
/*using OsmSharp.Streams;
using OsmSharp;*/
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
        public void ParseXmlBuildings()
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

            foreach (var way in list)
            {
                Console.WriteLine(way.Center.Longitude);
            }
        }


        [Test]
        public void ParseXmlShops()
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

            foreach (var way in ways)
            {
                Console.WriteLine(way.Center.Longitude);
            }
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

/*    public class Node
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public List<Tag> Tags { get; set; }
        public double Longitude { get; set; }
    }*/

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