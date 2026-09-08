using System.Text.Json;
using OSDC.Drilling.WellBoreArchitecture.ModelShared;

namespace ServiceTest;

[TestFixture]
public class RigContractDeserializationTests
{
    [Test]
    public void TopDrive_AllowsNullControllerTypeFromRigService()
    {
        TopDrive? topDrive = JsonSerializer.Deserialize<TopDrive>(
            """{"TopDriveControllerType":null}""");

        Assert.That(topDrive, Is.Not.Null);
        Assert.That(topDrive!.TopDriveControllerType, Is.Null);
    }
}
