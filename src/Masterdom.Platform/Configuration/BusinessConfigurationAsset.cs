namespace Masterdom.Platform.Configuration;

public sealed record BusinessConfigurationAsset<TPayload>(
    BusinessConfigurationMetadata Metadata,
    TPayload Payload);
