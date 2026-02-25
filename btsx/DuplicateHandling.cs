namespace Btsx
{
    /// <summary>
    /// Defines options for how to deal with a duplicate object being found on a remote server when migrating data,
    /// </summary>
    public enum DuplicateHandling
    {
        /// <summary>
        /// Overwrite the destination object with the new one.
        /// </summary>
        Overwrite,
        
        /// <summary>
        /// Leave the destination object and do not copy the source one.
        /// </summary>
        Skip,

        /// <summary>
        /// Create a copy of the source object at the destination.
        /// </summary>
        CreateDuplicate,

        /// <summary>
        /// Update the destination object so that any un-assigned properties have values copied from the source object if they are assigned.
        /// </summary>
        Merge
    }
}
