using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Xml;

namespace MapLibrary
{
    public class MapCalculator
    {
        private const string WayName = "way";
        private const string NodeName = "node";
        //private string format;
        //private string apiAddress;
        private string City;
        private string cityRequest;
        private string filterRequest;
        private bool withBD = false;
        double[][] heatmapArr;
        string heatmapJSON;
        private OverpassAPIConnector overpassAPI= null;


        //private string filterKey = "shop";
        private List<string> filterKey;// = new List<string>(); //{ "school", "shop", "hospital", "kindergarten"};

        public MapCalculator(
            string apiAddress = "overpass-api.de", 
            List<string> filterKey = null, 
            string City = null
            )
        {
            this.overpassAPI = new OverpassAPIConnector(apiAddress);
            filterKey = new List<string>(filterKey);
            //this.apiAddress = apiAddress;
            this.City = City;
        }

        public void setApiAddress(string apiAddress)
        {
            overpassAPI.setApiAddress(apiAddress);
            //this.apiAddress = apiAddress;
        }

/*        public void setJsonFormat()
        {
            this.format = "json";
        }

        public void setXmlFormat()
        {
            this.format = "xml";
        }

        public void setArrayFormat()
        {
            this.format = "array";
        }*/

        public void setCity(string City)
        {
            this.City = City;
        }

        List<string> buildingsTags = new List<string>()
        {
            "apartments",
            "detached",
            "house"
        };

