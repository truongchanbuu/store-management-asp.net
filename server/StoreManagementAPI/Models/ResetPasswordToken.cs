using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;


namespace StoreManagementAPI.Models
{
    public class ResetPasswordToken
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        [BsonElement("userId")]
        public string UserId { get; set; } = "";

        [BsonElement("expiryDate")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime ExpiryDate { get; set; } = DateTime.Now;

        [BsonElement("_class")]
        public string _class { get; set; } = "";
    }
}
