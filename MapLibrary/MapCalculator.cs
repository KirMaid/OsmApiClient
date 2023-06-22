using CsvHelper;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace MapLibrary
{
    public class MapCalculator
    {
        private string City;
        private bool useBD = false;
        private bool useCache;
        private OverpassAPIConnector overpassAPI;
        private OpenStreetXmlParser xmlParser;
        public DistanceParams filterParams;
        Dictionary<string, DateTime> updateDataTime = new Dictionary<string, DateTime>();
        Dictionary<string, List<Node>> city = new Dictionary<string, List<Node>>();

        public MapCalculator(
            string apiAddress = "maps.mail.ru/osm/tools/overpass",
            string City = null,
            bool useBD = false,
            bool useCache = false
            )
        {
            this.overpassAPI = new OverpassAPIConnector(apiAddress);
            this.City = City;
            this.xmlParser = new OpenStreetXmlParser(this.overpassAPI);
            this.useBD = useBD;
            this.useCache = useCache;
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
            {
               "playground",
               new Dictionary<List<string>, List<string>>()
               {
                   [new List<string>() {"leisure"}] = new List<string>() {"playground"},

               }
            },
            {
               "bar",
               new Dictionary<List<string>, List<string>>()
               {
                   [new List<string>() {"amenity"}] = new List<string>() { "bar","pub" },

               }
            },
            {
               "cafe",
               new Dictionary<List<string>, List<string>>()
               {
                   [new List<string>() {"amenity"}] = new List<string>() { "cafe" },

               }
            },
            {
               "industrial",
               new Dictionary<List<string>, List<string>>()
               {
                   [new List<string>() {"industrial"}] = new List<string>() {"aluminium_smelting", "brickyard", "factory", "mine", "oil", "refinery", "shipyard", "scrap_yard", "sawmill"},

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
                    filterParams.countFilters += 1;
                }
            }
        }

        public void setDistanceAndCount(int distance, int count, int minCount = 0)
        {
            this.filterParams = new DistanceParams(distance, count, minCount);
        }

        public void setLevels(int minLevel, int maxLevel)
        {
            this.filterParams = new DistanceParams(maxLevel, minLevel);
        }


/*        public HeatmapElements calculateHeatmapOld()
        {
            return CalkHeatmapOld(xmlParser.ParseXmlBuildings(City, buildingsTags), xmlParser.ParseXmlFilter(City, dictTagsChosen), filterParams);
        }
*/
        public HeatmapElements calculatePopulationDensity()
        {
            return CalkPopulationDensity(xmlParser.ParseXmlBuildings(City, buildingsTags));
        }

        public HeatmapElements calculateNumberOfStoreys()
        {
            return CalkNumberOfStoreys(xmlParser.ParseXmlBuildings(City, buildingsTags), filterParams);
        }

        public HeatmapElements calculateHeatmap(bool useParallel = false)
        {
            if(useParallel)
                return CalkHeatmapParallel(xmlParser.ParseXmlBuildings(City, buildingsTags), xmlParser.ParseXmlFilter(City, dictTagsChosen), filterParams);
            else
                return CalkHeatmap(xmlParser.ParseXmlBuildings(City, buildingsTags), xmlParser.ParseXmlFilter(City, dictTagsChosen), filterParams);
        }

        public HeatmapElements calculateHeatmapReverse()
        {
            return CalkHeatmapReverse(xmlParser.ParseXmlBuildings(City, buildingsTags), xmlParser.ParseXmlFilter(City, dictTagsChosen), filterParams);
        }

        public HeatmapElements CalkPopulationDensity(List<Node> addreses)
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
                heatmapElement.Coefficient = levels * 5 * 6 * 1.5;
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

        /* public HeatmapElements CalkHeatmapParams(List<Node> addreses, List<Node> filter, List<DistanceParams> filterParams)
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
 */
        //TODO:идёт компенсация одной категории за счёт другой, непорядок
        /// <summary>
        /// Внутренняя функция для расчёта тепловой карты
        /// </summary>
        /// <returns>Массив точек с коэффициентом тепловой карты</returns>
       /* public HeatmapElements CalkHeatmapOld(List<Node> addreses, List<Node> filter, DistanceParams filterParams)
        {
            var list = new List<HeatmapElement>();
            foreach (Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                var score = 0.0;
                var coef = 1.0 / (filterParams.countFilters * filterParams.countObjects);//0,1 коэффициент
                var minCoef = coef * filterParams.minCount * filterParams.countFilters;
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
                if (score < minCoef)
                    heatmapElement.Coefficient = 0;
                else
                    heatmapElement.Coefficient = score;
                list.Add(heatmapElement);
                score = 0.0;
            }
            return new HeatmapElements(list);
        }
*/

        /// <summary>
        /// Внутренняя функция для расчёта тепловой карты
        /// </summary>
        /// <returns>Массив точек с коэффициентом тепловой карты</returns>
        public HeatmapElements CalkHeatmap(List<Node> addreses, List<List<Node>> filters, DistanceParams filterParams)
        {
            var list = new List<HeatmapElement>();
            var countFilters = filters.Count;
            foreach (Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                double score = 0.0;
                //Кол-во категорий фильтров на желаемое кол-во объектов
                //TODO: Решить что делать со счётчиком фильтров в DistanceParams 
                double coef = 1.0 / (countFilters * filterParams.countObjects);
                foreach (List<Node> filter in filters)
                {
                    int foundsFilters = 0;
                    foreach (Node nodeFilter in filter)
                    {
                        var dist = calculateTheDistance(node.Center, nodeFilter.Center);
                        if (dist < filterParams.distance)
                        {
                            if (foundsFilters < filterParams.countObjects)
                                foundsFilters++;
                            else
                                break;
                        }
                    }
                    if (foundsFilters >= filterParams.minCount)
                    {
                        score += coef * foundsFilters;
                    }
                    else
                    {
                        score = 0;
                        break;
                    }    
                }
                if (score >= coef * filterParams.minCount)
                {
                    heatmapElement.Coefficient = score;
                    list.Add(heatmapElement);
                }
            }
            return new HeatmapElements(list);
        }

        public HeatmapElements CalkHeatmapParallel(List<Node> addreses, List<List<Node>> filters, DistanceParams filterParams)
        {
            var list = new List<HeatmapElement>();
            var countFilters = filters.Count;
            Parallel.ForEach(addreses, i => {
                var heatmapNode = CalkHeatHode(i, filters,filterParams);
                if (heatmapNode.Coefficient != 0)
                    list.Add(heatmapNode);
            });
            return new HeatmapElements(list);
        }

       /* public HeatmapElements CalkHeatmapParallel (List<Node> addreses, List<List<Node>> filters, DistanceParams filterParams)
        {
            var list = new List<HeatmapElement>();
            var countFilters = filters.Count;
            Parallel.ForEach(addreses, node =>
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                double score = 0.0;
                //Кол-во категорий фильтров на желаемое кол-во объектов
                //TODO: Решить что делать со счётчиком фильтров в DistanceParams
                double coef = 1.0 / (countFilters * filterParams.countObjects);
                foreach (List<Node> filter in filters)
                {
                    int foundsFilters = 0;
                    Parallel.ForEach(filter, nodeFilter =>
                    {
                        var dist = calculateTheDistance(node.Center, nodeFilter.Center);
                        if (dist < filterParams.distance)
                        {
                            Interlocked.Increment(ref foundsFilters);
                        }
                    });
                    if (foundsFilters >= filterParams.minCount)
                    {
                        score += coef * foundsFilters;
                    }
                    else
                    {
                        score = 0;
                        break;
                    }
                }
                if (score >= coef * filterParams.minCount)
                {
                    heatmapElement.Coefficient = score;
                    list.Add(heatmapElement);
                }
            });
            return new HeatmapElements(list);
        }*/


        public HeatmapElement CalkHeatHode(Node node, List<List<Node>> filters, DistanceParams filterParams)
        {
            var countFilters = filters.Count;
            HeatmapElement heatmapElement = new HeatmapElement();
            heatmapElement.Center = node.Center;
            double score = 0.0;
            //Кол-во категорий фильтров на желаемое кол-во объектов
            //TODO: Решить что делать со счётчиком фильтров в DistanceParams 
            double coef = 1.0 / (countFilters * filterParams.countObjects);
            foreach (List<Node> filter in filters)
            {
                int foundsFilters = 0;
                foreach (Node nodeFilter in filter)
                {
                    var dist = calculateTheDistance(node.Center, nodeFilter.Center);
                    if (dist < filterParams.distance)
                    {
                        if (foundsFilters < filterParams.countObjects)
                            foundsFilters++;
                        else
                            break;
                    }
                }
                if (foundsFilters >= filterParams.minCount)
                {
                    score += coef * foundsFilters;
                }
                else
                {
                    score = 0;
                    break;
                }
            }
            heatmapElement.Coefficient = score;
            return heatmapElement;
        }


        /// <summary>
        /// Внутренняя функция для расчёта реверсивной тепловой карты
        /// </summary>
        /// <returns>Массив точек с коэффициентом тепловой карты</returns>
/*        public HeatmapElements CalkHeatmapReverseOld(List<Node> addreses, List<Node> filter, DistanceParams filterParams)
         {
             var list = new List<HeatmapElement>();
             foreach (Node node in addreses)
             {
                 HeatmapElement heatmapElement = new HeatmapElement();
                 heatmapElement.Center = node.Center;
                 var score = 1.0;
                 //Кол-во категорий фильтров на желаемое кол-во объектов
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
         }*/

        public HeatmapElements CalkHeatmapReverse(List<Node> addreses, List<List<Node>> filters, DistanceParams filterParams)
        {
            var list = new List<HeatmapElement>();
            var countFilters = filters.Count;
            foreach (Node node in addreses)
            {
                HeatmapElement heatmapElement = new HeatmapElement();
                heatmapElement.Center = node.Center;
                double score = 1.0;
                //Кол-во категорий фильтров на желаемое кол-во объектов
                //TODO: Решить что делать со счётчиком фильтров в DistanceParams 
                double coef = 1.0 / (countFilters * filterParams.countObjects);
                foreach (List<Node> filter in filters)
                {
                    int foundsFilters = 0;
                    foreach (Node nodeFilter in filter)
                    {
                        var dist = calculateTheDistance(node.Center, nodeFilter.Center);
                        if (dist < filterParams.distance)
                        {
                            if (foundsFilters < filterParams.countObjects)
                                foundsFilters++;
                            else
                                break;
                        }

                        if (dist < filterParams.distance && ((node.Center.Latitude == 48.7500673) && (node.Center.Longitude == 44.5008089)))
                        {
                            Console.WriteLine(nodeFilter.Center.Latitude);
                            Console.WriteLine(nodeFilter.Center.Longitude);
                        }
                    }
                    if (foundsFilters >= filterParams.countObjects)
                    {
                        score = 0;
                        break;
                    }
                    else
                    {
                        score -= coef * foundsFilters; 
                    }
                }
                if (score > 0)
                {
                    heatmapElement.Coefficient = score;
                    list.Add(heatmapElement);
                }
            }
            return new HeatmapElements(list);
        }

        public OverpassAPIConnector OverpassAPIConnector
        {
            get => default;
            set
            {
            }
        }

        public DistanceParams DistanceParams
        {
            get => default;
            set
            {
            }
        }

        public HeatmapElements HeatmapElements
        {
            get => default;
            set
            {
            }
        }

        public OpenStreetXmlParser OpenStreetXmlParser
        {
            get => default;
            set
            {
            }
        }
    }

    public class Node
    {
        public long Id { get; set; }
        public List<Tag> Tags { get; set; }
        public Coordinate Center { get; set; }

        public Tag Tag
        {
            get => default;
            set
            {
            }
        }
    }

    public class HeatmapElement
    {
        public Coordinate Center { get; set; }
        public double Coefficient { get; set; }

        public Coordinate Coordinate
        {
            get => default;
            set
            {
            }
        }
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

        public HeatmapElement HeatmapElement
        {
            get => default;
            set
            {
            }
        }

        public Node Node
        {
            get => default;
            set
            {
            }
        }

        public void getCSV(string pathCsvFile = "D:\\test.csv")
        {
            using (var writer = new StreamWriter(pathCsvFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(elements);
            }
        }
        public void getJSON(string pathJSONFile = "D:\\test.json")
        {
            File.WriteAllText(pathJSONFile, toJSON());
            using (StreamWriter file = File.CreateText(pathJSONFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, elements);
            }
        }

        public string toJSON()
        {
            return JsonConvert.SerializeObject(elements, Formatting.Indented);
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
        public string BuildQueryFilterOld(string City, Dictionary<string, Dictionary<List<string>, List<string>>> dictTags)
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

        public List<string> BuildQueryFilters(string City, Dictionary<string, Dictionary<List<string>, List<string>>> dictTags)
        {
            List<string> queries = new List<string>();
            string startStr = "https://" + apiAddress + "/api/interpreter?data=" + $"area[name = \"{City}\"];(";
            string endStr = ");out center;out;";
            foreach (var el in dictTags)
            {
                queries.Add(startStr + QueryFilter(el.Value) + endStr);
            }
            return queries;
        }

        /// <summary>
        /// Построитель запроса для одной категории фильтра
        /// </summary>
        /// <param name="dict">Теги для фильтра</param>
        /// <returns></returns>
        public string QueryOneFilter(Dictionary<List<string>, List<string>> dict)
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

    public class OpenStreetXmlParser
    {
        private const string WayName = "way";
        private const string NodeName = "node";
        private OverpassAPIConnector overpassAPI;

        public OpenStreetXmlParser(OverpassAPIConnector overpassAPI)
        {
            this.overpassAPI = overpassAPI;
        }

        public Node Node
        {
            get => default;
            set
            {
            }
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
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == WayName)
                        {
                            var way = GetWay(reader);
                            list.Add(way);
                        }
/*                        if (reader.Name == NodeName)
                        {
                            var way = GetNode(reader);
                            list.Add(way);
                        }*/
                    }
                }
            }
            return list;
        }

        public List<Node> ParseXmlFilterOld(string City, Dictionary<string, Dictionary<List<string>, List<string>>> dictTags)
        {
            var requestStream = overpassAPI.createRequest(overpassAPI.BuildQueryFilterOld(City, dictTags));
            var list = new List<Node>();
            using (var reader = XmlReader.Create(requestStream))
            {
                while (reader.Read())
                {
                    if(reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == WayName)
                        {
                            var way = GetWay(reader);
                            list.Add(way);
                        }
                        if (reader.Name == NodeName)
                        {
                            var way = GetNode(reader);
                            list.Add(way);
                        }
                    }
                }
            }
            return list;
        }

        public List<List<Node>> ParseXmlFilter(string City, Dictionary<string, Dictionary<List<string>, List<string>>> dictTags)
        {
            List<List<Node>> filtersNodes = new List<List<Node>>();
            List<string> filters = overpassAPI.BuildQueryFilters(City, dictTags);
            foreach (var filter in filters)
            {
                var requestStream = overpassAPI.createRequest(filter);
                var nodes = new List<Node>();
                using (var reader = XmlReader.Create(requestStream))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == WayName)
                                nodes.Add(GetWay(reader));
                            if (reader.Name == NodeName)
                                nodes.Add(GetNode(reader));
                        }
                    }
                }
                filtersNodes.Add(nodes);
            }
            return filtersNodes;
        }
    }

    public class PostGISConnector
    {
        protected string connString = "Server=127.0.0.1;Port=5432;Database=osm;User Id=postgres;Password=1234";
        protected NpgsqlConnection connection;
        protected void setConnection(string connString)
        {
            connection = new NpgsqlConnection(connString);
        }
        
        public void openConnection()
        {
            connection.Open();
        }

        public void closeConnection()
        {
            connection.Close();
        }
        //planet_osm_polygon
        public void executeRequest(string request)
        {
            openConnection();
           /* using var cmdSelect = new NpgsqlCommand("SELECT id, name, age FROM mytable WHERE id = @id", conn);
            cmdSelect.Parameters.AddWithValue("id", 1);
            using var reader = cmdSelect.ExecuteReader();
            if (reader.Read())
            {
                Console.WriteLine("{0} {1} {2}",
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2));
            }
            else
            {
                Console.WriteLine("Запись не найдена");
            }*/
            closeConnection();
        }
        //Таблица 
        //ConnectionString Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;
    }

    //Класс для работы с полигонами, предназначет для разбиения области на полигоны(Пока в разработке)
    public class Polygon
    {
        public bool isPointInsidePolygon(List<Coordinate> coordinates, Coordinate coordinate)
        {
            int i1, i2, n, S1, S2, S3;
            double S;
            bool flag;
            for (int i = 0; i < 6; i++)
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
                        coordinates[i1].Latitude * (coordinates[i2].Longitude - coordinates[i].Longitude) +
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


