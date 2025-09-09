using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Domain_Models
{
    public class ReferenceThreshold : BaseEntity
    {
        [Required] public Technology Technology { get; set; }

        [Required] public Scope Scope { get; set; }

        /// Optional identifier for the scope (e.g., MunicipalityId / SettlementId / TransmitterLocation text).
        public string? ScopeIdentifier { get; set; }

        /// For DIGITAL_TV entries when IsTvChannel=true on the related frequency concept.
        public int? ChannelNumber { get; set; }

        /// For FM entries (MHz).
        public float? FrequencyMHz { get; set; }

        [Required] public float MinDbuVPerM { get; set; }
        [Required] public float MaxDbuVPerM { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Notes { get; set; }
    }
}
