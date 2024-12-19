namespace Readymade.Utils.Patterns {
    /// <summary>
    /// Represents a type that can have its dependencies injected explicitly.
    /// </summary>
    public interface IConfigurable {
        /// <summary>
        /// Whether the instance has been injected with its dependencies.
        /// </summary>
        public bool IsConfigured { get; }
    }

    /// <inheritdoc />
    public interface IConfigurable<T1> : IConfigurable {
        /// <summary>
        /// Inject dependencies.
        /// </summary>
        public void Configure ( T1 placeableSystem );
    }

    /// <inheritdoc />
    public interface IConfigurable<T1, T2> : IConfigurable {
        /// <inheritdoc cref="IConfigurable{T1}.Configure(T1)"/>
        public void Configure ( T1 arg1, T2 arg2 );
    }

    /// <inheritdoc />
    public interface IConfigurable<T1, T2, T3> : IConfigurable {
        /// <inheritdoc cref="IConfigurable{T1}.Configure(T1)"/>
        public void Configure ( T1 arg1, T2 arg2, T3 arg3 );
    }

    /// <inheritdoc />
    public interface IConfigurable<T1, T2, T3, T4> : IConfigurable {
        /// <inheritdoc cref="IConfigurable{T1}.Configure(T1)"/>
        public void Configure ( T1 arg1, T2 arg2, T3 arg3, T4 arg4 );
    }

    /// <inheritdoc />
    public interface IConfigurable<T1, T2, T3, T4, T5> : IConfigurable {
        /// <inheritdoc cref="IConfigurable{T1}.Configure(T1)"/>
        public void Configure ( T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5 );
    }

    /// <inheritdoc />
    public interface IConfigurable<T1, T2, T3, T4, T5, T6> : IConfigurable {
        /// <inheritdoc cref="IConfigurable{T1}.Configure(T1)"/>
        public void Configure ( T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6 );
    }
}