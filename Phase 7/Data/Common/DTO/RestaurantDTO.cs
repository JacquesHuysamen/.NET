﻿namespace Pezza.Common.DTO
{
    using System;
    using Pezza.Common.Entities;

    public class RestaurantDTO : Entity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureUrl { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Province { get; set; }

        public string PostalCode { get; set; }

        public bool? IsActive { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
