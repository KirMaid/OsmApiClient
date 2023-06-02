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
        //[Route("Default/GetAuthor/{authorId:int}")]
        public ActionResult/*double[][]*/ Index()
        {
            MapCalculator calculator = new MapCalculator();
            //MapLibrary map = new MapLibrary();
            //return /*var result = */ map.calculateHeatmapArray();
            calculator.setCity("Волгоград");
            calculator.setDistanceAndCount(500, 5, 4);
            calculator.setFilter(new List<string>() { "hospital" });

            return View(calculator.calculateHeatmap().getJSON());
        }

        [HttpGet]
        [Route("/api/{authorId:int,}")]
    }
}
