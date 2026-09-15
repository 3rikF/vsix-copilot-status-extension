
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using CoPilotStatusExtension.ViewModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Helper;

//-----------------------------------------------------------------------------------------------------------------------------------------
public static class VersionHelper
{
	public static string GetExtensionVersion(string fallBack)
	{
		try
		{
			Assembly assembly = typeof(GitHubStatusBarViewModel).Assembly;
			string resourceName = assembly.GetManifestResourceNames()
				.FirstOrDefault(r => r.EndsWith("source.extension.vsixmanifest"));

			if (resourceName is null)
				return fallBack;

			using Stream stream = assembly.GetManifestResourceStream(resourceName);
			if (stream is null)
				return fallBack;

			XDocument doc	= XDocument.Load(stream);
			XNamespace ns	= "http://schemas.microsoft.com/developer/vsx-schema/2011";

			string? version	= doc.Root
				?.Element(ns + "Metadata")
				?.Element(ns + "Identity")
				?.Attribute("Version")
				?.Value;

			return string.IsNullOrEmpty(version)
				? fallBack
				: version!;
		}
		catch
		{
			return fallBack;
		}
	}
}
