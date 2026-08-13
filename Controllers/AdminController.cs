using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;

namespace Mvc.Controllers
{
    public class AdminController : Controller
    {
        private readonly ClinicDbContext _context;

        public AdminController(ClinicDbContext context)
        {
            _context = context;
        }

        #region Dashboard

        public IActionResult Index()
        {
            ViewBag.TotalDoctors = _context.Doctors.Count();

            ViewBag.ApprovedDoctors =
                _context.Doctors.Count(d => d.Status == "Approved");

            ViewBag.PendingDoctors =
                _context.Doctors.Count(d => d.Status == "Pending");

            ViewBag.RejectedDoctors =
                _context.Doctors.Count(d => d.Status == "Rejected");

            ViewBag.TotalPatients =
                _context.Patients.Count();

            ViewBag.TotalAppointments =
                _context.Appointments.Count();

            ViewBag.PendingAppointments =
                _context.Appointments.Count(a => a.Status == "Pending");

            ViewBag.ApprovedAppointments =
                _context.Appointments.Count(a => a.Status == "Approved");

            ViewBag.CancelledAppointments =
                _context.Appointments.Count(a => a.Status == "Cancelled");

            return View();
        }

        #endregion

        #region Doctors

        public IActionResult Doctors()
        {
            var doctors = _context.Doctors
                .Include(d => d.User)
                .OrderBy(d => d.Status)
                .ThenBy(d => d.User.FullName)
                .ToList();

            return View(doctors);
        }

        public IActionResult DoctorDetails(int id)
        {
            var doctor = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Appointments)
                .FirstOrDefault(d => d.DoctorId == id);

            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        // =========================
        // Approve Doctor
        // =========================
        [HttpPost]
        public IActionResult ApproveDoctor(int id)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);

            if (doctor == null)
                return NotFound();

            doctor.Status = "Approved";
            doctor.RejectionReason = null;

            _context.SaveChanges();

            return RedirectToAction(nameof(Doctors));
        }

        // =========================
        // Reject Doctor
        // =========================
        [HttpPost]
        public IActionResult RejectDoctor(int id, string rejectionReason)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);

            if (doctor == null)
                return NotFound();

            doctor.Status = "Rejected";
            doctor.RejectionReason = rejectionReason;

            _context.SaveChanges();

            return RedirectToAction(nameof(Doctors));
        }

        #endregion

        #region Patients

        public IActionResult Patients()
        {
            var patients = _context.Patients
                .Include(p => p.User)
                .OrderBy(p => p.User.FullName)
                .ToList();

            return View(patients);
        }

        public IActionResult PatientDetails(int id)
        {
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.MedicalRecord)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            ViewBag.Appointments = appointments;

            return View(patient);
        }

        #endregion

        #region Appointments

        public IActionResult Appointments()
        {
            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            return View(appointments);
        }

        public IActionResult AppointmentDetails(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.MedicalRecord)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        #endregion
    }
}