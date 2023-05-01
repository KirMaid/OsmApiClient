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

        [Test]
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
            //calculator.
        }


    } 
};