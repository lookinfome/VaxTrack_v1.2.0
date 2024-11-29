using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using v1Remastered.Services;
using v1Remastered.Models;
using v1Remastered.Dto;
using System.Linq;


namespace v1Remastered.Services
{
    public interface IUserProfileService
    {
        // exposed to: user profile controller
        public bool UpdateUserProfile(string userId, string userPhone, DateTime userBirthDate);

        // exposed to: user profile controller
        public UserDetailsDto_UserProfile FetchUserProfileDetails(string userid);

        // exposed to: user profile controller
        public Dictionary<string, string> FetchAdditionalUserDetails(UserDetailsDto_UserProfile userProfileDetails);

        // exposed to: 
        public string FetchUserRoleById(string userid);

        // exposed to: account controller
        public string FetchUserName(string userid);
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _v1RemDb;
        private readonly IUserVaccineDetailsService _userVaccineDetailsService;
        private readonly IBookingService _bookingService;

        private readonly IHospitalService _hospitalService;

        public UserProfileService(AppDbContext v1RemDb, IUserVaccineDetailsService userVaccineDetailsService, IBookingService bookingService, IHospitalService hospitalService)
        {
            _v1RemDb = v1RemDb;
            _userVaccineDetailsService = userVaccineDetailsService;
            _bookingService = bookingService;
            _hospitalService = hospitalService;
        }

        public UserDetailsDto_UserProfile FetchUserProfileDetails(string userid)
        {
            var userDetails = _v1RemDb.UserDetails.FirstOrDefault(record => record.UserId == userid);

            if (userDetails != null && !string.IsNullOrEmpty(userDetails.UserId))
            {
                var userVaccineDetails = _userVaccineDetailsService.FetchUserVaccinationDetails(userid);
                var userBookingDetails = _bookingService.FetchBookingDetails(userid);

                var userProfileDetails = new UserDetailsDto_UserProfile()
                {
                    UserId = userDetails.UserId,
                    Username = userDetails.UserName,
                    UserBirthdate = userDetails.UserBirthdate,
                    UserGender = userDetails.UserGender,
                    UserPhone = userDetails.UserPhone,
                    UserRole = userDetails.UserRole,
                    UserUid = userDetails.UserUid,
                    UserVaccineDetails = userVaccineDetails,
                    UserBookingDetails = userBookingDetails
                };

                return userProfileDetails;
            }

            return new UserDetailsDto_UserProfile();
        }

        public bool UpdateUserProfile(string userId, string userPhone, DateTime userBirthDate)
        {
            // Both values are empty
            if (string.IsNullOrEmpty(userPhone) && userBirthDate == DateTime.MinValue)
            {
                return false; // No update needed
            }

            // Update logic for user phone and/or birth date
            if (!string.IsNullOrEmpty(userPhone) || userBirthDate != DateTime.MinValue)
            {
                // fetch user details
                var fetchedDetails = _v1RemDb.UserDetails.FirstOrDefault(record => record.UserId == userId);

                if (fetchedDetails != null)
                {
                    // Call your logic to update user profile here, based on which values are non-empty
                    if (!string.IsNullOrEmpty(userPhone))
                    {
                        // Update phone logic
                        fetchedDetails.UserPhone = userPhone;
                    }

                    if (userBirthDate != DateTime.MinValue)
                    {
                        // Update birth date logic
                        fetchedDetails.UserBirthdate = userBirthDate;
                    }

                    // save to DB
                    _v1RemDb.UserDetails.Update(fetchedDetails);

                    // update DB
                    int userDetailsUpdateStatus = _v1RemDb.SaveChanges();

                    return userDetailsUpdateStatus <= 0 ? false : true;

                }

                return false; // Successful update
            }

            return false; // Return false if no values are updated
        }

        public Dictionary<string, string> FetchAdditionalUserDetails(UserDetailsDto_UserProfile userProfileDetails)
        {
            Dictionary<string, string> additionalUserDetails = new Dictionary<string, string>();

            if(userProfileDetails.UserId != null)
            {
                string hospital1Name = _hospitalService.FetchHospitalNameById(userProfileDetails.UserBookingDetails.D1HospitalId);
                string hospital2Name = _hospitalService.FetchHospitalNameById(userProfileDetails.UserBookingDetails.D2HospitalId);
                


                additionalUserDetails.Add("D1HospitalName", hospital1Name);
                additionalUserDetails.Add("D2HospitalName", hospital2Name);
            }

            return additionalUserDetails;
        }


        public string FetchUserRoleById(string userid)
        {
            var userRecord = _v1RemDb.UserDetails.FirstOrDefault(record => record.UserId == userid);

            if (userRecord != null)
            {
                return userRecord.UserRole ? "admin" : "user";
            }

            return "user"; // Default role if userRecord is null
        }


        public string FetchUserName(string userid)
        {
            var fetchedDetail = _v1RemDb.UserDetails.FirstOrDefault(record => record.UserId == userid);

            if (fetchedDetail != null)
            {
                return fetchedDetail.UserName;
            }

            return "";
        }


        

        
    }

}