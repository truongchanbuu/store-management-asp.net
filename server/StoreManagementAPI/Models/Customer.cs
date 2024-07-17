using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;

namespace StoreManagementAPI.Models
{
    public class Customer
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? CustId { get; set; } = "";

        [BsonElement("name")]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; } = "";

        [BsonElement("phone")]
        [BsonRepresentation(BsonType.String)]
        public string Phone { get; set; } = "";

        [BsonElement("email")]
        [BsonRepresentation(BsonType.String)]
        public string Email { get; set; } = "";

        [BsonElement("point")]
        [BsonRepresentation(BsonType.Double)]
        public double Point { get; set; } = 0;

        [BsonElement("_class")]
        public string _class { get; set; } = "";

        public override string ToString()
        {
            return $"[Customer] CustId: {CustId}, Name: {Name}, Phone: {Phone}, Email: {Email}, Point: {Point}, _class: {_class}";
        }
    }
}
