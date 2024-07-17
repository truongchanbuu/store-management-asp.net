using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace StoreManagementAPI.Models
{
    public class OrderProduct
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        [BsonElement("pid")]
        [BsonRepresentation(BsonType.String)]
        public string Pid { get; set; } = "";

        [BsonElement("oid")]
        [BsonRepresentation(BsonType.String)]
        public string Oid { get; set; } = "";

        [BsonElement("quantity")]
        [BsonRepresentation(BsonType.Int32)]
        public int Quantity { get; set; } = 0;


        [BsonElement("importPrice")]
        [BsonRepresentation(BsonType.Double)]
        public double ImportPrice { get; set; } = 0;

        [BsonElement("retailPrice")]
        [BsonRepresentation(BsonType.Double)]
        public double RetailPrice { get; set; } = 0;

        [BsonElement("_class")]
        public string _class { get; set; } = "";

        public override string ToString()
        {
            return $"[OrderProduct] Id: {Id}, Pid: {Pid}, Oid: {Oid}, Quantity: {Quantity}, ImportPrice: {ImportPrice}, RetailPrice: {RetailPrice}, _class: {_class}";
        }



    }
}
