using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OSDC.Drilling.WellBoreArchitecture.ModelShared;

/// <summary>Collects bounded discovery pages. Reference projections are for display/context only;
/// fetch a selected full record before editing or accessing engineering collections.</summary>
public static class DiscoveryClientExtensions
{
    public static async Task<List<SlotLight>> ListClusterSlotsLightAsync(this Client client, System.Guid clusterId, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<SlotLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SlotDiscoveryPage page = await client.DiscoverClusterSlotsAsync(offset: offset, limit: 200, clusterId: clusterId, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static async Task<List<ClusterLight>> ListClusterLightAsync(this Client client, string? name = null, System.Guid? fieldId = null, System.Guid? rigId = null, bool? isSingleWell = null, bool? isFixedPlatform = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<ClusterLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ClusterDiscoveryPage page = await client.DiscoverClusterAsync(offset: offset, limit: 200, name: name, fieldId: fieldId, rigId: rigId, isSingleWell: isSingleWell, isFixedPlatform: isFixedPlatform, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<ClusterLight>> ListClusterLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListClusterLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<Cluster>> ListClusterReferencesAsync(this Client client, string? name = null, System.Guid? fieldId = null, System.Guid? rigId = null, bool? isSingleWell = null, bool? isFixedPlatform = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListClusterLightAsync(name: name, fieldId: fieldId, rigId: rigId, isSingleWell: isSingleWell, isFixedPlatform: isFixedPlatform, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<Cluster>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<Cluster>> ListClusterReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListClusterReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<Cluster> LoadClusterDetailsAsync(this Client client, IList<Cluster> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetClusterByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<ClusterFeatureCategoryLight>> ListClusterFeatureCategoryLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<ClusterFeatureCategoryLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ClusterFeatureCategoryDiscoveryPage page = await client.DiscoverClusterFeatureCategoryAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<ClusterFeatureCategoryLight>> ListClusterFeatureCategoryLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListClusterFeatureCategoryLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<ClusterFeatureCategory>> ListClusterFeatureCategoryReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListClusterFeatureCategoryLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<ClusterFeatureCategory>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<ClusterFeatureCategory>> ListClusterFeatureCategoryReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListClusterFeatureCategoryReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<ClusterFeatureCategory> LoadClusterFeatureCategoryDetailsAsync(this Client client, IList<ClusterFeatureCategory> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetClusterFeatureCategoryByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<ClusterIdentityLight>> ListClusterIdentityLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<ClusterIdentityLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ClusterIdentityDiscoveryPage page = await client.DiscoverClusterIdentityAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<ClusterIdentityLight>> ListClusterIdentityLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListClusterIdentityLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<ClusterIdentity>> ListClusterIdentityReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListClusterIdentityLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<ClusterIdentity>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<ClusterIdentity>> ListClusterIdentityReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListClusterIdentityReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<ClusterIdentity> LoadClusterIdentityDetailsAsync(this Client client, IList<ClusterIdentity> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetClusterIdentityByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<SlotFeatureCategoryLight>> ListSlotFeatureCategoryLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<SlotFeatureCategoryLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SlotFeatureCategoryDiscoveryPage page = await client.DiscoverSlotFeatureCategoryAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<SlotFeatureCategoryLight>> ListSlotFeatureCategoryLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListSlotFeatureCategoryLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<SlotFeatureCategory>> ListSlotFeatureCategoryReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListSlotFeatureCategoryLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<SlotFeatureCategory>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<SlotFeatureCategory>> ListSlotFeatureCategoryReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListSlotFeatureCategoryReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<SlotFeatureCategory> LoadSlotFeatureCategoryDetailsAsync(this Client client, IList<SlotFeatureCategory> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetSlotFeatureCategoryByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<FieldLight>> ListFieldLightAsync(this Client client, string? name = null, System.Guid? projectionDefinitionId = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<FieldLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FieldDiscoveryPage page = await client.DiscoverFieldAsync(offset: offset, limit: 200, name: name, projectionDefinitionId: projectionDefinitionId, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<FieldLight>> ListFieldLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<Field>> ListFieldReferencesAsync(this Client client, string? name = null, System.Guid? projectionDefinitionId = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListFieldLightAsync(name: name, projectionDefinitionId: projectionDefinitionId, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<Field>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<Field>> ListFieldReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<Field> LoadFieldDetailsAsync(this Client client, IList<Field> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetFieldByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<FieldDelineationLineTypeLight>> ListFieldDelineationLineTypeLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<FieldDelineationLineTypeLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FieldDelineationLineTypeDiscoveryPage page = await client.DiscoverFieldDelineationLineTypeAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<FieldDelineationLineTypeLight>> ListFieldDelineationLineTypeLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldDelineationLineTypeLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<FieldDelineationLineType>> ListFieldDelineationLineTypeReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListFieldDelineationLineTypeLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<FieldDelineationLineType>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<FieldDelineationLineType>> ListFieldDelineationLineTypeReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldDelineationLineTypeReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<FieldDelineationLineType> LoadFieldDelineationLineTypeDetailsAsync(this Client client, IList<FieldDelineationLineType> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetFieldDelineationLineTypeByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<FieldFeatureCategoryLight>> ListFieldFeatureCategoryLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<FieldFeatureCategoryLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FieldFeatureCategoryDiscoveryPage page = await client.DiscoverFieldFeatureCategoryAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<FieldFeatureCategoryLight>> ListFieldFeatureCategoryLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldFeatureCategoryLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<FieldFeatureCategory>> ListFieldFeatureCategoryReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListFieldFeatureCategoryLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<FieldFeatureCategory>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<FieldFeatureCategory>> ListFieldFeatureCategoryReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldFeatureCategoryReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<FieldFeatureCategory> LoadFieldFeatureCategoryDetailsAsync(this Client client, IList<FieldFeatureCategory> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetFieldFeatureCategoryByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<FieldIdentityLight>> ListFieldIdentityLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<FieldIdentityLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FieldIdentityDiscoveryPage page = await client.DiscoverFieldIdentityAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<FieldIdentityLight>> ListFieldIdentityLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldIdentityLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<FieldIdentity>> ListFieldIdentityReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListFieldIdentityLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<FieldIdentity>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<FieldIdentity>> ListFieldIdentityReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldIdentityReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<FieldIdentity> LoadFieldIdentityDetailsAsync(this Client client, IList<FieldIdentity> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetFieldIdentityByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<FieldMembershipCategoryLight>> ListFieldMembershipCategoryLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<FieldMembershipCategoryLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FieldMembershipCategoryDiscoveryPage page = await client.DiscoverFieldMembershipCategoryAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<FieldMembershipCategoryLight>> ListFieldMembershipCategoryLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldMembershipCategoryLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<FieldMembershipCategory>> ListFieldMembershipCategoryReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListFieldMembershipCategoryLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<FieldMembershipCategory>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<FieldMembershipCategory>> ListFieldMembershipCategoryReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListFieldMembershipCategoryReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<FieldMembershipCategory> LoadFieldMembershipCategoryDetailsAsync(this Client client, IList<FieldMembershipCategory> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetFieldMembershipCategoryByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<RigLight>> ListRigLightAsync(this Client client, string? name = null, System.Guid? clusterId = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<RigLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RigDiscoveryPage page = await client.DiscoverRigAsync(offset: offset, limit: 200, name: name, clusterId: clusterId, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<RigLight>> ListRigLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListRigLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<RigReadResponse>> ListRigReferencesAsync(this Client client, string? name = null, System.Guid? clusterId = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListRigLightAsync(name: name, clusterId: clusterId, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<RigReadResponse>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<RigReadResponse>> ListRigReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListRigReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<RigReadResponse> LoadRigDetailsAsync(this Client client, IList<RigReadResponse> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetRigByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<RigFeatureCategoryLight>> ListRigFeatureCategoryLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<RigFeatureCategoryLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RigFeatureCategoryDiscoveryPage page = await client.DiscoverRigFeatureCategoryAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<RigFeatureCategoryLight>> ListRigFeatureCategoryLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListRigFeatureCategoryLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<RigFeatureCategory>> ListRigFeatureCategoryReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListRigFeatureCategoryLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<RigFeatureCategory>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<RigFeatureCategory>> ListRigFeatureCategoryReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListRigFeatureCategoryReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<RigFeatureCategory> LoadRigFeatureCategoryDetailsAsync(this Client client, IList<RigFeatureCategory> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetRigFeatureCategoryByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<WellLight>> ListWellLightAsync(this Client client, string? name = null, System.Guid? slotId = null, System.Guid? clusterId = null, bool? isSingleWell = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<WellLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WellDiscoveryPage page = await client.DiscoverWellAsync(offset: offset, limit: 200, name: name, slotId: slotId, clusterId: clusterId, isSingleWell: isSingleWell, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<WellLight>> ListWellLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<Well>> ListWellReferencesAsync(this Client client, string? name = null, System.Guid? slotId = null, System.Guid? clusterId = null, bool? isSingleWell = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListWellLightAsync(name: name, slotId: slotId, clusterId: clusterId, isSingleWell: isSingleWell, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<Well>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<Well>> ListWellReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<Well> LoadWellDetailsAsync(this Client client, IList<Well> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetWellByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<WellFeatureCategoryLight>> ListWellFeatureCategoryLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<WellFeatureCategoryLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WellFeatureCategoryDiscoveryPage page = await client.DiscoverWellFeatureCategoryAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<WellFeatureCategoryLight>> ListWellFeatureCategoryLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellFeatureCategoryLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<WellFeatureCategory>> ListWellFeatureCategoryReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListWellFeatureCategoryLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<WellFeatureCategory>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<WellFeatureCategory>> ListWellFeatureCategoryReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellFeatureCategoryReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<WellFeatureCategory> LoadWellFeatureCategoryDetailsAsync(this Client client, IList<WellFeatureCategory> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetWellFeatureCategoryByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<WellIdentityLight>> ListWellIdentityLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<WellIdentityLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WellIdentityDiscoveryPage page = await client.DiscoverWellIdentityAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<WellIdentityLight>> ListWellIdentityLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellIdentityLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<WellIdentity>> ListWellIdentityReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListWellIdentityLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<WellIdentity>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<WellIdentity>> ListWellIdentityReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellIdentityReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<WellIdentity> LoadWellIdentityDetailsAsync(this Client client, IList<WellIdentity> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetWellIdentityByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<WellBoreLight>> ListWellBoreLightAsync(this Client client, string? name = null, System.Guid? wellId = null, System.Guid? rigId = null, System.Guid? parentWellBoreId = null, bool? isSidetrack = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<WellBoreLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WellBoreDiscoveryPage page = await client.DiscoverWellBoreAsync(offset: offset, limit: 200, name: name, wellId: wellId, rigId: rigId, parentWellBoreId: parentWellBoreId, isSidetrack: isSidetrack, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<WellBoreLight>> ListWellBoreLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<WellBore>> ListWellBoreReferencesAsync(this Client client, string? name = null, System.Guid? wellId = null, System.Guid? rigId = null, System.Guid? parentWellBoreId = null, bool? isSidetrack = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListWellBoreLightAsync(name: name, wellId: wellId, rigId: rigId, parentWellBoreId: parentWellBoreId, isSidetrack: isSidetrack, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<WellBore>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<WellBore>> ListWellBoreReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<WellBore> LoadWellBoreDetailsAsync(this Client client, IList<WellBore> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetWellBoreByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<WellBoreFeatureCategoryLight>> ListWellBoreFeatureCategoryLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<WellBoreFeatureCategoryLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WellBoreFeatureCategoryDiscoveryPage page = await client.DiscoverWellBoreFeatureCategoryAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<WellBoreFeatureCategoryLight>> ListWellBoreFeatureCategoryLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreFeatureCategoryLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<WellBoreFeatureCategory>> ListWellBoreFeatureCategoryReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListWellBoreFeatureCategoryLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<WellBoreFeatureCategory>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<WellBoreFeatureCategory>> ListWellBoreFeatureCategoryReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreFeatureCategoryReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<WellBoreFeatureCategory> LoadWellBoreFeatureCategoryDetailsAsync(this Client client, IList<WellBoreFeatureCategory> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetWellBoreFeatureCategoryByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<WellBoreIdentityLight>> ListWellBoreIdentityLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<WellBoreIdentityLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WellBoreIdentityDiscoveryPage page = await client.DiscoverWellBoreIdentityAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<WellBoreIdentityLight>> ListWellBoreIdentityLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreIdentityLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<WellBoreIdentity>> ListWellBoreIdentityReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListWellBoreIdentityLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<WellBoreIdentity>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<WellBoreIdentity>> ListWellBoreIdentityReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreIdentityReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<WellBoreIdentity> LoadWellBoreIdentityDetailsAsync(this Client client, IList<WellBoreIdentity> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetWellBoreIdentityByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<WellBoreArchitectureLight>> ListWellBoreArchitectureLightAsync(this Client client, string? name = null, System.Guid? wellBoreId = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<WellBoreArchitectureLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WellBoreArchitectureDiscoveryPage page = await client.DiscoverWellBoreArchitectureAsync(offset: offset, limit: 200, name: name, wellBoreId: wellBoreId, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<WellBoreArchitectureLight>> ListWellBoreArchitectureLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreArchitectureLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<WellBoreArchitecture>> ListWellBoreArchitectureReferencesAsync(this Client client, string? name = null, System.Guid? wellBoreId = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListWellBoreArchitectureLightAsync(name: name, wellBoreId: wellBoreId, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<WellBoreArchitecture>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<WellBoreArchitecture>> ListWellBoreArchitectureReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreArchitectureReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<WellBoreArchitecture> LoadWellBoreArchitectureDetailsAsync(this Client client, IList<WellBoreArchitecture> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetWellBoreArchitectureByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<WellBoreArchitectureFeatureCategoryLight>> ListWellBoreArchitectureFeatureCategoryLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<WellBoreArchitectureFeatureCategoryLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WellBoreArchitectureFeatureCategoryDiscoveryPage page = await client.DiscoverWellBoreArchitectureFeatureCategoryAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<WellBoreArchitectureFeatureCategoryLight>> ListWellBoreArchitectureFeatureCategoryLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreArchitectureFeatureCategoryLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<WellBoreArchitectureFeatureCategory>> ListWellBoreArchitectureFeatureCategoryReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListWellBoreArchitectureFeatureCategoryLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<WellBoreArchitectureFeatureCategory>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<WellBoreArchitectureFeatureCategory>> ListWellBoreArchitectureFeatureCategoryReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreArchitectureFeatureCategoryReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<WellBoreArchitectureFeatureCategory> LoadWellBoreArchitectureFeatureCategoryDetailsAsync(this Client client, IList<WellBoreArchitectureFeatureCategory> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetWellBoreArchitectureFeatureCategoryByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    public static async Task<List<WellBoreArchitectureIdentityLight>> ListWellBoreArchitectureIdentityLightAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var items = new List<WellBoreArchitectureIdentityLight>();
        int offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WellBoreArchitectureIdentityDiscoveryPage page = await client.DiscoverWellBoreArchitectureIdentityAsync(offset: offset, limit: 200, name: name, cancellationToken: cancellationToken);
            items.AddRange(page.Items);
            if (page.NextOffset is not int next) return items;
            if (next <= offset) throw new InvalidOperationException("Discovery pagination did not advance.");
            offset = next;
        }
    }

    public static Task<List<WellBoreArchitectureIdentityLight>> ListWellBoreArchitectureIdentityLightAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreArchitectureIdentityLightAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Light fields projected into the existing UI reference type. Never submit these partial references as replacements.</summary>
    public static async Task<List<WellBoreArchitectureIdentity>> ListWellBoreArchitectureIdentityReferencesAsync(this Client client, string? name = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        var summaries = await client.ListWellBoreArchitectureIdentityLightAsync(name: name, cancellationToken: cancellationToken);
        return summaries.ConvertAll(summary => JsonSerializer.Deserialize<WellBoreArchitectureIdentity>(JsonSerializer.Serialize(summary))
            ?? throw new JsonException("A discovery reference is null."));
    }

    public static Task<List<WellBoreArchitectureIdentity>> ListWellBoreArchitectureIdentityReferencesAsync(this Client client, CancellationToken cancellationToken) =>
        client.ListWellBoreArchitectureIdentityReferencesAsync(cancellationToken: cancellationToken, name: null);

    /// <summary>Replace one reference with its complete selected record before editing or reading nested data.</summary>
    public static async Task<WellBoreArchitectureIdentity> LoadWellBoreArchitectureIdentityDetailsAsync(this Client client, IList<WellBoreArchitectureIdentity> references, Guid id, CancellationToken cancellationToken = default)
    {
        var value = await client.GetWellBoreArchitectureIdentityByIdAsync(id, cancellationToken: cancellationToken);
        for (int index = 0; index < references.Count; index++)
            if (DiscoveryIdentifier(references[index]) == id) { references[index] = value; return value; }
        references.Add(value);
        return value;
    }

    private static Guid? DiscoveryIdentifier(object value)
    {
        object metadata = value.GetType().GetProperty("MetaInfo")?.GetValue(value);
        return (metadata?.GetType().GetProperty("ID")?.GetValue(metadata) ?? value.GetType().GetProperty("ID")?.GetValue(value)) as Guid?;
    }
}
