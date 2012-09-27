
namespace Artech.ILInvoker
{
    /// <summary>
    /// The target type specific <see cref="PropertyAccessor"/>.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    public class PropertyAccessor<T> : PropertyAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessor&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public PropertyAccessor( string propertyName):base(typeof(T),propertyName)
        {           
        }

        /// <summary>
        /// Gets the property value of the given object.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The property value of the given object.</returns>
        public static object Get(T obj, string propertyName)
        {
            Guard.ArgumentNotNullOrEmpty(obj, "obj");
            Guard.ArgumentNotNullOrEmpty(propertyName, "propertyName");
            return PropertyAccessor.Get(obj, propertyName);
        }

        /// <summary>
        /// Sets the property value of the given object.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The property value.</param>
        public static void Set(T obj, string propertyName, object value)
        {
            Guard.ArgumentNotNullOrEmpty(obj, "obj");
            Guard.ArgumentNotNullOrEmpty(propertyName, "propertyName");
            PropertyAccessor.Set(obj, propertyName, value);
           
        }
    }
}
