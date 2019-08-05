using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using Brixworth.ServiceIntegrationTesting;
using Microsoft.IdentityModel.JsonWebTokens;
using User.DataAccess.DbContexts;
using User.DataAccess.Entities;
using User.Domain.Constants;
using User.Domain.Models;
using Xunit;

namespace User.Service.Tests
{
    public class ControllerIntegrationTests
    {
        #region PostUserPreferencesAsync

        [Fact]
        public async void PostUserPreferencesAsync_ValidAuthInvalidModel_ReturnsBadRequest()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = AdminIdentity;

            var model = new PreferencesModel();
            var request = CreatePostPreferencesRequest(token, model);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            RefreshAll(context);
            Assert.Equal(0, context.Preferences.Count());
        }

        [Fact]
        public async void PostUserPreferencesAsync_InvalidAuthValidModel_ReturnsForbidden()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = UserIdentity;

            var model = MakePreferencesModel(Guid.NewGuid());
            var request = CreatePostPreferencesRequest(token, model);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            RefreshAll(context);
            Assert.Equal(0, context.Preferences.Count());
        }

        [Fact]
        public async void PostUserPreferencesAsync_ExpiredAuthValidModel_ReturnsUnauthorized()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = ExpiredIdentity;

            var model = MakePreferencesModel(Guid.NewGuid());
            var request = CreatePostPreferencesRequest(token, model);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            RefreshAll(context);
            Assert.Equal(0, context.Preferences.Count());
        }

        [Fact]
        public async void PostUserPreferencesAsync_ValidAuthValidModel_ReturnsOk()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = AdminIdentity;

            var model = MakePreferencesModel(Guid.NewGuid());
            var request = CreatePostPreferencesRequest(token, model);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            RefreshAll(context);
            Assert.Single(context.Preferences.Where(p => p.UserId == model.UserId).ToList());
        }

        [Fact]
        public async void PostUserPreferencesAsync_ValidAuthValidModelExistingUser_ReturnsUnauthorized()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = AdminIdentity;

            var model = MakePreferencesModel(Guid.NewGuid());
            var request1 = CreatePostPreferencesRequest(token, model);
            var request2 = CreatePostPreferencesRequest(token, model);

            // When
            var response1 = await client.SendAsync(request1);
            var response2 = await client.SendAsync(request2);

            // Then
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
            RefreshAll(context);
            Assert.Single(context.Preferences.Where(p => p.UserId == model.UserId).ToList());
            Assert.Equal(UserConstants.UserExists, await response2.Content.ReadAsStringAsync());
        }

        #endregion

        #region GetUserPreferencesAsync

        [Fact]
        public async void GetUserPreferencesAsync_ExpiredAuth_ReturnsUnauthorized()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = ExpiredIdentity;

            var request = CreateGetPreferencesRequest(token, Guid.NewGuid());

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async void GetUserPreferencesAsync_ValidAuthWrongUser_ReturnsForbid()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = UserIdentity;

            var request = CreateGetPreferencesRequest(token, Guid.NewGuid());

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async void GetUserPreferencesAsync_ValidAuthNoData_ReturnsNotFound()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = AdminIdentity;

            var request = CreateGetPreferencesRequest(token, Guid.NewGuid());

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void GetUserPreferencesAsync_ValidAuthWithData_ReturnsOk()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = UserIdentity;

            var language = "DK";
            var existingPreference = CreateUserPreference(userId, language);
            context.Preferences.Add(existingPreference);
            context.SaveChanges();

            var request = CreateGetPreferencesRequest(token, userId);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.Content.ReadAsAsync<PreferencesModel>().Result;
            Assert.Equal(userId, result.UserId);
            Assert.Equal(language, result.Language);
        }

        #endregion

        #region PutUserPreferencesAsync

        [Fact]
        public async void PutUserPreferencesAsync_ValidAuthInvalidModel_ReturnsBadRequest()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = AdminIdentity;

            var model = new PutPreferencesModel();
            var request = CreatePutPreferencesRequest(token, userId, model);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            RefreshAll(context);
            Assert.Equal(0, context.Preferences.Count());
        }

        [Fact]
        public async void PutUserPreferencesAsync_ExpiredAuth_ReturnsUnauthorized()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = ExpiredIdentity;

            var model = MakePutPreferencesModel("DK");
            var request = CreatePutPreferencesRequest(token, Guid.NewGuid(), model);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async void PutUserPreferencesAsync_ValidAuthWrongUser_ReturnsForbid()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = UserIdentity;

            var model = MakePutPreferencesModel("DK");
            var request = CreatePutPreferencesRequest(token, Guid.NewGuid(), model);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async void PutUserPreferencesAsync_ValidAuthNoData_ReturnsNotFound()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = AdminIdentity;

            var model = MakePutPreferencesModel("DK");
            var request = CreatePutPreferencesRequest(token, userId, model);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void PutUserPreferencesAsync_ValidAuthWithData_ReturnsOk()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = UserIdentity;

            var language = "DK";
            var existingPreference = CreateUserPreference(userId, language);
            context.Preferences.Add(existingPreference);
            context.SaveChanges();

            var model = MakePutPreferencesModel("GB");
            var request = CreatePutPreferencesRequest(token, userId, model);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            RefreshAll(context);
            Assert.Single(context.Preferences.Where(p => p.UserId == userId && p.PreferredLanguage == model.Language).ToList());
        }

        #endregion

        #region DeleteUserPreferencesAsync

        [Fact]
        public async void DeleteUserPreferencesAsync_InvalidAuth_ReturnsForbidden()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = UserIdentity;

            var existingPreference = CreateUserPreference(userId, "DK");
            context.Preferences.Add(existingPreference);
            context.SaveChanges();

            var request = CreateDeletePreferencesRequest(token, userId);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            RefreshAll(context);
            Assert.Single(context.Preferences.Where(p => p.UserId == userId && !p.Deleted).ToList());
        }

        [Fact]
        public async void DeleteUserPreferencesAsync_ExpiredAuth_ReturnsUnauthorized()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = ExpiredIdentity;

            var existingPreference = CreateUserPreference(userId, "DK");
            context.Preferences.Add(existingPreference);
            context.SaveChanges();

            var request = CreateDeletePreferencesRequest(token, userId);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            RefreshAll(context);
            Assert.Single(context.Preferences.Where(p => p.UserId == userId && !p.Deleted).ToList());
        }

        [Fact]
        public async void DeleteUserPreferencesAsync_ValidAuthNoData_ReturnsOk()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = AdminIdentity;

            var request = CreateDeletePreferencesRequest(token, Guid.NewGuid());

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async void DeleteUserPreferencesAsync_ValidAuthDeletedData_ReturnsOk()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = AdminIdentity;

            var existingPreference = CreateUserPreference(userId, "DK", true);
            context.Preferences.Add(existingPreference);
            context.SaveChanges();

            var request = CreateDeletePreferencesRequest(token, userId);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            RefreshAll(context);
            Assert.Single(context.Preferences.Where(p => p.UserId == userId && p.Deleted).ToList());
        }

        [Fact]
        public async void DeleteUserPreferencesAsync_ValidAuthWithData_ReturnsOk()
        {
            // Given
            var databaseName = Guid.NewGuid().ToString();
            var (client, context) = ServiceTestingHelper.BuildServer<Startup, UserDbContext>(databaseName);
            var (token, userId) = AdminIdentity;

            var language = "DK";
            var existingPreference = CreateUserPreference(userId, language);
            context.Preferences.Add(existingPreference);
            context.SaveChanges();

            var request = CreateDeletePreferencesRequest(token, userId);

            // When
            var response = await client.SendAsync(request);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            RefreshAll(context);
            Assert.Single(context.Preferences.Where(p => p.UserId == userId && p.Deleted).ToList());
        }

        #endregion
        
        #region Test Helpers

        private (string, Guid) AdminIdentity => BuildIdentityDetails(true, false);
        private (string, Guid) UserIdentity => BuildIdentityDetails(false, false);
        private (string, Guid) ExpiredIdentity => BuildIdentityDetails(false, true);

        private (string token, Guid userId) BuildIdentityDetails(bool elevatedRights, bool expired)
        {
            var date = expired ? DateTime.UtcNow.AddMinutes(-10) : DateTime.UtcNow.AddDays(1);
            var userId = Guid.NewGuid();
            var claims = new[]
            {
                new Claim(ClaimTypes.Authentication, "true"),
                new Claim("userId", userId.ToString()),
                new Claim(ClaimTypes.Role, "user"),
                new Claim("ElevatedRights", elevatedRights.ToString(), "bool"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            };
            var token = ServiceTestingHelper.GenerateToken(claims, date);
            return (token, userId);
        }

        private HttpRequestMessage CreatePostPreferencesRequest(string token, PreferencesModel model)
        {
            return ServiceTestingHelper.BuildRequest(token, HttpMethod.Post, "/v1/user", model);
        }

        private HttpRequestMessage CreatePutPreferencesRequest(string token, Guid userId, PutPreferencesModel model)
        {
            return ServiceTestingHelper.BuildRequest(token, HttpMethod.Put, $"/v1/user/{userId}", model);
        }

        private HttpRequestMessage CreateGetPreferencesRequest(string token, Guid userId)
        {
            return ServiceTestingHelper.BuildRequest(token, HttpMethod.Get, $"/v1/user/{userId}", null);
        }

        private HttpRequestMessage CreateDeletePreferencesRequest(string token, Guid userId)
        {
            return ServiceTestingHelper.BuildRequest(token, HttpMethod.Delete, $"/v1/user/{userId}", null);
        }

        private static void RefreshAll(DbContext context)
        {
            foreach (var entity in context.ChangeTracker.Entries())
            {
                entity.Reload();
            }
        }

        private PreferencesModel MakePreferencesModel(Guid userId)
        {
            return new PreferencesModel
            {
                UserId = userId,
                Language = "GB"
            };
        }

        private PutPreferencesModel MakePutPreferencesModel(string language = "GB")
        {
            return new PutPreferencesModel
            {
                Language = language
            };
        }

        private Preference CreateUserPreference(Guid userPreferenceId, string language = "GB", bool deleted = false)
        {
            var userId = Guid.NewGuid();
            return new Preference
            {
                UserId = userPreferenceId,
                PreferredLanguage = language,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                EditedBy = userId,
                EditedOn = DateTime.UtcNow,
                Deleted = deleted
            };
        }

        #endregion
    }
}
