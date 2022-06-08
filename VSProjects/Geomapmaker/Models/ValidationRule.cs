using System.Collections.Generic;

namespace Geomapmaker.Models
{
    public class ValidationRule
    {
        public string Description { get; set; }

        public ValidationStatus Status { get; set; } = ValidationStatus.Skipped;

        public List<string> Errors { get; set; } = new List<string>();
    }

    public enum ValidationStatus
    {
        Skipped,
        Passed,
        Failed
    }
}
