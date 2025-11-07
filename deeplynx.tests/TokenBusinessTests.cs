// TokenBusinessTests.cs

using System.IdentityModel.Tokens.Jwt;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class TokenBusinessTests : IntegrationTestBase
    {
        private Config _config;
        private TokenBusiness _tokenBusiness;

        public TokenBusinessTests(TestSuiteFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _config = new Config();
            _tokenBusiness = new TokenBusiness(_config, Context);
        }

        #region CreateToken Tests

        [Fact]
        public void CreateToken_ReturnsJwt_WhenVerifySucceeds_AndSecretExists()
        {
            // Arrange
            var email = $"tester-{Guid.NewGuid():N}@example.com";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = "Tester" };
            Context.Users.Add(user);
            Context.SaveChanges();

            var apiKey = "api-key-123";
            var plaintextSecret = "my-plaintext-secret";

            // Store the HASHED secret in the database
            var hashedSecret = _tokenBusiness.HashApiKey(plaintextSecret);
            Context.ApiKeys.Add(new ApiKey { Key = apiKey, Secret = hashedSecret, UserId = user.Id });
            Context.SaveChanges();

            // Set the JWT signing secret environment variable
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test-jwt-secret-key-min-32-chars");

            // Act - Pass the PLAINTEXT secret to CreateToken
            var jwt = _tokenBusiness.CreateToken(plaintextSecret, apiKey, expiration: 5);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(jwt));
            var handler = new JwtSecurityTokenHandler();
            var parsed = handler.ReadJwtToken(jwt);
            Assert.Equal(apiKey, parsed.Claims.First(c => c.Type == "apiKey").Value);
            Assert.True(parsed.ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public void CreateToken_Throws_WhenVerifyFails()
        {
            // Arrange
            var email = $"tester-{Guid.NewGuid():N}@example.com";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = "Tester" };
            Context.Users.Add(user);
            Context.SaveChanges();

            var apiKey = "api-key-XYZ";
            var correctSecret = "correct-secret";
            var hashedSecret = _tokenBusiness.HashApiKey(correctSecret);

            Context.ApiKeys.Add(new ApiKey { Key = apiKey, Secret = hashedSecret, UserId = user.Id });
            Context.SaveChanges();

            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test-jwt-secret-key-min-32-chars");

            // Pass wrong plaintext secret
            var wrongSecret = "wrong-secret";

            // Act & Assert
            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                _tokenBusiness.CreateToken(wrongSecret, apiKey, expiration: 5));
            Assert.Contains("Invalid API credentials", ex.Message);
        }

        [Fact]
        public void CreateToken_Throws_WhenApiKeyNotFound()
        {
            // Arrange
            var email = $"tester-{Guid.NewGuid():N}@example.com";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = "Tester" };
            Context.Users.Add(user);
            Context.SaveChanges();

            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test-jwt-secret-key-min-32-chars");

            var nonExistentApiKey = "does-not-exist";
            var someSecret = "any-secret";

            // Act & Assert
            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _tokenBusiness.CreateToken(someSecret, nonExistentApiKey, expiration: 5));
            Assert.Contains("API key not found", ex.Message);
        }

        #endregion

        #region GetApiKey Tests

        [Fact]
        public void GetApiKey_ReturnsKey_WhenUserExists()
        {
            // Arrange
            var email = $"tester-{Guid.NewGuid():N}@example.com";
            var name = "tester";
            var username = "tester";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = name, Username = username };
            Context.Users.Add(user);
            Context.SaveChanges();

            var apiKey = "getKey-1";
            var row = new ApiKey { Key = apiKey, Secret = "sec", UserId = user.Id };
            Context.ApiKeys.Add(row);
            Context.SaveChanges();

            // Act
            var found = _tokenBusiness.GetApiKey(apiKey);

            // Assert
            Assert.NotNull(found);
            Assert.Equal(apiKey, found!.Key);
            Assert.Equal(user.Id, found.UserId);
        }

        [Fact]
        public void GetApiKey_ReturnsNull_WhenUserMissing()
        {
            // Arrange
            UserContextStorage.Email = "nobody@example.com"; // no user with this email

            // Act
            var found = _tokenBusiness.GetApiKey("doesnotmatter");

            // Assert
            Assert.Null(found);
        }

        #endregion

        #region CreateApiKey Tests

        [Fact]
        public void CreateApiKey_PersistsRow_And_ReturnsDto_WithVerifiableHash()
        {
            // Arrange
            var email = $"creator-{Guid.NewGuid():N}@example.com";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = "Creator" };
            Context.Users.Add(user);
            Context.SaveChanges();

            // Act
            TokenResponseDto dto = _tokenBusiness.CreateApiKey();

            // Assert - DTO populated
            Assert.False(string.IsNullOrWhiteSpace(dto.apiKey));
            Assert.False(string.IsNullOrWhiteSpace(dto.apiSecret));

            // Assert - Persisted row (secret is stored as HASH)
            var saved = Context.ApiKeys.SingleOrDefault(k => k.Key == dto.apiKey && k.UserId == user.Id);
            Assert.NotNull(saved);
            Assert.False(string.IsNullOrWhiteSpace(saved!.Secret));
            
            Assert.True(_tokenBusiness.VerifyApiKey(dto.apiSecret, saved.Secret));
    
            // Verify they are NOT the same (one is plaintext, one is hash)
            Assert.NotEqual(dto.apiSecret, saved.Secret);
        }

        [Fact]
        public void CreateApiKey_Throws_WhenUserNotFound()
        {
            // Arrange
            UserContextStorage.Email = "nouser@example.com";

            // Act & Assert
            var ex = Assert.Throws<KeyNotFoundException>(() => _tokenBusiness.CreateApiKey());
            Assert.Contains("User with email", ex.Message);
        }

        #endregion

        #region DeleteApiKey Tests

        [Fact]
        public async Task DeleteApiKey_RemovesRow_And_ReturnsTrue()
        {
            // Arrange
            var email = $"deleter2-{Guid.NewGuid():N}@example.com";
            var name = "deleter2";
            var username = "deleter2";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = name, Username = username };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            var key = "delete-me";
            Context.ApiKeys.Add(new ApiKey { Key = key, Secret = "sec", UserId = user.Id });
            await Context.SaveChangesAsync();

            // Act
            var ok = await _tokenBusiness.DeleteApiKey(user.Id, key);

            // Assert
            Assert.True(ok);
            Assert.False(await Context.ApiKeys.AnyAsync(k => k.Key == key && k.UserId == user.Id));
        }

        [Fact]
        public async Task DeleteApiKey_Throws_WhenNotFound()
        {
            // Arrange
            var email = $"deleter-{Guid.NewGuid():N}@example.com";
            var name = "deleter";
            var username = "deleter";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = name, Username = username };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _tokenBusiness.DeleteApiKey(user.Id, "nope"));

            Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Hash/Verify Tests

        [Fact]
        public void HashApiKey_And_VerifyApiKey_RoundTrip()
        {
            // Arrange
            var apiKey = "roundtrip-key";

            // Act
            var hash = _tokenBusiness.HashApiKey(apiKey);

            // Assert
            Assert.True(_tokenBusiness.VerifyApiKey(apiKey, hash));
            Assert.False(_tokenBusiness.VerifyApiKey("wrong", hash));
        }

        #endregion

        #region GetAllUserKeys Tests

        [Fact]
        public async Task GetAllUserKeys_ReturnsOnlyKeys_ForUser()
        {
            // Arrange
            var email = $"keys-{Guid.NewGuid():N}@example.com";
            var name = "keysName";
            var username = "keysUsername";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = name, Username = username };
            var other = new User
                { Email = $"other-{Guid.NewGuid():N}@example.com", Name = "otherName", Username = "otherUsername" };

            Context.Users.AddRange(user, other);
            await Context.SaveChangesAsync();

            Context.ApiKeys.AddRange(
                new ApiKey { Key = "K1", Secret = "s1", UserId = user.Id },
                new ApiKey { Key = "K2", Secret = "s2", UserId = user.Id },
                new ApiKey { Key = "Z9", Secret = "s9", UserId = other.Id }
            );
            await Context.SaveChangesAsync();

            // Act
            var keys = await ((interfaces.ITokenBusiness)_tokenBusiness).GetAllUserKeys(user.Id);

            // Assert
            Assert.Equal(2, keys.Count);
            Assert.Contains("K1", keys);
            Assert.Contains("K2", keys);
            Assert.DoesNotContain("Z9", keys);
        }

        #endregion
    }
}