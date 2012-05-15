using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using AssemblyBuilder = System.Reflection.Emit.AssemblyBuilder;

    /// <summary>
    /// Represents advanced assembly version information.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    [Serializable]
    public sealed class AssemblyVersionResource
    {
        private readonly string m_product;
        private readonly Version m_version;
        private readonly string m_company;
        private readonly string m_copyright;
        private readonly string m_trademark;

        /// <summary>
        /// Initializes a new additional information about assembly.
        /// </summary>
        /// <param name="productName">A product name. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="productVersion">A product version.</param>
        /// <param name="company">Assembly manufacturer.</param>
        /// <param name="copyright">Copyright information.</param>
        /// <param name="trademark">A product trademarks.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="productName"/> is <see langword="null"/> or empty.</exception>
        public AssemblyVersionResource(string productName, Version productVersion, string company = "", string copyright = "", string trademark = "")
        {
            if (string.IsNullOrEmpty(productName)) throw new ArgumentNullException("productName");
            if (productVersion == null) productVersion = new Version(1, 0, 0, 0);
            m_company = company ?? String.Empty;
            m_copyright = copyright ?? String.Empty;
            m_trademark = trademark ?? String.Empty;
            m_version = productVersion;
            m_product = productName;
        }

        /// <summary>
        /// Gets product name.
        /// </summary>
        public string ProductName
        {
            get { return m_product; }
        }

        /// <summary>
        /// Gets product version.
        /// </summary>
        public Version ProductVersion
        {
            get { return m_version; }
        }

        /// <summary>
        /// Gets manufacturer.
        /// </summary>
        public string Company
        {
            get { return m_company; }
        }

        /// <summary>
        /// Gets copyright information.
        /// </summary>
        public string Copyright
        {
            get { return m_copyright; }
        }

        /// <summary>
        /// Gets trademark information.
        /// </summary>
        public string Trademark
        {
            get { return m_trademark; }
        }

        private static ConstructorInfo AssemblyProductAttributeConstructor
        {
            get { return typeof(AssemblyProductAttribute).GetConstructor(new[] { typeof(string) }); }
        }

        private static ConstructorInfo AssemblyCompanyAttributeConstructor
        {
            get { return typeof(AssemblyCompanyAttribute).GetConstructor(new[] { typeof(string) }); }
        }

        private static ConstructorInfo AssemblyCopyrightAttributeConstructor
        {
            get { return typeof(AssemblyCopyrightAttribute).GetConstructor(new[] { typeof(string) }); }
        }

        private static ConstructorInfo AssemblyTrademarkAttributeConstructor
        {
            get { return typeof(AssemblyTrademarkAttribute).GetConstructor(new[] { typeof(string) }); }
        }

        internal void Emit(AssemblyBuilder assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            assembly.DefineVersionInfoResource(ProductName, ProductVersion.ToString(), Company, Copyright, Trademark);
            //Emits product name
            assembly.SetCustomAttribute(new CustomAttributeBuilder(AssemblyProductAttributeConstructor, new[] { ProductName }));
            //Emits company
            assembly.SetCustomAttribute(new CustomAttributeBuilder(AssemblyCompanyAttributeConstructor, new[] { Company }));
            //Emits copyright
            assembly.SetCustomAttribute(new CustomAttributeBuilder(AssemblyCopyrightAttributeConstructor, new[] { Copyright }));
            //Emits trademark
            assembly.SetCustomAttribute(new CustomAttributeBuilder(AssemblyTrademarkAttributeConstructor, new[] { Trademark }));
        }
    }
}
