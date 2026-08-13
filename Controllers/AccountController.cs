using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mvc.Data;
using Mvc.Models.Clinic;

namespace Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly ClinicDbContext _context;


        public AccountController(ClinicDbContext context)
        {
            _context = context;
        }


        #region Sign Up

        // GET: Account/SignUp

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }


        // POST: Account/SignUp

        [HttpPost]
        public IActionResult SignUp(
            User user,
            DateTime? birthDate,
            string? gender,
            string? specialization,
            IFormFile? certificate)
        {

            // =========================
            // Validate User
            // =========================

            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ViewBag.Error = "Full Name is required.";
                return View(user);
            }


            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ViewBag.Error = "Email is required.";
                return View(user);
            }


            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ViewBag.Error = "Password is required.";
                return View(user);
            }


            if (string.IsNullOrWhiteSpace(user.Role))
            {
                ViewBag.Error = "Please select a valid role.";
                return View(user);
            }


            // =========================
            // Check Email
            // =========================

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ViewBag.Error = "Email already exists.";
                return View(user);
            }


            // =========================
            // Doctor
            // =========================

            if (user.Role.Trim() == "Doctor")
            {

                if (string.IsNullOrWhiteSpace(specialization))
                {
                    ViewBag.Error =
                        "Specialization is required.";

                    return View(user);
                }


                if (certificate == null)
                {
                    ViewBag.Error =
                        "Certificate is required.";

                    return View(user);
                }


                // =========================
                // Save Certificate
                // =========================

                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/certificates");


                Directory.CreateDirectory(folder);


                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(
                        certificate.FileName);


                string filePath =
                    Path.Combine(folder, fileName);


                using (var stream =
                       new FileStream(
                           filePath,
                           FileMode.Create))
                {
                    certificate.CopyTo(stream);
                }


                // =========================
                // Save User
                // =========================

                user.Role = "Doctor";

                _context.Users.Add(user);

                _context.SaveChanges();


                // =========================
                // Save Doctor
                // =========================

                Doctor doctor = new Doctor
                {
                    UserId = user.UserId,

                    Specialization = specialization,

                    Status = "Pending",

                    CertificatePath =
                        "/uploads/certificates/"
                        + fileName
                };


                _context.Doctors.Add(doctor);

                _context.SaveChanges();


                // =========================
                // Doctor Message
                // =========================

                TempData["SuccessMessage"] =
                    "Your registration has been completed successfully. " +
                    "Please wait for the Admin to approve your account.";


                // Go Home

                return RedirectToAction(
                    "Index",
                    "Home");
            }


            // =========================
            // Patient
            // =========================

            else if (user.Role.Trim() == "Patient")
            {

                if (birthDate == null)
                {
                    ViewBag.Error =
                        "Birth Date is required.";

                    return View(user);
                }


                if (string.IsNullOrWhiteSpace(gender))
                {
                    ViewBag.Error =
                        "Gender is required.";

                    return View(user);
                }


                // =========================
                // Save User
                // =========================

                user.Role = "Patient";

                _context.Users.Add(user);

                _context.SaveChanges();


                // =========================
                // Save Patient
                // =========================

                Patient patient = new Patient
                {
                    UserId = user.UserId,

                    BirthDate = birthDate.Value,

                    Gender = gender
                };


                _context.Patients.Add(patient);

                _context.SaveChanges();


                // Go Patient Page

                return RedirectToAction(
                    "Index",
                    "Patient");
            }


            // =========================
            // Invalid Role
            // =========================

            else
            {
                ViewBag.Error =
                    "Please select a valid role.";

                return View(user);
            }
        }

        #endregion


        #region Login

        // GET: Account/Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // POST: Account/Login

        [HttpPost]
        public IActionResult Login(
            string email,
            string password)
        {

            // =========================
            // Find User
            // =========================

            var user = _context.Users.FirstOrDefault(u =>
                u.Email == email &&
                u.Password == password);


            if (user == null)
            {
                ViewBag.Error =
                    "Invalid email or password.";

                return View();
            }


            // =========================
            // Save UserId
            // =========================

            HttpContext.Session.SetInt32(
                "UserId",
                user.UserId);


            // =========================
            // Doctor
            // =========================

            if (user.Role == "Doctor")
            {

                var doctor = _context.Doctors
                    .FirstOrDefault(d =>
                        d.UserId == user.UserId);


                if (doctor == null)
                {
                    ViewBag.Error =
                        "Doctor account not found.";

                    return View();
                }


                // Pending

                if (doctor.Status == "Pending")
                {
                    ViewBag.Error =
                        "Your account is waiting for admin approval.";

                    return View();
                }


                // Rejected

                if (doctor.Status == "Rejected")
                {
                    ViewBag.Error =
                        "Your account has been rejected.";

                    return View();
                }


                // Approved

                if (doctor.Status == "Approved")
                {
                    return RedirectToAction(
                        "Index",
                        "Doctor");
                }
            }


            // =========================
            // Patient
            // =========================

            else if (user.Role == "Patient")
            {
                return RedirectToAction(
                    "Index",
                    "Patient");
            }


            // =========================
            // Admin
            // =========================

            else if (user.Role == "Admin")
            {
                return RedirectToAction(
                    "Index",
                    "Admin");
            }


            ViewBag.Error =
                "Invalid role.";

            return View();
        }

        #endregion
    }
}