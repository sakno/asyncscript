using System;

namespace DynamicScript.Runtime.Serialization
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Stream = System.IO.Stream;
    using IFormatter = System.Runtime.Serialization.IFormatter;
    using ScriptObject = Environment.ScriptObject;

    /// <summary>
    /// Represents DynamicScript object serializer.
    /// </summary>
    
    [ComVisible(false)]
    public static class DynamicScriptSerializer
    {
        /// <summary>
        /// Serializes DynamicScript object using the specified formatter.
        /// </summary>
        /// <param name="serializer">Serialization formatter. Cannot be <see langword="null"/>.</param>
        /// <param name="output">Serialization stream. Cannot be <see langword="null"/>.</param>
        /// <param name="obj">An object to serialize.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="serializer"/> or <paramref name="output"/> is <see langword="null"/>.</exception>
        public static void Serialize(IFormatter serializer, Stream output, IScriptObject obj)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (output == null) throw new ArgumentNullException("output");
            if (obj == null) obj = ScriptObject.Void;
            serializer.Serialize(output, obj);
        }

        /// <summary>
        /// Serializes DynamicScript object using the specified formatter.
        /// </summary>
        /// <typeparam name="TFormatter">Type of the serialization formatter.</typeparam>
        /// <param name="output">Serialization stream. Cannot be <see langword="null"/>.</param>
        /// <param name="obj">An object to serialize.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="output"/> is <see langword="null"/>.</exception>
        public static void Serialize<TFormatter>(Stream output, IScriptObject obj)
            where TFormatter : IFormatter, new()
        {
            Serialize(new TFormatter(), output, obj);
        }

        /// <summary>
        /// Deserializes DynamicScript object using the specified formatter.
        /// </summary>
        /// <param name="deserializer">Deserialization formatter. Cannot be <see langword="null"/>.</param>
        /// <param name="input">A stream that contains serialized object. Cannot be <see langword="null"/>.</param>
        /// <returns>Deserialized object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="deserializer"/> or <paramref name="input"/> is <see langword="null"/>.</exception>
        public static IScriptObject Deserialize(IFormatter deserializer, Stream input)
        {
            if (deserializer == null) throw new ArgumentNullException("deserializer");
            if (input == null) throw new ArgumentNullException("input");
            return deserializer.Deserialize(input) as IScriptObject ?? ScriptObject.Void;
        }

        /// <summary>
        /// Deserializes DynamicScript object using the specified formatter.
        /// </summary>
        /// <typeparam name="TFormatter">Type of the deserialization formatter.</typeparam>
        /// <param name="input">A stream that contains serialized object. Cannot be <see langword="null"/>.</param>
        /// <returns>Deserialized object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
        public static IScriptObject Deserialize<TFormatter>(Stream input)
            where TFormatter : IFormatter, new()
        {
            return Deserialize(new TFormatter(), input);
        }
    }
}
