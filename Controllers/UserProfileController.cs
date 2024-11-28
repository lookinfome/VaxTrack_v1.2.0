
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using v1Remastered.Services;
using v1Remastered.Dto;
using Microsoft.AspNetCore.Identity;

namespace v1Remastered.Controllers
{
    [Route("UserProfile")]
    public class UserProfileController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserProfileService _userProfileService;
        private readonly IHospitalService _hospitalService;
        private readonly UserManager<AppUserIdentityModel> _userManager;
        public UserProfileController(IAuthService authService, IUserProfileService userProfileService, IHospitalService hospitalService, UserManager<AppUserIdentityModel> userManager)
        {
            _authService = authService;
            _userProfileService = userProfileService;
            _hospitalService = hospitalService;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("{userid}")]
        public async Task<IActionResult> UserProfile(string userid)
        {
            if (!string.IsNullOrEmpty(userid))
            {

                // fetch user details from asp-net-user table for authentication
                var loggedInUser = await _userManager.GetUserAsync(User);

                // if user's record not found
                if (loggedInUser == null || loggedInUser.UserName != userid)
                {
                    await _authService.LogoutUserAsync();
                    return RedirectToAction("LoginUser", "Account");
                }

                // fetch hospital details
                UserDetailsDto_UserProfile userProfileDetails = _userProfileService.FetchUserProfileDetails(userid);
                ViewBag.D1HospitalName = _hospitalService.FetchHospitalNameById(userProfileDetails.UserBookingDetails.D1HospitalId);
                ViewBag.D2HospitalName = _hospitalService.FetchHospitalNameById(userProfileDetails.UserBookingDetails.D2HospitalId);

                // welcome message
                if(!string.IsNullOrEmpty(TempData["userLoginMsgSuccess"]?.ToString()))
                {
                    ViewBag.WelcomeMessage = TempData["userLoginMsgSuccess"];
                }

                if(!string.IsNullOrEmpty(TempData["userProfileUpdateMsg"]?.ToString()))
                {
                    ViewBag.UserProfileUpdateMsg = TempData["userProfileUpdateMsg"];
                }
 
                // final call to view
                if (userProfileDetails.UserId != null)
                {
                    return View(userProfileDetails);
                }
            }

            return View("~/Shared/_Error.cshtml");
        }

        [HttpGet("{userid}/Edit")]
        public async Task<IActionResult> UserProfileEdit(string userid)
        {
            if (!string.IsNullOrEmpty(userid))
            {

                // fetch user details from asp-net-user table for authentication
                var loggedInUser = await _userManager.GetUserAsync(User);

                // if user's recor not found
                if (loggedInUser == null || loggedInUser.UserName != userid)
                {
                    await _authService.LogoutUserAsync();
                    return RedirectToAction("LoginUser", "Account");
                }

                return View();
            }
            return View("~/Shared/_Error.cshtml");
        }

        [HttpPost("{userid}/Edit")]
        public IActionResult UserProfileEdit(UserDetailsDto_UserProfileEdit submittedDetails, [FromRoute] string userid)
        {
            string userPassword = string.IsNullOrEmpty(submittedDetails.UserPassword) ? "" : submittedDetails.UserPassword;

            // fetch user authentic state
            bool isUserAuthentic = _authService.CheckUserAuthenticity(userid, userPassword).Result;

            if(isUserAuthentic)
            {
                bool result = _userProfileService.UpdateUserProfile(userid, submittedDetails.UserPhone, submittedDetails.UserBirthdate);
                string userName = _userProfileService.FetchUserName(userid);

                TempData["userProfileUpdateMsg"] = $"{userName}'s profile have been updated successfully";

                return result ? RedirectToAction("UserProfile", "UserProfile", new { userid = userid }) : View();
            }

            TempData["userAuthenticityStatus"] = "Wrong password, user needs to authenticate themselves first";
            ViewBag.UserAuthenticityStatus = TempData["userAuthenticityStatus"];
            return View();

        }


    }

}