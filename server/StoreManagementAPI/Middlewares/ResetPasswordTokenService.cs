using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StoreManagementAPI.Configs;
using StoreManagementAPI.Models;

namespace StoreManagementAPI.Middlewares
{
    public class ResetPasswordTokenService
    {
        private readonly IMongoCollection<ResetPasswordToken> _tokens;

        public ResetPasswordTokenService(IOptions<StoreManagementDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);

            _tokens = database.GetCollection<ResetPasswordToken>(settings.Value.PRTCollectionName);
        }

        public async Task<string> CreateToken(string userId)
        {
            var resetPasswordToken = new ResetPasswordToken
            {
                UserId = userId,
                ExpiryDate = DateTime.Now.AddMinutes(1)
            };

            await _tokens.InsertOneAsync(resetPasswordToken);

            return resetPasswordToken.Id;
        }

        public async Task<string?> GetUserIdFromToken(string token)
        {
            ClearExpiredToken();

            var resetPasswordToken = await _tokens.Find(t => t.Id == token).FirstOrDefaultAsync();
            return resetPasswordToken?.UserId;
        }

        private void ClearExpiredToken()
        {
            var expiredTokens = _tokens.Find(t => t.ExpiryDate < DateTime.Now).ToList();
            foreach (var token in expiredTokens)
                _tokens.DeleteOne(t => t.Id == token.Id);
        }
    }
}
