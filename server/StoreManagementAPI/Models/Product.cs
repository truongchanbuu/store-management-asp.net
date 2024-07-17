using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace StoreManagementAPI.Models
{
    public class Product
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Pid { get; set; } = "";

        [BsonElement("name")]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; } = "";

        [BsonElement("category")]
        [BsonRepresentation(BsonType.String)]
        public Category Category { get; set; } = Category.PHONE;

        [BsonElement("importPrice")]
        [BsonRepresentation(BsonType.Double)]
        public double ImportPrice { get; set; } = 0;

        [BsonElement("retailPrice")]
        [BsonRepresentation(BsonType.Double)]
        public double RetailPrice { get; set; } = 0;

        [BsonElement("barcode")]
        [BsonRepresentation(BsonType.String)]
        public string Barcode { get; set; } = "";

        [BsonElement("illustrator")]
        [BsonRepresentation(BsonType.String)]
        public string Illustrator { get; set; } = "";


        [BsonElement("quantity")]
        [BsonRepresentation(BsonType.Int32)]
        public int Quantity { get; set; } = 0;


        [BsonElement("createdAt")]
        [BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? CreatedAt { get; set; } = DateTime.SpecifyKind(
                                                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local), DateTimeKind.Utc);

        [BsonElement("updatedAt")]
        [BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? UpdatedAt { get; set; } = null;

        [BsonElement("_class")]
        public string _class { get; set; } = "";

        override public string ToString()
        {
            return $"Product: {Name}, Category: {Category}, ImportPrice: {ImportPrice}, RetailPrice: {RetailPrice}, Barcode: {Barcode}, Illustrator: {Illustrator}, Quantity: {Quantity}, CreatedAt: {CreatedAt}, UpdatedAt: {UpdatedAt}";
        }

    }
}
