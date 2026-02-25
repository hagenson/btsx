namespace Btsx
{
    /// <summary>
    /// Properties marked with this attribute will be encrypted when a job is persisted and
    /// cleared  once a job has completed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ProtectedAttribute : Attribute
    {
    }
}