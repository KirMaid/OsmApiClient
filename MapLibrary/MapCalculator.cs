using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Xml;

namespace MapLibrary
{
    public class MapCalculator
    {
        private string City;
        private bool withBD = false;
        private OverpassAPIConnector overpassAPI;
        private OpenStreetXmlParser xmlParser;
        public DistanceParams filterParams;

        public MapCalculator(
            string apiAddress = "maps.mail.ru/osm/tools/overpass",
            string City = null
            )
        {
            this.overpassAPI = new OverpassAPIConnector(apiAddress);
            this.City = City;
            this.xmlParser = new OpenStreetXmlParser(this.overpassAPI);
        }

        public void setApiAddress(string apiAddress)
        {
            overpassAPI.setApiAddress(apiAddress);
        }

        public void setCity(string City)
        {
            this.City = City;
        }

        public OverpassAPIConnector getApiConnector()
        {
            return overpassAPI;
        }

        public OpenStreetXmlParser getXmlParser()
        {
            return xmlParser;
        }

        public List<string> buildingsTags = new List<string>()
        {
            "apartments",
            "detached",
            "house",
        };

        Dictionary<string, Dictionary<List<string>, List<string>>> dictTagsAll = new Dictionary<string, Dictionary<List<string>, List<string>>>() {
            { "shop",
                new Dictionary<List<string>, List<string>>() {
                    [new List<string>() { "shop" }] = new List<string>() { "supermarket", "convenience", "mall", "general", "department_store" }
                }
            },
            { "school",
                new Dictionary<List<string>, List<string>>()
                {
                    [new List<string>() { "amenity" }] = new List<string>() { "school" },
                }
            },
            {
              "hospital",
               new Dictionary<List<string>, List<string>>()
               {
                   //[new List<string>() {"building"}] = new List<string>() { "clinic", "hospital" },
                   [new List<string>() {"amenity"}] = new List<string>() { "clinic", "hospital" },
               }
            },
            {
               "kindergarten",
               new Dictionary<List<string>, List<string>>()
               {
                   //[new List<string>() {"building"}] = new List<string>() { "kindergarten" },
                   [new List<string>() {"amenity"}] = new List<string>() { "kindergarten" },

               }
            },
             {
               "bus_stop",
               new Dictionary<List<string>, List<string>>()
               {
                   [new List<string>() {"highway"}] = new List<string>() { "bus_stop" },

               }
            },
        };

        public Dictionary<string, Dictionary<List<string>, List<string>>> dictTagsChosen = new Dictionary<string, Dictionary<List<string>, List<string>>>() { };
      
        /// <summary>
        /// Служит для ручного добавления значений в фильтр значений (Для разработчиков)
        /// </summary>
        private void addTag(string nameTag, Dictionary<List<string>, List<string>> keyAndValues)
        {
            dictTagsAll.Add(nameTag, keyAndValues);
        }

        /// <summary>
        /// Метод выставляет переменную фильтра
        /// </summary>
        /// <param name="nameFilter"> Название категорий фильтра.
        /// Существующие категории:
        /// shop
        /// school
        /// hospital
        /// kindergarten
        /// </param>
        /// 
        public void setFilter(List<string> nameFilters)
        {
            foreach (var el in nameFilters)
            {
                if (!dictTagsAll.ContainsKey(el))
                {
                    throw new Exception("Такой категории фильтра не существует");
                }
                else
                {
                    dictTagsChosen.Add(el, dictTagsAll[el]);
                    filterParams.countFilters +=  1;
                }
            }
        }

        public void setDistanceAndCount(int distance,int count, int minCount = 0)
        {
            this.filterParams = new DistanceParams(distance, count, minCount);
        }

        public void setLevels(int minLevel, int maxLevel)
        {
            this.filterParams = new DistanceParams(maxLevel,minLevel);
        }


        public HeatmapElements calculateHeatmap()
        {
            return CalkHeatmap(xmlParser.ParseXmlBuildings(City,buildingsTags), xmlParser.ParseXmlFilter(City,dictTagsChosen), filterParams);
        }

        public HeatmapElements calculatePopulationDensity()
        {
            return CalkPopulationDensity(xmlParser.ParseXmlBuildings(City, buildingsTags), filterParams);
        }

