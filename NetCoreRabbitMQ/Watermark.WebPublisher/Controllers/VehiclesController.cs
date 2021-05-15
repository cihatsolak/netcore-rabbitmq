using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Watermark.WebPublisher.Models;
using Watermark.WebPublisher.Services;

namespace Watermark.WebPublisher.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public VehiclesController(AppDbContext context, RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Vehicles.ToListAsync());
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Brand,Price,Stock,ImageName,ImageFile")] Vehicle vehicle, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }

            if (ImageFile is { Length: > 0 })
            {
                string randomImageName = string.Concat(Guid.NewGuid(), Path.GetExtension(ImageFile.FileName)); //Path.GetExtension(vehicle.ImageFile.FileName) -> .jpg, .png
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", randomImageName);

                await using FileStream stream = new(path, FileMode.Create);
                await ImageFile.CopyToAsync(stream); //Resmi yukarıdaki stream'e kopyala.

                _rabbitMQPublisher.Publish(new VehicleImageCreatedEvent //RabbitMQ'ya event fırlatıyorum.
                {
                    ImageName = randomImageName
                });

                vehicle.ImageName = randomImageName;
            }

            _context.Add(vehicle);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
