using System;
using System.Collections.Generic;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

public enum WellBoreArchitectureBatchExportScope { Unspecified = 0, All = 1, Selected = 2 }

public sealed class WellBoreArchitectureBatchExportRequest
{
    public WellBoreArchitectureBatchExportScope Scope { get; set; }
    public List<Guid>? WellBoreArchitectureIDs { get; set; }
}

/// <summary>A portable, versioned logical backup with its referenced local catalogs.</summary>
public sealed class WellBoreArchitectureBatchExportDocument
{
    public const string CurrentFormatIdentifier = "OSDC.Drilling.WellBoreArchitecture.BatchExport";
    public const int CurrentSchemaVersion = 1;
    public string FormatIdentifier { get; set; } = CurrentFormatIdentifier;
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public DateTimeOffset ExportedAtUtc { get; set; }
    public WellBoreArchitectureBatchCatalogDependencies CatalogDependencies { get; set; } = new();
    public List<WellBoreArchitecture> WellBoreArchitectures { get; set; } = [];
}

public sealed class WellBoreArchitectureBatchCatalogDependencies
{
    public List<WellBoreArchitectureIdentity> Identities { get; set; } = [];
    public List<WellBoreArchitectureFeatureCategory> FeatureCategories { get; set; } = [];
}

public sealed class WellBoreArchitectureBatchErrorEnvelope
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<WellBoreArchitectureBatchError> Errors { get; set; } = [];
}

public sealed class WellBoreArchitectureBatchError
{
    public int? PositionIndex { get; set; }
    public string Property { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public enum WellBoreArchitectureBatchRestoreConflictPolicy { Unspecified = 0, FailIfExists = 1, ReplaceExisting = 2 }
public enum WellBoreArchitectureBatchCatalogRestorePolicy { Unspecified = 0, MapExisting = 1, MapOrCreateMissing = 2 }

public sealed class WellBoreArchitectureBatchRestoreRequest
{
    public WellBoreArchitectureBatchRestoreConflictPolicy ConflictPolicy { get; set; }
    public WellBoreArchitectureBatchCatalogRestorePolicy CatalogPolicy { get; set; }
    /// <summary>
    /// Explicitly permits normalized-name catalogue mapping when source and local UUIDs differ.
    /// False is the safe default; exact UUID matches and newly created definitions preserve source IDs.
    /// </summary>
    public bool AllowNormalizedNameMapping { get; set; }
    public WellBoreArchitectureBatchExportDocument? Document { get; set; }
}

public sealed class WellBoreArchitectureBatchRestoreResponse
{
    public DateTimeOffset RestoredAtUtc { get; set; }
    public int CreatedCount { get; set; }
    public int ReplacedCount { get; set; }
    public int CreatedCatalogDefinitionCount { get; set; }
    public int CreatedCatalogOptionCount { get; set; }
    public List<WellBoreArchitectureBatchCatalogMapping> CatalogMappings { get; set; } = [];
    public List<Guid> WellBoreArchitectureIDs { get; set; } = [];
}

public sealed class WellBoreArchitectureBatchCatalogMapping
{
    public string Catalog { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid SourceID { get; set; }
    public Guid LocalID { get; set; }
    public string Resolution { get; set; } = string.Empty;
}
