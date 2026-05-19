using System.Net;
using System.Net.Http.Json;

namespace Barkfest.API.Tests.Controllers;

public class AdminControllerTests(BarkfestApiFactory factory)
    : IClassFixture<BarkfestApiFactory>
{
    private readonly HttpClient _unauthenticatedClient = factory.CreateClient();

    // -----------------------------------------------------------------------
    // PATCH /v1/admin/owners/{id}/active — auth
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SetOwnerActive_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.PatchAsJsonAsync(
            $"/v1/admin/owners/{Guid.NewGuid()}/active",
            new { active = false });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetOwnerActive_When_CallerIsNotAdmin_Returns_Forbidden()
    {
        var ownerId = await RegisterOwnerAsync();
        var nonAdminClient = factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await nonAdminClient.PatchAsJsonAsync(
            $"/v1/admin/owners/{ownerId}/active",
            new { active = false });

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetOwnerActive_When_AdminDeactivatesOwner_Returns_NoContent()
    {
        var ownerId = await RegisterOwnerAsync();
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());

        var response = await adminClient.PatchAsJsonAsync(
            $"/v1/admin/owners/{ownerId}/active",
            new { active = false });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetOwnerActive_When_AdminReactivatesOwner_Returns_NoContent()
    {
        var ownerId = await RegisterOwnerAsync();
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());

        // Deactivate first
        await adminClient.PatchAsJsonAsync(
            $"/v1/admin/owners/{ownerId}/active",
            new { active = false });

        // Then reactivate
        var response = await adminClient.PatchAsJsonAsync(
            $"/v1/admin/owners/{ownerId}/active",
            new { active = true });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetOwnerActive_When_OwnerNotFound_Returns_NotFound()
    {
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());

        var response = await adminClient.PatchAsJsonAsync(
            $"/v1/admin/owners/{Guid.NewGuid()}/active",
            new { active = false });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AuthenticatedRequest_When_OwnerIsInactive_Returns_Forbidden()
    {
        // Register owner and get a valid token for them
        var ownerId = await RegisterOwnerAsync("inactive-mid-flight");
        var ownerClient = factory.CreateAuthenticatedClient(ownerId);

        // Confirm the token works before deactivation
        var beforeResponse = await ownerClient.GetAsync($"/v1/owners/{ownerId}");
        beforeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Admin deactivates the owner
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());
        await adminClient.PatchAsJsonAsync(
            $"/v1/admin/owners/{ownerId}/active",
            new { active = false });

        // Same valid token — now rejected by ActiveOwnerMiddleware
        var afterResponse = await ownerClient.GetAsync($"/v1/owners/{ownerId}");
        afterResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Login_When_OwnerIsInactive_Returns_Forbidden()
    {
        var username = $"inactive{Guid.NewGuid():N}";
        const string password = "SecurePass1!";

        // Register owner
        var registerResponse = await _unauthenticatedClient.PostAsJsonAsync("/v1/auth/register", new
        {
            username,
            firstName = "In",
            lastName = "Active",
            email = $"inactive-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password
        });
        var ownerId = Guid.Parse(
            registerResponse.Headers.Location!.ToString().Split('/').Last());

        // Admin deactivates the owner
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());
        await adminClient.PatchAsJsonAsync(
            $"/v1/admin/owners/{ownerId}/active",
            new { active = false });

        // Attempt to login
        var loginResponse = await _unauthenticatedClient.PostAsJsonAsync("/v1/auth/login", new
        {
            username,
            password
        });

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    // -----------------------------------------------------------------------
    // PATCH /v1/admin/admins/{id}/password — update administrator password
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateAdministratorPassword_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.PatchAsJsonAsync(
            $"/v1/admin/admins/{Guid.NewGuid()}/password",
            new { newPassword = "NewPass1!" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAdministratorPassword_When_CallerIsNotAdmin_Returns_Forbidden()
    {
        var ownerClient = factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await ownerClient.PatchAsJsonAsync(
            $"/v1/admin/admins/{Guid.NewGuid()}/password",
            new { newPassword = "NewPass1!" });

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAdministratorPassword_When_AdminNotFound_Returns_NotFound()
    {
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());

        var response = await adminClient.PatchAsJsonAsync(
            $"/v1/admin/admins/{Guid.NewGuid()}/password",
            new { newPassword = "NewPass1!" });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAdministratorPassword_When_AdminUpdatesPassword_Returns_NoContent()
    {
        // Create a second admin to update
        var callerAdminId = Guid.NewGuid();
        var adminClient = factory.CreateAuthenticatedAdminClient(callerAdminId);

        var createResponse = await adminClient.PostAsJsonAsync("/v1/admin/admins", new
        {
            username = $"a{Guid.NewGuid():N}",
            name = "Target Admin",
            email = $"target-{Guid.NewGuid():N}@barkfest.dev",
            phoneNumber = "+15555550100",
            password = "OldPass1!"
        });
        createResponse.EnsureSuccessStatusCode();
        var targetId = Guid.Parse(createResponse.Headers.Location!.ToString().Split('/').Last());

        var response = await adminClient.PatchAsJsonAsync(
            $"/v1/admin/admins/{targetId}/password",
            new { newPassword = "NewPass1!" });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // -----------------------------------------------------------------------
    // DELETE /v1/admin/admins/{id} — delete administrator
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DeleteAdministrator_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.DeleteAsync(
            $"/v1/admin/admins/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAdministrator_When_CallerIsNotAdmin_Returns_Forbidden()
    {
        var ownerClient = factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await ownerClient.DeleteAsync(
            $"/v1/admin/admins/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAdministrator_When_AdminDeletesSelf_Returns_Forbidden()
    {
        var adminId = Guid.NewGuid();
        var adminClient = factory.CreateAuthenticatedAdminClient(adminId);

        var response = await adminClient.DeleteAsync($"/v1/admin/admins/{adminId}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAdministrator_When_AdminNotFound_Returns_NotFound()
    {
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());

        var response = await adminClient.DeleteAsync(
            $"/v1/admin/admins/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAdministrator_When_AdminDeletesOther_Returns_NoContent()
    {
        var callerAdminId = Guid.NewGuid();
        var adminClient = factory.CreateAuthenticatedAdminClient(callerAdminId);

        // Create a second admin to delete
        var createResponse = await adminClient.PostAsJsonAsync("/v1/admin/admins", new
        {
            username = $"a{Guid.NewGuid():N}",
            name = "To Delete Admin",
            email = $"todelete-{Guid.NewGuid():N}@barkfest.dev",
            phoneNumber = "+15555550100",
            password = "TempPass1!"
        });
        createResponse.EnsureSuccessStatusCode();
        var targetId = Guid.Parse(createResponse.Headers.Location!.ToString().Split('/').Last());

        var response = await adminClient.DeleteAsync($"/v1/admin/admins/{targetId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // -----------------------------------------------------------------------
    // POST /v1/admin/admins — create admin
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateAdmin_When_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _unauthenticatedClient.PostAsJsonAsync("/v1/admin/admins", new
        {
            username = $"a{Guid.NewGuid():N}",
            name = "New Admin",
            email = $"new-{Guid.NewGuid():N}@barkfest.dev",
            phoneNumber = "+15555550100",
            password = "AdminPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAdmin_When_CallerIsNotAdmin_Returns_Forbidden()
    {
        var ownerClient = factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await ownerClient.PostAsJsonAsync("/v1/admin/admins", new
        {
            username = $"a{Guid.NewGuid():N}",
            name = "New Admin",
            email = $"new-{Guid.NewGuid():N}@barkfest.dev",
            phoneNumber = "+15555550100",
            password = "AdminPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAdmin_When_AdminCreatesAdmin_Returns_Created()
    {
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());

        var response = await adminClient.PostAsJsonAsync("/v1/admin/admins", new
        {
            username = $"a{Guid.NewGuid():N}",
            name = "New Admin",
            email = $"new-{Guid.NewGuid():N}@barkfest.dev",
            phoneNumber = "+15555550100",
            password = "AdminPass1!"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAdmin_When_EmailAlreadyInUse_Returns_BadRequest()
    {
        var adminClient = factory.CreateAuthenticatedAdminClient(Guid.NewGuid());
        var email = $"dup-admin-{Guid.NewGuid():N}@barkfest.dev";

        await adminClient.PostAsJsonAsync("/v1/admin/admins", new { username = $"a{Guid.NewGuid():N}", name = "New Admin", email, phoneNumber = "+15555550100", password = "AdminPass1!" });

        var response = await adminClient.PostAsJsonAsync("/v1/admin/admins", new { username = $"a{Guid.NewGuid():N}", name = "New Admin", email, phoneNumber = "+15555550100", password = "AdminPass1!" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<Guid> RegisterOwnerAsync(string prefix = "admin-test")
    {
        var response = await _unauthenticatedClient.PostAsJsonAsync("/v1/auth/register", new
        {
            username = $"u{Guid.NewGuid():N}",
            firstName = "Test",
            lastName = "User",
            email = $"{prefix}-{Guid.NewGuid():N}@example.com",
            phoneNumber = (string?)null,
            password = "SecurePass1!"
        });
        response.EnsureSuccessStatusCode();
        return Guid.Parse(response.Headers.Location!.ToString().Split('/').Last());
    }
}
