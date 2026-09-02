using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

public enum WellBoreArchitectureExternalReferenceValidationStatus { Valid, Invalid, Unavailable }

public sealed class WellBoreArchitectureExternalReferenceIssue
{
    public string Property { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class WellBoreArchitectureExternalReferenceValidation
{
    public Guid WellBoreArchitectureID { get; set; }
    public Guid? WellBoreID { get; set; }
    public bool? WellBoreExists { get; set; }
    public WellBoreArchitectureExternalReferenceValidationStatus Status { get; set; }
    public DateTimeOffset CheckedAtUtc { get; set; }
    public List<WellBoreArchitectureExternalReferenceIssue> Issues { get; set; } = [];
}

public enum WellBoreArchitectureExternalReferenceAuditScope { All, Selected }

public sealed class WellBoreArchitectureExternalReferenceAuditRequest
{
    [JsonRequired]
    public WellBoreArchitectureExternalReferenceAuditScope Scope { get; set; }
    public List<Guid>? WellBoreArchitectureIDs { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; } = 100;
}

public sealed class WellBoreArchitectureExternalReferenceAuditResult
{
    public DateTimeOffset CheckedAtUtc { get; set; }
    public int Total { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
    public int ValidCount { get; set; }
    public int InvalidCount { get; set; }
    public int UnavailableCount { get; set; }
    public List<WellBoreArchitectureExternalReferenceValidation> Items { get; set; } = [];
}
