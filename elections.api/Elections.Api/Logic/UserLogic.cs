using Elections.Api.Core;
using Elections.Api.DE;
using Elections.Api.Models;
using MongoDB.Driver;

namespace Elections.Api.Logic;

public class UserLogic(IMongoDatabase database)
{
    private readonly IMongoCollection<ContactDB> _contactCollection = database.GetCollection<ContactDB>("contact");

    public async Task<ApiResponse> Contact(ContactReq req, string ip)
    {
        try
        {
            var firstName = req.FirstName.Clean();
            var lastName = req.LastName.Clean();
            var phone = req.Phone.Clean();
            var email = req.Email.Clean();
            var message = req.Message.Clean();

            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);
            var recentEntry = await _contactCollection
                .Find(x => x.Ip == ip && x.Created > thirtyMinutesAgo)
                .FirstOrDefaultAsync();

            if (recentEntry != null)
            {
                return ApiResponse.FromError("ERROR_CONTACT_RATE_LIMIT");
            }

            var contactDB = new ContactDB
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Email = email,
                Message = message,
                Ip = ip,
                Created = DateTime.UtcNow
            };

            await _contactCollection.InsertOneAsync(contactDB);
            return ApiResponse.FromSuccess();
        }
        catch (Exception ex)
        {
            return ApiResponse.FromError(ex.Message);
        }
    }
}
