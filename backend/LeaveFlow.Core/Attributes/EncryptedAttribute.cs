namespace LeaveFlow.Core.Attributes;

/// <summary>
/// Marks a property as encrypted. The value will be encrypted before storing in DB
/// and decrypted when retrieved.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EncryptedAttribute : Attribute
{
}
