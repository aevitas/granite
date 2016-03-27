namespace Granite.Core.Serialization
{
    public interface ISerializer
    {
        object Deserialize(string serialized);

        string Serialize(object input);
    }
}
