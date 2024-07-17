using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Runtime.CompilerServices;


namespace StoreManagementAPI.Models
{
    public class User
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";


        [BsonElement("email")]
        public string Email { get;set;} = "";

        [BsonElement("username")]
        public string Username { get;set;} = "";

        [BsonElement("password")]
        public string Password { get;set;} = "";

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public Status Status { get; set; } = Status.NORMAL;

        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)]
        public Role Role { get; set; } = Role.EMPLOYEE;

        [BsonElement("avatar")]
        public string Avatar { get; set; } = "";

        [BsonElement("_class")]
        public string _class { get; set; } = "";

        public override string ToString()
        {
            return $"[User] Id: {Id}, Email: {Email}, Username: {Username}, Password: {Password}, Status: {Status}, Role: {Role}, Avatar: {Avatar}, _class: {_class}";
        }


    }
}
