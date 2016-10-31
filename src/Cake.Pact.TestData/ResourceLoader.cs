namespace Cake.Pact.TestData
{
    using System.IO;
    using System.Reflection;
    using System.Text;

    public class ResourceLoader
    {
        private const string Prefix = "Cake.Pact.TestData.";

        public static string Load(string resourceName)
        {
            var fullyQualifiedResourceName = Prefix + resourceName;

            var assembly = typeof(ResourceLoader).GetTypeInfo().Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            var resourceStream = assembly.GetManifestResourceStream(fullyQualifiedResourceName);

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
