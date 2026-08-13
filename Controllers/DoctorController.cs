
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Models.Clinic;

namespace Mvc.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ClinicDbContext _context;

        public DoctorController(ClinicDbContext context)
        {
            _context = context;
        }


        #region Doctor Dashboard

        public IActionResult Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return NotFound();

            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.MedicalRecord)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();

            ViewBag.DoctorName = doctor.User.FullName;

            return View(appointments);
        }

        #endregion


        #region Appointment Details

        public IActionResult AppointmentDetails(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var doctor = _context.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return NotFound();

            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.MedicalRecord)
                .FirstOrDefault(a =>
                    a.AppointmentId == id &&
                    a.DoctorId == doctor.DoctorId);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        #endregion


        #region Approve Appointment

        [HttpPost]
        public IActionResult ApproveAppointment(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var doctor = _context.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return NotFound();

            var appointment = _context.Appointments
                .FirstOrDefault(a =>
                    a.AppointmentId == id &&
                    a.DoctorId == doctor.DoctorId);

            if (appointment == null)
                return NotFound();

            if (appointment.Status == "Pending")
            {
                appointment.Status = "Approved";

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        #endregion


        #region Cancel Appointment

        [HttpPost]
        public IActionResult CancelAppointment(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var doctor = _context.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return NotFound();

            var appointment = _context.Appointments
                .FirstOrDefault(a =>
                    a.AppointmentId == id &&
                    a.DoctorId == doctor.DoctorId);

            if (appointment == null)
                return NotFound();

            if (appointment.Status == "Pending" ||
                appointment.Status == "Approved")
            {
                appointment.Status = "Cancelled";

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        #endregion


        #region Medical Record

        [HttpGet]
        public IActionResult CreateMedicalRecord(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var doctor = _context.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return NotFound();

            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .FirstOrDefault(a =>
                    a.AppointmentId == id &&
                    a.DoctorId == doctor.DoctorId);

            if (appointment == null)
                return NotFound();

            if (appointment.Status != "Approved")
                return RedirectToAction("Index");

            var existingRecord = _context.MedicalRecords
                .FirstOrDefault(r => r.AppointmentId == id);

            if (existingRecord != null)
                return RedirectToAction(
                    "MedicalRecord",
                    new { id = id });

            ViewBag.PatientName = appointment.Patient.User.FullName;
            ViewBag.AppointmentDate = appointment.Date;
            ViewBag.AppointmentTime = appointment.Time;

            return View();
        }


        [HttpPost]
        public IActionResult CreateMedicalRecord(
            int id,
            string diagnosis,
            string? prescription,
            string? note)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var doctor = _context.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return NotFound();

            var appointment = _context.Appointments
                .FirstOrDefault(a =>
                    a.AppointmentId == id &&
                    a.DoctorId == doctor.DoctorId);

            if (appointment == null)
                return NotFound();

            if (appointment.Status != "Approved")
                return RedirectToAction("Index");

            var existingRecord = _context.MedicalRecords
                .FirstOrDefault(r => r.AppointmentId == id);

            if (existingRecord != null)
                return RedirectToAction(
                    "MedicalRecord",
                    new { id = id });

            var record = new MedicalRecord
            {
                AppointmentId = appointment.AppointmentId,
                Date = DateTime.Now,
                Diagnosis = diagnosis,
                Prescription = prescription,
                Note = note
            };

            _context.MedicalRecords.Add(record);

            _context.SaveChanges();

            return RedirectToAction(
                "AppointmentDetails",
                new { id = id });
        }

        #endregion


        #region Medical Record Details

        public IActionResult MedicalRecord(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var doctor = _context.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return NotFound();

            var record = _context.MedicalRecords
                .Include(r => r.Appointment)
                .ThenInclude(a => a.Patient)
                .ThenInclude(p => p.User)
                .FirstOrDefault(r =>
                    r.AppointmentId == id &&
                    r.Appointment.DoctorId == doctor.DoctorId);

            if (record == null)
                return NotFound();

            return View(record);
        }

        #endregion
    }
}

