﻿using Icu.Collation;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Util;
using System;
using System.IO;

namespace Lucene.Net.Collation
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

	/// <summary>
	/// <para>
	///   Configures <seealso cref="KeywordTokenizer"/> with <seealso cref="CollationAttributeFactory"/>.
	/// </para>
	/// <para>
	///   Converts the token into its <seealso cref="java.text.CollationKey"/>, and then
	///   encodes the CollationKey either directly or with 
	///   <seealso cref="IndexableBinaryStringTools"/> (see <a href="#version">below</a>), to allow 
	///   it to be stored as an index term.
	/// </para>
	/// <para>
	///   <strong>WARNING:</strong> Make sure you use exactly the same Collator at
	///   index and query time -- CollationKeys are only comparable when produced by
	///   the same Collator.  Since <seealso cref="java.text.RuleBasedCollator"/>s are not
	///   independently versioned, it is unsafe to search against stored
	///   CollationKeys unless the following are exactly the same (best practice is
	///   to store this information with the index and check that they remain the
	///   same at query time):
	/// </para>
	/// <ol>
	///   <li>JVM vendor</li>
	///   <li>JVM version, including patch version</li>
	///   <li>
	///     The language (and country and variant, if specified) of the Locale
	///     used when constructing the collator via
	///     <seealso cref="Collator#getInstance(java.util.Locale)"/>.
	///   </li>
	///   <li>
	///     The collation strength used - see <seealso cref="Collator#setStrength(int)"/>
	///   </li>
	/// </ol> 
	/// <para>
	///   The <code>ICUCollationKeyAnalyzer</code> in the analysis-icu package 
	///   uses ICU4J's Collator, which makes its
	///   its version available, thus allowing collation to be versioned
	///   independently from the JVM.  ICUCollationKeyAnalyzer is also significantly
	///   faster and generates significantly shorter keys than CollationKeyAnalyzer.
	///   See <a href="http://site.icu-project.org/charts/collation-icu4j-sun"
	///   >http://site.icu-project.org/charts/collation-icu4j-sun</a> for key
	///   generation timing and key length comparisons between ICU4J and
	///   java.text.Collator over several languages.
	/// </para>
	/// <para>
	///   CollationKeys generated by java.text.Collators are not compatible
	///   with those those generated by ICU Collators.  Specifically, if you use 
	///   CollationKeyAnalyzer to generate index terms, do not use
	///   ICUCollationKeyAnalyzer on the query side, or vice versa.
	/// </para>
	/// <a name="version"/>
	/// <para>You must specify the required <seealso cref="Version"/>
	/// compatibility when creating CollationKeyAnalyzer:
	/// <ul>
	///   <li> As of 4.0, Collation Keys are directly encoded as bytes. Previous
	///   versions will encode the bytes with <seealso cref="IndexableBinaryStringTools"/>.
	/// </ul>
	/// </para>
	/// </summary>
	public sealed class CollationKeyAnalyzer : Analyzer
	{
		private readonly Collator collator;
		private readonly CollationAttributeFactory factory;
		private readonly LuceneVersion matchVersion;

		/// <summary>
		/// Create a new CollationKeyAnalyzer, using the specified collator.
		/// </summary>
		/// <param name="matchVersion"> See <a href="#version">above</a> </param>
		/// <param name="collator"> CollationKey generator </param>
		public CollationKeyAnalyzer(LuceneVersion matchVersion, Collator collator)
		{
			this.matchVersion = matchVersion;
			this.collator = collator;
			this.factory = new CollationAttributeFactory(collator);
		}
		
		[Obsolete("Use <seealso cref=\"CollationKeyAnalyzer#CollationKeyAnalyzer(LuceneVersion, Collator)\"/> and specify a version instead. This ctor will be removed in Lucene 5.0")]
		public CollationKeyAnalyzer(Collator collator) : this(LuceneVersion.LUCENE_31, collator)
		{
		}

		public override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
		{
#pragma warning disable 612, 618
            if (this.matchVersion.OnOrAfter(LuceneVersion.LUCENE_40))
#pragma warning restore 612, 618
            {
                var tokenizer = new KeywordTokenizer(this.factory, reader, KeywordTokenizer.DEFAULT_BUFFER_SIZE);
				return new TokenStreamComponents(tokenizer, tokenizer);
			}
			else
			{
				var tokenizer = new KeywordTokenizer(reader);
				return new TokenStreamComponents(tokenizer,
#pragma warning disable 612, 618
                    new CollationKeyFilter(tokenizer, this.collator));
#pragma warning restore 612, 618
            }
        }
	}
}