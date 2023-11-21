using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TheGregsPresenter;
public sealed class Config {
	class SerializableConfig {
		public string? LastFile;
	}


	private static readonly JsonSerializerOptions jsonOptions = new() {
		AllowTrailingCommas = true,
		IncludeFields = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		WriteIndented = true };


	private SerializableConfig source;


	/// <summary>
	/// The default config file path.
	/// </summary>
	public static string DefaultPath {
		get {
			// Gets (roaming) appdata folder (created if it doesn't already exist)
			string appData = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create);

			return Path.Join(appData, "HomeBudget", "config.json");
		}
	}


	/// <summary>
	/// The path of the file this config uses.
	/// </summary>
	public string ConfigPath { get; private set; }

	/// <summary>
	/// The most recently opened file.
	/// </summary>
	public string? LastFile {
		get {
			string? value = source.LastFile;

			if (value is null
					// If file no longer exists, ignore it:
					|| !File.Exists(value)) {

				return null;
			}

			return value;
		}
		set {
			source.LastFile = value;
			ForceWrite();
		}
	}


	/// <summary>
	/// Gets a config which uses the default path.
	/// </summary>
	public Config() : this(DefaultPath) {}

	/// <summary>
	/// Gets a config which uses a specified path.
	/// </summary>
	/// <param name="path">The path of the config file to use.</param>
	public Config(string path) {
		ConfigPath = path;

		try {
			source = JsonSerializer.Deserialize<SerializableConfig>(
				File.ReadAllBytes(path),
				jsonOptions)!;
		} catch {
			// Something went wrong when reading the config,
			// use a default one:
			source = new();
		}
	}

	/// <summary>
	/// Forces this config to be written to its file.
	/// </summary>
	/// <exception cref="DirectoryNotFoundException">
	/// Thrown when <see cref="ConfigPath"/> is the root folder or one of its
	/// components is not a directory.
	/// </exception>
	public void ForceWrite() {
		string directory = Path.GetDirectoryName(ConfigPath)
			?? throw new DirectoryNotFoundException("Tried to use root folder as config file.");

		// Ensure all parent directories exist:
		_ = Directory.CreateDirectory(directory);

		// Write to file:
		File.WriteAllBytes(
			ConfigPath,
			JsonSerializer.SerializeToUtf8Bytes(source, jsonOptions));
	}
}
