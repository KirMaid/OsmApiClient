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
/*        [Test]
        public void calculateHeatmap()
        {
            MapLibrary map = new MapLibrary();
            var test = map.calculateHeatmap();
            foreach (WebApplication.HeatmapElement elem in test)
            {
                Console.WriteLine(elem.Coefficient);
            }
        }*/

/*        [Test]
        public void BuildQueryCity()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            Assert.IsNull(calculator.BuildQueryCity());
        }

        [Test]
        public void ParseXmlBuildings()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            Assert.IsNull(calculator.ParseXmlBuildings());
        }

        [Test]
        public void calculateHeatmapArray()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
        }

        [Test]
        public void ParseXmlFilter()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setFilter(new List<string>() { "shop" });
            var test = calculator.ParseXmlFilter();
            Assert.IsNull(test);
        }

        [Test]
        public void BuildQueryFilter()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setFilter(new List<string>() { "shop"});

            Assert.IsNull(calculator.BuildQueryFilter());
        }*/
        [Test]
        public void getCSV()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500,5,4);
            calculator.setFilter(new List<string>() { "hospital" });
            calculator.calculateHeatmap().getCSV();
        }

        [Test]
        public void ParseXmlFilter()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 0);
            calculator.setFilter(new List<string>() { "hospital" });
            Assert.IsNull(calculator.getApiConnector().BuildQueryFilter("Волгоград",calculator.dictTagsChosen));
        }


        [Test]
        public void ParseXmlBuildings()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 0);
            calculator.setFilter(new List<string>() { "hospital" });
            //Assert.IsNull(calculator.getApiConnector().BuildQueryCity("Волгоград", calculator.buildingsTags));
            List<Node> nodes = calculator.getXmlParser().ParseXmlBuildings("Волгоград", calculator.buildingsTags);
            Assert.IsNull(calculator.getXmlParser().ParseXmlBuildings("Волгоград", calculator.buildingsTags));
            //Assert.IsNull(calculator.getApiConnector().Pars);
        }

        [Test]
        public void countFilters()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 3);
            calculator.setFilter(new List<string>() { "hospital" });
            Assert.IsNull(calculator.filterParams.countFilters);
        }

        [Test]
        public void CalkPopulationDensity()
        {
            MapCalculator calculator = new MapCalculator();
            calculator.setCity("Волгоград");
            calculator.setLevels(0, 50);
            calculator.calculatePopulationDensity().getCSV();
        }

    } 
};