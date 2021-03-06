﻿using Net.Core.EntityModels.Core;

namespace Net.Core.EntityModels.Location
{
    public class City : IdentityColumnEntity
    {
        #region Scalar Properties
        public long CountryId { get; set; }
        public string CityName { get; set; }
        public bool IsActive { get; set; }
        #endregion

        public virtual Country Country { get; set; }
    }
}
