﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpMinifier.Tests
{
	[TestFixture]
	public class MinifierTests
	{
		Dictionary<string, string> Samples;

		[SetUp]
		public void Init()
		{
			Samples = new Dictionary<string, string>();
			var sampleFiles = Directory.GetFiles(@"..\..\Samples");
			foreach (var file in sampleFiles)
			{
				var code = File.ReadAllText(file);
				Samples.Add(Path.GetFileNameWithoutExtension(file), code);
				if (!CompileUtils.CanCompile(code))
					Assert.Inconclusive("All input code should be compilied");
			}
		}

		[Test]
		public void RemoveSpaces()
		{
			var minifierOptions = new MinifierOptions(false)
			{
				SpacesRemoving = true
			};
			var minifier = new Minifier(minifierOptions);
			foreach (var sample in Samples)
			{
				var minified = minifier.MinifyFromString(sample.Value);
				Assert.IsTrue(CompileUtils.CanCompile(minified));
				if (sample.Key == "Test1")
					Assert.IsFalse(minified.Contains(" /*"));
			}
		}

		[Test]
		public void LineLengthConstraint()
		{
			var minifierOptions = new MinifierOptions
			{
				SpacesRemoving = true,
				CommentsRemoving = true,
				LineLength = 80,
				RegionsRemoving = true
			};
			var minifier = new Minifier(minifierOptions);
			foreach (var sample in Samples)
			{
				var minified = minifier.MinifyFromString(sample.Value);
				Assert.IsTrue(CompileUtils.CanCompile(minified));
			}
		}

		[Test]
		public void RemoveComments()
		{
			var minifierOptions = new MinifierOptions(false)
			{
				SpacesRemoving = true,
				CommentsRemoving = true
			};
			var minifier = new Minifier(minifierOptions);

			var test = Samples["Test1"];
			if (!test.Contains("//") || !test.Contains("/*") || !test.Contains("*/"))
				Assert.Inconclusive("Invalid test sample for RemoveComments test");
			var minified = minifier.MinifyFromString(test);
			Assert.IsTrue(CompileUtils.CanCompile(minified));
			Assert.IsFalse(minified.Contains("//"));
			Assert.IsFalse(minified.Contains("/*"));
			Assert.IsFalse(minified.Contains("*/"));
		}
		
		[Test]
		public void RemoveRegions()
		{
			var minifierOptions = new MinifierOptions
			{
				SpacesRemoving = true,
				RegionsRemoving = true
			};
			var minifier = new Minifier(minifierOptions);

			var test = Samples["Test1"];
			if (!test.Contains("#region") || !test.Contains("#endregion"))
				Assert.Inconclusive("Invalid test sample for RemoveRegions test");
			var minified = minifier.MinifyFromString(test);
			Assert.IsTrue(CompileUtils.CanCompile(minified));
			Assert.IsFalse(minified.Contains("#region"));
			Assert.IsFalse(minified.Contains("#endregion"));
		}

		[Test]
		public void CompressIdentifiers()
		{
			var minifierOptions = new MinifierOptions(false)
			{
				LocalVarsCompressing = true,
				MembersCompressing = true,
				TypesCompressing = true
			};
			var minifier = new Minifier(minifierOptions);
			foreach (var sample in Samples)
			{
				var minified = minifier.MinifyFromString(sample.Value);
				Assert.IsTrue(CompileUtils.CanCompile(minified));
			}
		}

		[Test]
		public void CompressMisc()
		{
			var minifierOptions = new MinifierOptions(false)
			{
				MiscCompressing = true
			};
			var minifier = new Minifier(minifierOptions);
			var minified = minifier.MinifyFromString(Samples["MiscCompression"]);
			Assert.IsTrue(minified.Contains("255"));
			Assert.IsTrue(minified.Contains("0x7048860F9180"));
			Assert.IsFalse(minified.Contains("private"));
			Assert.AreEqual(2, minified.Count(c => c == '{'));
			Assert.AreEqual(2, minified.Count(c => c == '}'));
		}

		[Test]
		public void IgnoredIdAndComments()
		{
			var minifier = new Minifier(null, new string[] { "unminifiedId" }, new string[] { "unremovableComment", "/*unremovableComment1*/" });
			var test = Samples["Test1"];
			if (!test.Contains("unminifiedId") || !test.Contains("unremovableComment") || !test.Contains("/*unremovableComment1*/"))
				Assert.Inconclusive("Invalid test sample for IgnoredIdAndComments test");
			var minified = minifier.MinifyFromString(test);
			Assert.IsTrue(minified.Contains("unminifiedId"));
			Assert.IsTrue(minified.Contains("unremovableComment"));
			Assert.IsTrue(minified.Contains("/*unremovableComment1*/"));
		}
	}
}
