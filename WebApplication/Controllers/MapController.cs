using MapLibrary;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Pages
{
    public class MapController : Controller
    {
        public IActionResult Index()
        {
            MapCalculator calculator = new MapCalculator();
            MapLibrary map = new MapLibrary();
            var result = map.calculateHeatmapArray();
            return View(result);
        }
    }
}
