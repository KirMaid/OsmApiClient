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
using MapLibrary;

namespace WebApplication.Tests
{
    public class Tests
    {
        [SetUp]
        public void SetUp()
        {
            
        }
        [Test]
        public void BuildQueryCity()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setApiAddress("maps.mail.ru/osm/tools/overpass");
            List<string> buildingsTags = new List<string>()
            {
                "apartments",
                "detached",
                "house",
            };
            string queryCity = "https://maps.mail.ru/osm/tools/overpass/api/interpreter?data=" +
                "area[name = \"Волгоград\"];" +
                "(" +
                "way[\"building\" = \"apartments\"](area);" +
                "way[\"building\" = \"detached\"](area);" +
                "way[\"building\" = \"house\"](area);" +
                ");" +
                "out center;" +
                "out;";
            Assert.AreEqual(calculator.getApiConnector().BuildQueryCity("Волгоград", buildingsTags), queryCity);
        }

        [Test]
        public void ParseXmlBuildings()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setApiAddress("maps.mail.ru/osm/tools/overpass");
            List<string> buildingsTags = new List<string>()
            {
                "apartments",
                "detached",
                "house",
            };

            //Assert.IsInstanceOf(List<Node>,)
            CollectionAssert.AllItemsAreInstancesOfType(calculator.getXmlParser().ParseXmlBuildings("Волгоград", buildingsTags), typeof(Node));
        }

        [Test]
        public void getCSV()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500,5,2);
            calculator.setFilter(new List<string>() { "hospital","kindergarten" });
            calculator.calculateHeatmap().getCSV();
        }

        [Test]
        public void getCSVParallel()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 2);
            calculator.setFilter(new List<string>() { "hospital", "kindergarten" });
            calculator.calculateHeatmap(true).getCSV();
        }

        [Test]
        public void getJSON()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 0);
            calculator.setFilter(new List<string>() { "kindergarten" });
            calculator.calculateHeatmap().getJSON("D:\\test.json");
        }

        [Test]
        public void getJSONParallel()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 0);
            calculator.setFilter(new List<string>() { "kindergarten" });
            calculator.calculateHeatmap(true).getJSON("D:\\test.json");
        }

        [Test]
        public void toJSON()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 0);
            calculator.setFilter(new List<string>() { "kindergarten" });
            calculator.calculateHeatmap().toJSON();
        }

        [Test]
        public void toJSONParallel()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 0);
            calculator.setFilter(new List<string>() { "kindergarten" });
            calculator.calculateHeatmap(true).toJSON();
        }
        [Test]
        public void toList()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 0);
            calculator.setFilter(new List<string>() { "kindergarten" });
            calculator.calculateHeatmap().getList();
        }
        [Test]
        public void toListParallel()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 0);
            calculator.setFilter(new List<string>() { "kindergarten" });
            calculator.calculateHeatmap(true).getList();
        }

        [Test]
        public void toListReverse()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5);
            calculator.setFilter(new List<string>() { "bar" });
            calculator.calculateHeatmapReverse().getList();
        }

        [Test]
        public void toJSONReverse()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5);
            calculator.setFilter(new List<string>() { "bar" });
            calculator.calculateHeatmapReverse().toJSON();
        }

        [Test]
        public void getJSONReverse()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5);
            calculator.setFilter(new List<string>() { "bar" });
            calculator.calculateHeatmapReverse().getJSON();
        }

        [Test]
        public void getCSVReverse()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(1000, 5);
            calculator.setFilter(new List<string>() { "bar" });
            calculator.calculateHeatmapReverse().getCSV();
        }

        [Test]
        public void ParseXmlFilter()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 0);
            calculator.setFilter(new List<string>() { "hospital" });
            Assert.IsNull(calculator.getApiConnector().BuildQueryFilterOld("Волгоград",calculator.dictTagsChosen));
        }

        [Test]
        public void countFilters()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 2);
            //calculator.setDistanceAndCount(500, 5, 3);
            calculator.setFilter(new List<string>() { "hospital","bus_stop" });
            Assert.IsNull(calculator.filterParams.countFilters);
        }

        [Test]
        public void multiFilterCalk()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 3);
            calculator.setFilter(new List<string>() { "hospital"});
            calculator.calculateHeatmap().getCSV();
        }

        [Test]
        public void multiFilterCalkCSV()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 3);
            calculator.setFilter(new List<string>() { "hospital", "bus_stop" });
            calculator.calculateHeatmap().getCSV();
        }

         [Test]
        public void multiFilterCalkJSON()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 3);
            calculator.setFilter(new List<string>() { "hospital", "bus_stop" });
            calculator.calculateHeatmap().getJSON();
        }

        [Test]
        public void multiFilterCalkStringJSON()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 3);
            calculator.setFilter(new List<string>() { "hospital", "bus_stop" });
            calculator.calculateHeatmap().toJSON();
        }

        [Test]
        public void CalkPopulationDensityList()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.calculatePopulationDensity().getList();
        }

        [Test]
        public void CalkPopulationDensityCSV()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.calculatePopulationDensity().getCSV();
        }

        [Test]
        public void CalkPopulationDensityJSON()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.calculatePopulationDensity().getJSON();
        }

        [Test]
        public void CalkPopulationDensityStringJSON()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            var json = calculator.calculatePopulationDensity().toJSON();
        }

        [Test]
        public void CalkNumberOfStoreys()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setLevels(0, 50);
            //calculator.calculatePopulationDensity().getCSV();
            calculator.calculateNumberOfStoreys().getCSV();
        }



    } 
};