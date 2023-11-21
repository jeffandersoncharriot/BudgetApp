using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGregsPresenter;
using static BudgetCodeTests.TestConstants;

namespace Tests;

[Collection("Sequential")]
public class TestConfig {
	static string appData => Environment.GetFolderPath(
		Environment.SpecialFolder.ApplicationData,
		Environment.SpecialFolderOption.Create);

	static readonly string ExpectedAppDataFolder = "HomeBudget";

	static string ExpectedDefaultConfig => Path.Join(appData, ExpectedAppDataFolder, "config.json");

	public static TheoryData<string> SampleConfigPaths = new() {
		{ ExpectedDefaultConfig },
		{ Path.Join(appData, ExpectedAppDataFolder, "hi.json") },
		{ Path.Join(GetSolutionDir(), "hi.json") },
		{ Path.Join(GetSolutionDir(), "hello.txt") },
	};

	// Run before every test
	public TestConfig() {
		try {
			File.Delete(ExpectedDefaultConfig);
		} catch { }

		try {
			Directory.Delete(Path.Join(appData, ExpectedAppDataFolder));
		} catch { }
	}

	[Fact]
	void DefaultPath_HasExpectedValue() {
		// Assert
		Assert.Equal(ExpectedDefaultConfig, Config.DefaultPath);
	}

	[Fact]
	void New_UsesDefaultPath() {
		// Act
		Config config = new();

		// Assert
		Assert.Equal(ExpectedDefaultConfig, config.ConfigPath);
	}

	[Theory]
	[MemberData(nameof(SampleConfigPaths))]
	void New_UsesProvidedPath(string path) {
		// Act
		Config config = new(path);

		// Assert
		Assert.Equal(path, config.ConfigPath);
	}

	[Fact]
	void New_UsesDefaultsWhenConfigFileCorrupt() {
		// Arrange
		Config config = new();
		File.Create(newDbPath).Close();
		config.LastFile = newDbPath;
		Assert.Equal(newDbPath, new Config().LastFile); // Sanity check
		File.WriteAllText(ExpectedDefaultConfig, "this is invalid json");

		// Act
		config = new Config();

		// Assert
		Assert.Null(config.LastFile);
	}

	[Fact]
	void LastFile_Set() {
		// Arrange
		Config config = new();
		File.Create(newDbPath).Close();

		// Act
		config.LastFile = newDbPath;

		// Assert
		Assert.Equal(newDbPath, config.LastFile);
		// Ensure it wrote to the config file:
		Assert.Contains(
			$"\"LastFile\": \"{newDbPath.Replace("\\", "\\\\")}\"",
			File.ReadAllText(ExpectedDefaultConfig));
	}

	[Fact]
	void LastFile_IsNullWhenFileMissing() {
		// Arrange
		Config config = new();
		File.Create(newDbPath).Close();
		config.LastFile = newDbPath;
		Assert.Equal(newDbPath, config.LastFile); // Sanity check

		// Act
		File.Delete(newDbPath);

		// Assert
		Assert.Null(config.LastFile);
	}

	[Theory]
	[MemberData(nameof(SampleConfigPaths))]
	void ForceWrite_WritesToCorrectFile(string path) {
		// Arrange
		Config config = new(path);

		try {
			File.Delete(path);
		} catch { }
		Assert.False(File.Exists(path)); // Sanity check

		// Act
		config.ForceWrite();

		// Assert
		File.Exists(path);
	}

	[Fact]
	void ForceWrite_WritesAllData() {
		// Arrange
		Config config = new();

		// Act
		config.ForceWrite();

		// Assert
		var expectedFields = typeof(Config)
			.GetNestedType("SerializableConfig", System.Reflection.BindingFlags.NonPublic)!
			.GetFields();
		Assert.True(expectedFields.Length > 0); // Sanity check

		foreach (var field in expectedFields) {
			Assert.Contains(
				field.Name,
				File.ReadAllText(ExpectedDefaultConfig));
		}
	}
}
