using MapLibrary;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Pages
{
    [ApiController]
    [Route("[controller]")]
    public class MapController : Controller
    {
        [HttpGet]
        public /*IActionResult*/ double[][] Index()
        {
            MapCalculator calculator = new MapCalculator();
            MapLibrary map = new MapLibrary();
            return /*var result = */ map.calculateHeatmapArray();

            //return View(result);
        }
    }
}