        public HeatmapElements calculateNumberOfStoreys()
        {
            return CalkNumberOfStoreys(xmlParser.ParseXmlBuildings(City, buildingsTags), filterParams);
        }

        public HeatmapElements CalkPopulationDensity(List<Node> addreses, DistanceParams filterParams) 
        {
            var list = new List<HeatmapElement>();
            foreach (Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                int.TryParse(
                node?.Tags?.Find(x => x.K == "building:levels")?.V ?? string.Empty,
                out int levels
                );
                if (levels > filterParams.minCount && levels < filterParams.countObjects)
                    heatmapElement.Coefficient = levels*10*3;
                else
                    heatmapElement.Coefficient = 0;
                list.Add(heatmapElement);
            }
            return new HeatmapElements(list);
        }
        public HeatmapElements CalkNumberOfStoreys(List<Node> addreses, DistanceParams filterParams)
        {
            var list = new List<HeatmapElement>();
            foreach (Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                int.TryParse(
                node?.Tags?.Find(x => x.K == "building:levels")?.V ?? string.Empty,
                out int levels
                );
                if (levels > filterParams.minCount && levels < filterParams.countObjects)
                    heatmapElement.Coefficient = levels;
                else
                    heatmapElement.Coefficient = 0;
                list.Add(heatmapElement);
            }
            return new HeatmapElements(list);
        }

        public double calculateTheDistance(Coordinate c1, Coordinate c2)
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

