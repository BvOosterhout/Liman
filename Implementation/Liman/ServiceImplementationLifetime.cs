namespace Liman
{
    public enum ServiceImplementationLifetime
    {
        /// <summary>
        /// This kind of service implementation does not have a specific lifetime requirement.
        /// Multiple instances may be created, but also shared by multiple users.
        /// </summary>
        Any,
        /// <summary>
        /// The singleton service implementation will have only one instance at a time.
        /// Use this only if the existence of multiple instances can cause issues.
        /// </summary>
        Singleton,
        /// <summary>
        /// The Application lifetime is similar to the singleton, as there will only be one instance at a time.
        /// The difference is that it's created at the start of the application.
        /// </summary>
        Application,
        /// <summary>
        /// This kind of service implementation must have only one instance during the scope lifetime.
        /// </summary>
        Scoped,
        /// <summary>
        /// Each instance of this service imlementation is different. Its lifetime is managed by the owner.
        /// </summary>
        Transient,
    }
}
