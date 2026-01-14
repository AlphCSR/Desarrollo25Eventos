
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace InvoicingMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly string _storagePath = "/app/invoices";

        [HttpGet("download/{bookingId}")]
        public IActionResult Download(Guid bookingId)
        {
            var filePath = Path.Combine(_storagePath, $"Invoice_{bookingId}.pdf");
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("La factura aún no ha sido generada o no existe.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", $"Factura_{bookingId}.pdf");
        }
    }
}
