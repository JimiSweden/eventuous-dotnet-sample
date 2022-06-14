namespace Orders.Domain
{
    /// <summary>
    /// todo? add 'Notes', i.e. mottagare, portkod etc. utöver ApartmentOrOfficeInfo
    /// </summary>
    public record Address
    {
        public string Name { get; internal init; }
        public string Company { get; internal init; }
        
        /// <summary>
        /// todo: value object
        /// </summary>
        public string PhoneNumber { get; internal init; }

        /// <summary>
        /// todo: value object
        /// </summary>
        public string EmailAddress { get; internal init; }
        public string StreetName { get; internal init; }
        public string StreetNumber { get; internal init; }
        public string ApartmentOrOfficeInfo { get; internal init; }
        public string Postcode { get; internal init; }
        public string PostTown { get; internal init; }
        public string Country { get; internal init; }
        public bool? IsResidential { get; internal init; }

        public Address(
            string name,
            string company,
            string phoneNumber,
            string emailAddress,
            string streetName,
            string streetNumber,
            string apartmentOrOfficeInfo, //floor, apartment number, etc.
            string postcode,
            string postTown,
            string country,
            bool? isResidential = null)
        {

            //TODO: validations.. 

            Name = name;
            Company = company;
            PhoneNumber = phoneNumber;
            EmailAddress = emailAddress;
            StreetName = streetName;
            StreetNumber = streetNumber;
            ApartmentOrOfficeInfo = apartmentOrOfficeInfo;
            Postcode = postcode;
            PostTown = postTown;
            Country = country;
            IsResidential = isResidential;
        }
        
    }
}
