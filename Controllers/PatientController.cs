using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Models.Clinic;

namespace Mvc.Controllers
{
    [Route("Patient")]
    public class PatientController : Controller
    {
        private readonly ClinicDbContext _context;

        public PatientController(ClinicDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // Patient Dashboard
        // /Patient
        // /Patient/Index
        // =====================================================

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null)
                return NotFound();

            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            ViewBag.PatientName = patient.User.FullName;

            return View(appointments);
        }


        // =====================================================
        // GET: /Patient/Book
        // =====================================================

        [HttpGet("Book")]
        public IActionResult Book()
        {
            var doctors = _context.Doctors
                .Include(d => d.User)
                .Where(d => d.Status == "Approved")
                .ToList();

            return View(doctors);
        }


        // =====================================================
        // POST: /Patient/Book
        // =====================================================

        [HttpPost("Book")]
        [ValidateAntiForgeryToken]
        public IActionResult Book(
            int doctorId,
            DateTime date,
            TimeSpan time)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var patient = _context.Patients
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null)
                return NotFound();


            // Check doctor exists and approved
            var doctor = _context.Doctors
                .FirstOrDefault(d =>
                    d.DoctorId == doctorId &&
                    d.Status == "Approved");

            if (doctor == null)
            {
                TempData["Error"] = "Selected doctor is not available.";
                return RedirectToAction("Book");
            }


            // Check doctor availability
            bool doctorBusy = _context.Appointments.Any(a =>
                a.DoctorId == doctorId &&
                a.Date.Date == date.Date &&
                a.Time == time &&
                a.Status != "Cancelled");

            if (doctorBusy)
            {
                ViewBag.Error =
                    "This doctor is already booked at this time.";

                var doctors = _context.Doctors
                    .Include(d => d.User)
                    .Where(d => d.Status == "Approved")
                    .ToList();

                return View(doctors);
            }


            // Check patient availability
            bool patientBusy = _context.Appointments.Any(a =>
                a.PatientId == patient.PatientId &&
                a.Date.Date == date.Date &&
                a.Time == time &&
                a.Status != "Cancelled");

            if (patientBusy)
            {
                ViewBag.Error =
                    "You already have an appointment at this time.";

                var doctors = _context.Doctors
                    .Include(d => d.User)
                    .Where(d => d.Status == "Approved")
                    .ToList();

                return View(doctors);
            }


            // Create appointment
            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = doctorId,
                Date = date,
                Time = time,
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // =====================================================
        // GET: /Patient/Details/5
        // =====================================================

        [HttpGet("Details/{id}")]
        public IActionResult Details(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var patient = _context.Patients
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null)
                return NotFound();


            var appointment = _context.Appointments

                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)

                .Include(a => a.Patient)
                .ThenInclude(p => p.User)

                .Include(a => a.MedicalRecord)

                .FirstOrDefault(a =>
                    a.AppointmentId == id &&
                    a.PatientId == patient.PatientId);


            if (appointment == null)
                return NotFound();


            return View(appointment);
        }


        // =====================================================
        // GET: /Patient/Edit/5
        // =====================================================

        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var patient = _context.Patients
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null)
                return NotFound();


            var appointment = _context.Appointments
                .FirstOrDefault(a =>
                    a.AppointmentId == id &&
                    a.PatientId == patient.PatientId);


            if (appointment == null)
                return NotFound();


            // Only Pending appointments can be edited
            if (appointment.Status != "Pending")
                return RedirectToAction("Index");


            ViewBag.Doctors = _context.Doctors
                .Include(d => d.User)
                .Where(d => d.Status == "Approved")
                .ToList();


            return View(appointment);
        }


        // =====================================================
        // POST: /Patient/Edit/5
        // =====================================================

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            int doctorId,
            DateTime date,
            TimeSpan time)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var patient = _context.Patients
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null)
                return NotFound();


            var appointment = _context.Appointments
                .FirstOrDefault(a =>
                    a.AppointmentId == id &&
                    a.PatientId == patient.PatientId);


            if (appointment == null)
                return NotFound();


            if (appointment.Status != "Pending")
                return RedirectToAction("Index");


            // Check doctor
            var doctor = _context.Doctors
                .FirstOrDefault(d =>
                    d.DoctorId == doctorId &&
                    d.Status == "Approved");

            if (doctor == null)
            {
                ViewBag.Error = "Selected doctor is not available.";

                ViewBag.Doctors = _context.Doctors
                    .Include(d => d.User)
                    .Where(d => d.Status == "Approved")
                    .ToList();

                return View(appointment);
            }


            // Check doctor availability
            bool doctorBusy = _context.Appointments.Any(a =>
                a.AppointmentId != id &&
                a.DoctorId == doctorId &&
                a.Date.Date == date.Date &&
                a.Time == time &&
                a.Status != "Cancelled");


            if (doctorBusy)
            {
                ViewBag.Error =
                    "This doctor is already booked at this time.";

                ViewBag.Doctors = _context.Doctors
                    .Include(d => d.User)
                    .Where(d => d.Status == "Approved")
                    .ToList();

                return View(appointment);
            }


            // Check patient availability
            bool patientBusy = _context.Appointments.Any(a =>
                a.AppointmentId != id &&
                a.PatientId == patient.PatientId &&
                a.Date.Date == date.Date &&
                a.Time == time &&
                a.Status != "Cancelled");


            if (patientBusy)
            {
                ViewBag.Error =
                    "You already have an appointment at this time.";

                ViewBag.Doctors = _context.Doctors
                    .Include(d => d.User)
                    .Where(d => d.Status == "Approved")
                    .ToList();

                return View(appointment);
            }


            // Update
            appointment.DoctorId = doctorId;
            appointment.Date = date;
            appointment.Time = time;
            appointment.Status = "Pending";

            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // =====================================================
        // POST: /Patient/Delete/5
        // =====================================================

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var patient = _context.Patients
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null)
                return NotFound();


            var appointment = _context.Appointments
                .FirstOrDefault(a =>
                    a.AppointmentId == id &&
                    a.PatientId == patient.PatientId);


            if (appointment == null)
                return NotFound();


            if (appointment.Status == "Pending")
            {
                _context.Appointments.Remove(appointment);
                _context.SaveChanges();
            }


            return RedirectToAction("Index");
        }
    }
}