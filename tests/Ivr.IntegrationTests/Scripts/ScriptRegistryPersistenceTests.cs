using Ivr.Domain.Confirmation;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Persistence;
using Ivr.IntegrationTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Ivr.IntegrationTests.Scripts;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ScriptRegistryPersistenceTests(PostgresPersistenceFixture fixture)
{
    [Fact]
    [Trait("TestId", "IT-SCRIPT-SEED-07")]
    public async Task MigrationSeedsMockApprovalAndKeepsOtherModesClosed()
    {
        await fixture.ResetAsync();
        IScriptRegistry registry = fixture.Services.GetRequiredService<IScriptRegistry>();

        Assert.NotNull(await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock));
        Assert.Null(await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.LabRealSim));
        Assert.Null(await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.ProductionReal));
        Assert.NotNull(await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.PreviousMockTemplateVersion,
            ExecutionMode.Mock));
    }

    [Fact]
    [Trait("TestId", "IT-SCRIPT-PERSISTENCE-08")]
    public async Task LifecyclePersistsAuditAndDatabaseRejectsApprovedContentOrApprovalMutation()
    {
        await fixture.ResetAsync();
        IScriptContentManager manager = fixture.Services.GetRequiredService<IScriptContentManager>();
        IScriptRegistry registry = fixture.Services.GetRequiredService<IScriptRegistry>();
        ScriptDraftDefinition definition = ScriptDraftDefinition.Create(
            "SCRIPT-ORDER-CONFIRM",
            "v-db-2",
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
        await manager.CreateDraftAsync(
            definition,
            ScriptActor.Create("db-author", [ScriptPermissions.Edit]),
            "Create database script fixture",
            "corr-db-script-create");
        await manager.SubmitForReviewAsync(
            definition.Key,
            ScriptActor.Create("db-reviewer", [ScriptPermissions.Review]),
            "Review database script fixture",
            "corr-db-script-review");
        await manager.ApproveAsync(
            definition.Key,
            ScriptApprovalType.MockTest,
            ScriptActor.Create("db-approver", [ScriptPermissions.ApproveMock]),
            "Approve database MOCK fixture",
            "corr-db-script-approve");

        Assert.NotNull(await registry.TryGetApproved(
            definition.Key.TemplateId,
            definition.Key.Version,
            ExecutionMode.Mock));
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        Assert.Equal(3, await context.AuditLog.CountAsync(entry =>
            entry.TargetType == "SCRIPT_VERSION"
            && entry.TargetId == definition.Key.ToString()));

        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var command = new NpgsqlCommand(
                "UPDATE ivr_script_versions SET template_text = 'changed' "
                + "WHERE template_id = @template AND version = @version",
                connection);
            command.Parameters.AddWithValue("template", definition.Key.TemplateId);
            command.Parameters.AddWithValue("version", definition.Key.Version);
            await command.ExecuteNonQueryAsync();
        });
        await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await using var command = new NpgsqlCommand(
                "UPDATE ivr_script_approvals SET reason = 'changed' "
                + "WHERE script_version_id = (SELECT id FROM ivr_script_versions "
                + "WHERE template_id = @template AND version = @version)",
                connection);
            command.Parameters.AddWithValue("template", definition.Key.TemplateId);
            command.Parameters.AddWithValue("version", definition.Key.Version);
            await command.ExecuteNonQueryAsync();
        });
    }
}