        public HeatmapElements CalkHeatmapParams(List<Node> addreses, List<Node> filter, List<DistanceParams> filterParams)
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
                    foreach (DistanceParams filterParam in filterParams)
                    {
                        if (dist < filterParam.distance)
                        {
                            if (score > (1 - filterParam.coefficient))
                            {
                                score = 1;
                            }
                            else
                                score += filterParam.coefficient;
                        }
                    }
                }
                heatmapElement.Coefficient = score;
                list.Add(heatmapElement);
                score = 0.0;
            }
            return new HeatmapElements(list);
        }

        /// <summary>
        /// Внутренняя функция для расчёта тепловой карты
        /// </summary>
        /// <returns>Массив точек с коэффициентом тепловой карты</returns>
        public HeatmapElements CalkHeatmap(List<Node> addreses, List<Node> filter, DistanceParams filterParams)
        {
            var list = new List<HeatmapElement>();
            foreach (Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                var score = 0.0;
                var coef = 1.0 / (filterParams.countFilters * filterParams.countObjects);
                var minCoef = coef * filterParams.minCount;
                foreach (Node nodeFilter in filter)
                {
                    var dist = calculateTheDistance(node.Center, nodeFilter.Center);
                    if (dist < filterParams.distance)
                    {
                        if (score > (1 - coef))
                        {
                            score = 1;
                        }
                        else
                            score += coef;
                    }
                }
                if(score < minCoef)
                    heatmapElement.Coefficient = 0;
                else
                    heatmapElement.Coefficient = score;
                list.Add(heatmapElement);
                score = 0.0;
            }
            return new HeatmapElements(list);
        }

        /// <summary>
        /// Внутренняя функция для расчёта реверсивной тепловой карты
        /// </summary>
        /// <returns>Массив точек с коэффициентом тепловой карты</returns>
        public HeatmapElements CalkHeatmapReverse(List<Node> addreses, List<Node> filter, DistanceParams filterParams)
        {
            var list = new List<HeatmapElement>();
            foreach (Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                var score = 1.0;
                var coef = 1 / (filterParams.countFilters * filterParams.countObjects);
                foreach (Node nodeFilter in filter)
                {
                    var dist = calculateTheDistance(node.Center, nodeFilter.Center);
                    if (dist < filterParams.distance)
                    {
                        if (score < coef)
                        {
                            score = 0;
                        }
                        else
                            score -= coef;
                    }
                }
                heatmapElement.Coefficient = score;
                list.Add(heatmapElement);
                score = 1.0;
            }
            return new HeatmapElements(list);
        }
    }

    public class Node
    {
        public long Id { get; set; }
        public List<Tag> Tags { get; set; }
        public Coordinate Center { get; set; }
    }

    public class HeatmapElement
    {
        public Coordinate Center { get; set; }
        public double Coefficient { get; set; }
    }

    public class Tag
    {
        public string K { get; set; }
        public string V { get; set; }
    }

    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class DistanceParams
    {
        public DistanceParams(int distance, int count, int minCount = 0)
        {
            this.distance = distance;
            this.countObjects = count;
            this.minCount = minCount;
        }

        public DistanceParams(int count, int minCount = 0)
        {
            this.countObjects = count;
            this.minCount = minCount;
        }
        public int distance = 0;
        public double coefficient;
        public int countObjects;
        public int countFilters = 0;
        public int minCount;
    }

    public class HeatmapElements
    {
        private List<HeatmapElement> elements;
        public HeatmapElements(List<HeatmapElement> elements)
        {
            this.elements = elements;
        }
        public void getCSV(string pathCsvFile = "D:\\test.csv")
        {
            try
            {
                using (var writer = new StreamWriter(pathCsvFile))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(elements);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public string getJSON()
        {
            return JsonSerializer.Serialize(elements);
        }

        public List<HeatmapElement> getList()
        {
            return elements;
        }

    }

    public class OverpassAPIConnector
    {
        private string apiAddress;
        private List<string> filterKey;
        public OverpassAPIConnector(string apiAddress)
        {
            this.apiAddress = apiAddress;
        }

        public Stream createRequest(string request)
        {
            var webRequest = WebRequest.Create(request);
            webRequest.Method = "GET";
            return webRequest.GetResponse().GetResponseStream();
        }

        public void setApiAddress(string apiAddress)
        {
            this.apiAddress = apiAddress;
        }

        /// <summary>
        /// Построитель строки запроса фильтра к Overpass API
        /// </summary>
        /// <param name="dict">Массив с тегами для выборки</param>
        /// <returns>Строка запроса</returns>
        public string BuildQueryFilter(string City, Dictionary<string, Dictionary<List<string>, List<string>>> dictTags)
        {
            string str = "https://" + apiAddress + "/api/interpreter?data=" + $"area[name = \"{City}\"];(";
            string endStr = ");out center;out;";
            foreach (var el in dictTags)
            {
                str += QueryFilter(el.Value);
            }
            str = str + endStr;
            return str;
        }

        /// <summary>
        /// Построитель запроса для одной категории фильтра
        /// </summary>
        /// <param name="dict">Теги для фильтра</param>
        /// <returns></returns>
        public string QueryFilter(Dictionary<List<string>, List<string>> dict)
        {
            string str = "";
            foreach (var el in dict)
            {
                string nodeWayStr = "";
                foreach (string listKey in el.Key)
                {
                    nodeWayStr += $"[\"{listKey}\" ~ \"";
                    foreach (string listValue in el.Value)
                    {
                        nodeWayStr += listValue + "|";
                    }
                    nodeWayStr = nodeWayStr.Remove(nodeWayStr.Length - 1, 1);
                    nodeWayStr += "\"](area);";
                    str += "node" + nodeWayStr;
                    str += "way" + nodeWayStr;
                }
            }
            return str;
        }

        /// <summary>
        /// Построитель строки запроса зданий к Overpass API
        /// </summary>
        /// <returns>Строка запроса</returns>
        public string BuildQueryCity(string City, List<string> buildingsTags)
        {
            string str = "https://" + apiAddress + "/api/interpreter?data=" + $"area[name = \"{City}\"];(";
            string endStr = ");out center;out;";
            foreach (var el in buildingsTags)
            {
                str += $"way[\"building\" = \"{el}\"](area);";
            }
            str = str + endStr;
            return str;
        }

        /// <summary>
        /// Построитель строки запроса зданий к Overpass API
        /// </summary>
        /// <returns>Строка запроса</returns>
        public string BuildQueryCityPolygon(string City)
        {
            return "https://" + apiAddress + "/api/interpreter?data=" + $"area[name = \"{City}\"];(node[\"boundary\"=\"administrative\"][\"admin_level\"=\"6\"](area); way[\"boundary\"=\"administrative\"][\"admin_level\"=\"6\"](area);relation[\"boundary\"=\"administrative\"][\"admin_level\"=\"6\"](area););out center;out;";
        }
    }

    public class OpenStreetXmlParser{
        private const string WayName = "way";
        private const string NodeName = "node";
        private OverpassAPIConnector overpassAPI;

        public OpenStreetXmlParser(OverpassAPIConnector overpassAPI)
        {
            this.overpassAPI = overpassAPI;
        }

        /// <summary>
        /// Функция получает из XML область и преобразует её в точку со следующими параметрами: id, координаты центра и все теги.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Объект точки</returns>
        private static Node GetWay(XmlReader reader)
        {
            var attributes = AttributesToDictionary(reader);
            var tags = new List<Tag>();
            Coordinate center = new Coordinate();
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

        /// <summary>
        /// Функция получает из XML точку и её основные параметры: id, координаты центра и все теги.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Объект точки</returns>
        private static Node GetNode(XmlReader reader)
        {
            var attributes = AttributesToDictionary(reader);
            Coordinate center = new Coordinate();
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

        /// <summary>
        /// Функция
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Список атрибутов</returns>
        private static Dictionary<string, string> AttributesToDictionary(XmlReader reader)
        {
            var attributes = new Dictionary<string, string>();
            while (reader.MoveToNextAttribute())
            {
                attributes.Add(reader.Name.ToLower(), reader.Value);
            }
            return attributes;
        }

        public List<Node> ParseXmlBuildings(string City, List<string> buildingsTags)
        {
            var requestStream = overpassAPI.createRequest(overpassAPI.BuildQueryCity(City, buildingsTags));
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

        public List<Node> ParseXmlFilter(string City, Dictionary<string, Dictionary<List<string>, List<string>>> dictTags)
        {
            var requestStream = overpassAPI.createRequest(overpassAPI.BuildQueryFilter(City, dictTags));
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
    }

    //Класс для работы с полигонами, предназначет для разбиения области на полигоны
    public class Polygon
    {
        public bool isPointInsidePolygon(List<Coordinate> coordinates,Coordinate coordinate)
        {
            int i1, i2, n, S1, S2, S3;
            double S;
            bool flag;
            for (int i = 0;i < 6; i++)
            {
                flag = true;
                i1 = i < 6 - 1 ? i + 1 : 0;
                while (flag)
                {
                    i2 = i1 + 1;
                    if (i2 >= 6)
                        i2 = 0;
                    if (i2 == (i < 6 - 1 ? i + 1 : 0))
                        break;
                    S = Math.Abs(
                        coordinates[i1].Latitude * (coordinates[i2].Longitude -coordinates[i].Longitude) +
                             coordinates[i2].Latitude * (coordinates[i].Longitude - coordinates[i1].Longitude) +
                             coordinates[i].Latitude * (coordinates[i1].Longitude - coordinates[i2].Longitude));

                }
            }
            return false;
        }
/*        int IsPointInsidePolygon(Point* p, int Number, int x, int y)
        {
            int i1, i2, n, N, S, S1, S2, S3, flag;
            N = Number;
            for (n = 0; n < N; n++)
            {
                flag = 0;
                i1 = n < N - 1 ? n + 1 : 0;
                while (flag == 0)
                {
                    i2 = i1 + 1;
                    if (i2 >= N)
                        i2 = 0;
                    if (i2 == (n < N - 1 ? n + 1 : 0))
                        break;
                    S = abs(p[i1].x * (p[i2].y - p[n].y) +
                             p[i2].x * (p[n].y - p[i1].y) +
                             p[n].x * (p[i1].y - p[i2].y));
                    S1 = abs(p[i1].x * (p[i2].y - y) +
                              p[i2].x * (y - p[i1].y) +
                              x * (p[i1].y - p[i2].y));
                    S2 = abs(p[n].x * (p[i2].y - y) +
                              p[i2].x * (y - p[n].y) +
                              x * (p[n].y - p[i2].y));
                    S3 = abs(p[i1].x * (p[n].y - y) +
                              p[n].x * (y - p[i1].y) +
                              x * (p[i1].y - p[n].y));
                    if (S == S1 + S2 + S3)
                    {
                        flag = 1;
                        break;
                    }
                    i1 = i1 + 1;
                    if (i1 >= N)
                        i1 = 0;
                    break;
                }
                if (flag == 0)
                    break;
            }
            return flag;*/
        }
}

