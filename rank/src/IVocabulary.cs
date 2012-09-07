using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AuthecoLib {
    /// <summary>
    /// Represents indexed vocabulary, where every term has one index and a frequency.
    /// [term id frequency] == [string int[0] int[1]] 
    /// </summary>
    public class IVocabulary : Dictionary<string, int[]>{
        public const int ID = 0;
        public const int FREQ = 1; 

        public IVocabulary() { }

        public IVocabulary(int capacity) : base(capacity) { }       

        /// <summary>
        /// Loads a vocabulary from "id;term;freq" CSV file or "term;freq" if idFormat is false.
        /// </summary>        
        public IVocabulary (string vocFile, bool idFormat) {
			if (!File.Exists (vocFile)) {
				Console.WriteLine ("Vocabulary file '{0}' not found", vocFile);	
				return;
			}
			
			StreamReader sr = new StreamReader (vocFile);
			string line;
			string[] fields;
			string word;
			int id = 1;
			int errorNum = 0;
			int duplicateNum = 0;

			while ((line = sr.ReadLine()) != null) {
				try {
					fields = line.Split (';');
					if (idFormat && fields.Length >= 3) {
						// Loading entry in "id;term;freq" format
						if (!this.ContainsKey (fields [1])) {
							this.Add (
                                fields [1].Trim (),
                                new int[] { int.Parse (fields [0]), int.Parse (fields [2]) }
							);
						} else {
							//Console.WriteLine("Duplicate term: {0}", line);
						}
					} else if (fields.Length >= 2) {
						// Loading entry in "term;freq" format
						word = fields [0].Trim ();
						if (!this.ContainsKey (word)) {
							this.Add (
                                word,
                                new int[] { id++, int.Parse (fields [1]) }
							);
						} else {
							//Console.WriteLine ("Duplicate terms: '{0}' and '{1}'", word, fields [0]);
							duplicateNum++;
							this [word] [1] += int.Parse (fields [1]);
						}
					}
				} catch (Exception exc) {
					Console.WriteLine ("Error when loading line '{0}': '{1}'.", 
					                   line, exc.Message);
					errorNum++;
				}
			}
			this.Remove ("");
			this.Remove (" ");
			sr.Close ();
			
			Console.WriteLine ("{0} words were loaded, {1} errors, {2} duplicates",
			                   id, errorNum, duplicateNum);
        }

        /// <summary>
        /// This method returns term id even if it was not in the dictionary
        /// </summary>
        public int getid(string term) {
            // Add term if it is not exist
            if (!this.ContainsKey(term)) {
                this.Add(term, new int[]{this.Count + 1, 0});            
            }
            this[term][FREQ]++; 
                        
            return this[term][ID]; 
        }

        /// <summary>
        /// Saves the data in a CSV file "id;term;freq" if saveId = true, else "term;freq".
        /// </summary>
        public void save(string outputFile, bool saveId){
            List<KeyValuePair<string, int[]>> list = this.ToList();
            if (saveId) {
                // Sort by ascending ID
                list.Sort((x, y) => x.Value[ID].CompareTo(y.Value[ID]));
            }
            else {
                // Rank by descending frequencies
                list.Sort((x, y) => x.Value[FREQ].CompareTo(y.Value[FREQ]));
                list.Reverse();
            }

            using (StreamWriter stm = new StreamWriter(outputFile)) {
                foreach (var pair in list) {
                    if(saveId) stm.WriteLine("{0};{1};{2}", pair.Value[ID], pair.Key, pair.Value[FREQ]);
                    else stm.WriteLine("{0};{1}", pair.Key, pair.Value[FREQ]);
                }
            }
            return;            
        } 
		
		/// <summary>
		/// Get total number of tokens in a corpus.
		/// </summary>
		public double getTokensNum () {			
			double tokensNum = 0;
			foreach (var word in this) {
				tokensNum += word.Value [FREQ];				
			}
			return tokensNum;
		}
		
		public double getMeanFreq () {
			return getTokensNum() / ((double)this.Count);
		}
		
		/// <summary>
		/// Returns a dictionary <word,P(word)>:
		/// P(w_i) = f_i / sum_i(f_i), where f_i -- is the frequency of the word w_i.
		/// </summary>
		public Dictionary<string,double> getProb () {
			double tokensNum = getTokensNum ();

			// Create a dictionary with probabilities
			Dictionary<string,double> probVoc = new Dictionary<string, double> ();
			foreach (var word in this) {
				probVoc.Add (word.Key, word.Value [FREQ] / tokensNum);
			}

			#region DEBUG
			/*
			// Check that the distribution sums to one
			double probSum = 0;
			int typesNum = 0;
			foreach (var word in probVoc) {
				probSum += word.Value;
				typesNum++;
				//Console.WriteLine ("P({0})={1}", word.Key, word.Value);
			}
			Console.WriteLine ("sum_i(P(w_i) = {0}, Types={1}, Tokens={2}",
			                   probSum, typesNum, tokensNum);
			*/
			#endregion						
			
			return probVoc;
		}
        
    }
}
