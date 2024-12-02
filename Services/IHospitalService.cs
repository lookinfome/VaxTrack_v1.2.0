using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using v1Remastered.Services;
using v1Remastered.Models;
using v1Remastered.Dto;
using System.Linq;


namespace v1Remastered.Services
{
    public interface IHospitalService
    {
        public HospitalDetailsModel FetchCentersWith2Slots();
        public List<HospitalDetailsModel> FetchCentersWith1Slots();
        public string FetchHospitalNameById(string hospitalId);
        public List<HospitalDetailsModel> FetchHospitalsList();

        // v2 booking
        public List<HospitalDetailsDto_HospitalDetails> FetchAvailableHospitalsList();
        public HospitalDetailsModel FetchHospitalDeailsById(string hospitalId);
        public string FetchHospitalIdyName(string hospitalName);
        public bool FetchSlotAvailableStatus();
    }

    public class HospitalService : IHospitalService
    {
        private readonly AppDbContext _v1RemDb;

        public HospitalService(AppDbContext v1RemDb)
        {
            _v1RemDb = v1RemDb;
        }

        // fetch hospital details with 2 available slots
        public HospitalDetailsModel FetchCentersWith2Slots()
        {
            var availableCenters = _v1RemDb.HospitalDetails.FirstOrDefault(record => record.HospitalAvailableSlots >= 2);

            if (availableCenters != null && !string.IsNullOrEmpty(availableCenters.HospitalId))
            {
                return availableCenters;
            }

            return new HospitalDetailsModel();
        }

        // fetch 2 hospital details with 1 slots
        public List<HospitalDetailsModel> FetchCentersWith1Slots()
        {
            List<HospitalDetailsModel> availableCenters = _v1RemDb.HospitalDetails.Where(record=>record.HospitalAvailableSlots == 1).Take(2).ToList();

            return availableCenters.Count<=0 ? new List<HospitalDetailsModel>() : availableCenters;

        }

        // fetch hospital name by id
        public string FetchHospitalNameById(string hospitalId)
        {
            var hospitalRecord = _v1RemDb.HospitalDetails.FirstOrDefault(record => record.HospitalId == hospitalId);
            
            if (hospitalRecord != null)
            {
                return !string.IsNullOrEmpty(hospitalRecord.HospitalName) ? hospitalRecord.HospitalName : "NA";
            }
            
            return "NA";
        }

        








        // fetch slot available status
        public bool FetchSlotAvailableStatus()
        {
            // extract available slot details
            HospitalDetailsModel availableCenters = FetchCentersWith2Slots();
            List<HospitalDetailsModel> availableCentersList = FetchCentersWith1Slots();

            if(string.IsNullOrEmpty(availableCenters.HospitalId) && availableCentersList.Count < 1)
            {
                return false;
            }

            return true;
        }
    
        // fetch all hospital details
        public List<HospitalDetailsModel> FetchHospitalsList()
        {
            var hospitalList = _v1RemDb.HospitalDetails.ToList();
            if(hospitalList != null)
            {
                return hospitalList;
            }
            return new List<HospitalDetailsModel>();
        }

        // fetch all available hospitals details
        public List<HospitalDetailsDto_HospitalDetails> FetchAvailableHospitalsList()
        {
            var hospitalList = _v1RemDb.HospitalDetails
                                .Where(record=>record.HospitalAvailableSlots >= 1)
                                .Select(record => new HospitalDetailsDto_HospitalDetails 
                                    {
                                        HospitalName = record.HospitalName,
                                        HospitalAvailableSlots = record.HospitalAvailableSlots,
                                        HospitalLocation = record.HospitalLocation
                                    }
                                )
                                .ToList();
            if(hospitalList != null)
            {
                return hospitalList;
            }
            return new List<HospitalDetailsDto_HospitalDetails>();
        }

        // fetch hospital id by name
        public string FetchHospitalIdyName(string hospitalName)
        {
            var hospitalDetails = _v1RemDb.HospitalDetails.FirstOrDefault(record=>record.HospitalName == hospitalName);
            if(hospitalDetails != null)
            {
                return hospitalDetails.HospitalId;
            }

            return "";
        }

        // fetch hospital details by id
        public HospitalDetailsModel FetchHospitalDeailsById(string hospitalId)
        {
            var hospitalDetails = _v1RemDb.HospitalDetails.FirstOrDefault(record=>record.HospitalId == hospitalId);
            if(hospitalDetails != null)
            {
                return hospitalDetails;
            }
            return new HospitalDetailsModel();
        }
        
    }

}