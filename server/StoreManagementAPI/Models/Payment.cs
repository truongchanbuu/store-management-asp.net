using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace StoreManagementAPI.Models
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("paymentId")]
        public string? PaymentId { get; set; }
        [BsonElement("oid")]
        public string? Oid { get; set; }
        [BsonElement("uid")]
        public string? Uid { get; set; }
        [BsonElement("paymentMethod")]
        public string? PaymentMethod { get; set; }
        [BsonElement("amount")]
        public double Amount { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("paymentTime")]
        public DateTime PaymentTime { get; set; }
        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public Status Status { get; set; }
        [BsonElement("_class")]
        public string _class { get; set; } = "";
    }
}