        Dictionary<string, Dictionary<List<string>, List<string>>> dictTags = new Dictionary<string, Dictionary<List<string>, List<string>>>() {
            { "shop", 
                new Dictionary<List<string>, List<string>>() {
                    [new List<string>() { "shop" }] = new List<string>() { "supermarket", "convenience", "mall", "general", "department_store" }
                }
            },
            { "school",
                new Dictionary<List<string>, List<string>>()
                {
                    [new List<string>() { "school" }] = new List<string>() { "school" },
                }
            },
            {
              "hospital",
               new Dictionary<List<string>, List<string>>()
               {
                   [new List<string>() { "amenity", "building" }] = new List<string>() { "clinic", "hospital" },
               }
            },
            {
               "kindergarten",
               new Dictionary<List<string>, List<string>>()
               {
                   [new List<string>() { "amenity", "building" }] = new List<string>() { "kindergarten" },

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
        /// <summary>
        /// Инициализирует облако тегов для фильтра
        /// </summary>
        public void initTags()
        {
            dictTags.Add(
                "shop",
                new Dictionary<List<string>, List<string>>() {
                    [new List<string>() { "shop" }] = new List<string>() { "supermarket", "convenience", "mall", "general", "department_store" },
                });

            dictTags.Add(
                "school",
                new Dictionary<List<string>, List<string>>()
                {
                    [new List<string>() { "school" }] = new List<string>() { "school" },
                });

            dictTags.Add(
               "hospital",
               new Dictionary<List<string>, List<string>>()
               {
                   [new List<string>() { "amenity", "building" }] = new List<string>() { "clinic", "hospital" },
               });

            dictTags.Add(
               "kindergarten",
               new Dictionary<List<string>, List<string>>()
               {
                   [new List<string>() { "amenity", "building" }] = new List<string>() { "kindergarten" },

               });
        }

        /// <summary>
        /// Служит для ручного добавления значений в фильтр значений
        /// </summary>
        public void addTag(string nameTag,Dictionary<List<string>, List<string>> keyAndValues)
        {
            dictTags.Add(nameTag,keyAndValues);
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
                if (!dictTags.ContainsKey(el))
                {
                    throw new Exception("Такой категории фильтра не существует");
                }
                else
                    this.filterKey.Add(el);
            }
        }

        public void setDistanceAndCoef()
        {

        }

        /* /// <summary>
        /// Построитель строки запроса фильтра к Overpass API
        /// </summary>
        /// <param name="dict">Массив с тегами для выборки</param>
        /// <returns>Строка запроса</returns>
        public string BuildQueryFilter()
        {
            string str = "https://" + apiAddress + "/api/interpreter?data=" + $"area[name = \"{City}\"];(";
            string endStr = ");out center;out;";
            foreach (var el in filterKey)
            {
               str += QueryFilter(dictTags[el]);
            }
            str = str + endStr;
            return str;
        }*/

        /// <summary>
        /// Построитель запроса для одной категории фильтра
        /// </summary>
        /// <param name="dict">Теги для фильтра</param>
        /// <returns></returns>
        /* public string QueryFilter(Dictionary<List<string>, List<string>> dict)
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
                    nodeWayStr = nodeWayStr.Remove(nodeWayStr.Length - 1,1);
                    nodeWayStr += "\"](area);";
                    str += "node" + nodeWayStr;
                    str += "way" + nodeWayStr;
                }
            }
            return str;
        }*/

        /// <summary>
        /// Построитель строки запроса зданий к Overpass API
        /// </summary>
        /// <returns>Строка запроса</returns>
        /*        public string BuildQueryCity()
        {
            string str = "https://" + apiAddress + "/api/interpreter?data=" + $"area[name = \"{City}\"];(";
            string endStr = ");out center;out;";
            foreach (var el in buildingsTags)
            {
                str += $"way[\"building\" = \"{el}\"](area);";
            }
            str = str + endStr;
            return str;
        }*/

        /*        public Stream createRequest(string request)
        {
            var webRequest = WebRequest.Create(request);
            webRequest.Method = "GET";
            return webRequest.GetResponse().GetResponseStream();
        }*/

        public List<Node> ParseXmlBuildings()
        {
            var requestStream = overpassAPI.createRequest(overpassAPI.BuildQueryCity(City,buildingsTags));
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

        public List<Node> ParseXmlFilter()
        {
            var requestStream = overpassAPI.createRequest(overpassAPI.BuildQueryFilter(City,dictTags));
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

        /// <summary>
        /// Функция получает из XML область и преобразует её в точку со следующими параметрами: id, координаты центра и все теги.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Объект точки</returns>
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

        /// <summary>
        /// Функция получает из XML точку и её основные параметры: id, координаты центра и все теги.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Объект точки</returns>
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

        /// <summary>
        /// Внутренняя функция для расчёта тепловой карты
        /// </summary>
        /// <returns>Массив точек с коэффициентом тепловой карты</returns>
        private HeatmapElements calculateHeatmapList()
        {
            return CalkHeatmap(ParseXmlBuildings(), ParseXmlFilter(),new DistanceParams());
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

        public HeatmapElements CalkHeatmap(List<Node> addreses, List<Node> filter, DistanceParams filterParams)
        {
            var list = new List<HeatmapElement>();
            foreach (Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                var score = 0.0;
                var coef = 1 / (filter.Count * filterParams.countObjects);
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
                heatmapElement.Coefficient = score;
                list.Add(heatmapElement);
                score = 0.0;
            }
            return new HeatmapElements(list);
        }


        /// <summary>
        /// Основная функция для расчёта тепловой карты
        /// </summary>
        /// <returns>Возвращает успешность расчёта</returns>
  /*      public Boolean calculateHeatmap()
        {
            switch (format)
            {
                case "array":
                    return calculateHeatmapArray();
                case "csv":
                    getCSV();
                    return true;
                case "json":
                    return getJSON();
                    return true;
                default:
                    throw new Exception("Не выбран формат возвращаемых данных");
            }
        }

        public Boolean calculateHeatmapArray()
        {
            try
            {
                CalkHeatmapArray(ParseXmlBuildings(), ParseXmlFilter());
            }
            catch (Exception e)
            {
                //LogError(e, "Ошибка при записи или расчёте");
                throw;
            }
            return true;
        }

        public Boolean calculateHeatmapArray()
        {
            try
            {
                CalkHeatmapArray(ParseXmlBuildings(), ParseXmlFilter());
            }
            catch (Exception e)
            {
                //LogError(e, "Ошибка при записи или расчёте");
                throw;
            }
            return true;
        }

        public void CalkHeatmapArray(List<Node> addreses, List<Node> filter, List<DistanceParams> filterParams)
        {
            heatmapArr = new double[addreses.Count][];
            NumberFormatInfo MyFormat = new NumberFormatInfo();
            MyFormat.NumberDecimalSeparator = ".";
            int i = 0;
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
                heatmapArr[i] = new double[3] { heatmapElement.Center.Latitude, heatmapElement.Center.Longitude, score };
                score = 0.0;
                i++;
            }
        }*/
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

    public class DistanceParams
    {
        public int distance;
        public double coefficient;
        public int countObjects;
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

        public List <HeatmapElement> getList()
        {
            return elements;
        }

        public void
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
            foreach (var el in filterKey)
            {
                str += QueryFilter(dictTags[el]);
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
        public string BuildQueryCity(string City,List<string> buildingsTags)
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
    }
}

