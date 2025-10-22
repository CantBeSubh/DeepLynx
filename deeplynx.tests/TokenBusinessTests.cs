// TokenBusinessTests.cs
using System.IdentityModel.Tokens.Jwt;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Context;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class TokenBusinessTests : IntegrationTestBase
    {
        private TokenBusiness _tokenBusiness;

        public TokenBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _tokenBusiness = new TokenBusiness(Context);
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
            var dbSecret = "short-secret"; // < 32 chars triggers pad-right branch
            Context.ApiKeys.Add(new ApiKey { Key = apiKey, Secret = dbSecret, UserId = user.Id });
            Context.SaveChanges();

            // CreateToken verifies: VerifyApiKey(apiKey, secretKeyParameter)
            // So we pass a *hash of the apiKey* as the secretKey argument.
            var secretKeyParam = _tokenBusiness.HashApiKey(apiKey);

            // Act
            var jwt = _tokenBusiness.CreateToken(secretKeyParam, apiKey, expiration: 5);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(jwt));
            var handler = new JwtSecurityTokenHandler();
            var parsed = handler.ReadJwtToken(jwt);
            Assert.Equal(apiKey, parsed.Claims.First(c => c.Type == "apiKey").Value);
            Assert.True(parsed.ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public void CreateToken_ReturnsEmpty_WhenVerifyFails()
        {
            // Arrange
            var email = $"tester-{Guid.NewGuid():N}@example.com";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = "Tester" };
            Context.Users.Add(user);
            Context.SaveChanges();

            var apiKey = "api-key-XYZ";
            Context.ApiKeys.Add(new ApiKey { Key = apiKey, Secret = "any-secret", UserId = user.Id });
            Context.SaveChanges();

            // Wrong hash -> VerifyApiKey(apiKey, wrongHash) == false
            var wrongHash = _tokenBusiness.HashApiKey("not-the-api-key");

            // Act
            var jwt = _tokenBusiness.CreateToken(wrongHash, apiKey, expiration: 5);

            // Assert
            Assert.Equal(string.Empty, jwt);
        }

        [Fact]
        public void CreateToken_Throws_WhenDbSecretMissing()
        {
            // Arrange
            var email = $"tester-{Guid.NewGuid():N}@example.com";
            UserContextStorage.Email = email;

            var user = new User { Email = email, Name = "Tester" };
            Context.Users.Add(user);
            Context.SaveChanges();

            var apiKey = "will-have-no-secret";
            // Add an ApiKey *without* a Secret to force the not found branch
            Context.ApiKeys.Add(new ApiKey { Key = apiKey, Secret = "", UserId = user.Id });
            Context.SaveChanges();

            var goodHash = _tokenBusiness.HashApiKey(apiKey);

            // Act & Assert
            var ex = Assert.Throws<KeyNotFoundException>(() =>
                _tokenBusiness.CreateToken(goodHash, apiKey, expiration: 5));
            Assert.Contains("Api key not found", ex.Message);
        }

        #endregion

        #region GetApiKey Tests

        [Fact]
        public void GetApiKey_ReturnsKey_WhenUserExists()
        {
            // Arrange
            var email = $"tester-{Guid.NewGuid():N}@example.com";
            UserContextStorage.Email = email;

            var user = new User { Email = email };
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
            Assert.True(_tokenBusiness.VerifyApiKey(dto.apiKey, dto.apiSecret));

            // Assert - Persisted row (secret is stored as plaintext per current design)
            var saved = Context.ApiKeys.SingleOrDefault(k => k.Key == dto.apiKey && k.UserId == user.Id);
            Assert.NotNull(saved);
            Assert.False(string.IsNullOrWhiteSpace(saved!.Secret));
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
            var email = $"deleter-{Guid.NewGuid():N}@example.com";
            UserContextStorage.Email = email;

            var user = new User { Email = email };
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
            UserContextStorage.Email = email;

            var user = new User { Email = email };
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
            UserContextStorage.Email = email;

            var user = new User { Email = email };
            var other = new User { Email = $"other-{Guid.NewGuid():N}@example.com" };

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