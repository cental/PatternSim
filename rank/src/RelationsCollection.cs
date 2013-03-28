using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace AuthecoLib {
    public class PairInfo {
        public PairInfo(double sim, int patternsNum) {
			this.sim = sim;			
			this.patternsNum = patternsNum;
        }        
        public double sim; // semantic similarity between pair of terms        		
		public int patternsNum; // number of patterns which were used to extract the relation
    }

    /// <summary>
    /// Represents a collection of binary semantic relations between words "word1;word2;type"
    /// </summary>
    public class RelationsCollection : Dictionary<string, Dictionary<string, PairInfo>> {
		const int TARGET_FIELD = 0;
        const int RELATUM_FIELD = 1;
        const int SUM_FIELD = 7;
		const int PATTERN_FIRST_FIELD = 8;

		public int _relationsCount;

		/// <summary>
        /// This field contains the vocabulary as it was on the moment of its loading. 
        /// The field exist for optimization purposes.
        /// </summary>
        private List<string> _shuffledVocabulary = new List<string>();

		public RelationsCollection() {
            isTransformed = false;
        }
		        
        public RelationsCollection (string relationsFile) : 
			this(relationsFile, false) {  }

		/// <summary>
        /// Loads relations from a CSV file into memory. The relationsFile is in
        /// "target;relatum;syno;cohypo;hyper_hypo;hyper;hypo;sum;pattern1;pattern2;
		/// pattern3;pattern4;pattern5;pattern6;pattern7;pattern8;pattern9;pattern10;
		/// pattern11;pattern12;pattern13;pattern14;pattern15;pattern16;pattern17"
		/// If addSymmetric then symmetrical relations are also added. 
        /// </summary>
        public RelationsCollection (string relationsFile, bool addSymetric) {
			isTransformed = false;
			_relationsCount = 0;
			if (File.Exists (relationsFile)) {
				load (relationsFile, addSymetric);
			} 
			else {
				Console.WriteLine ("Error: relations file '{0}' does not exist.", relationsFile);
			}            
        }

        public void add(string target, string relatum, double sim, int patternsNum){
            if (!this.ContainsKey(target)) {
                this.Add(target, new Dictionary<string, PairInfo>());
			}

			if (this[target].ContainsKey(relatum)) {
				//Console.WriteLine("<{0},{1}> is already in the dictionary", target, relatum);
			}
			else{ 
				this[target].Add(relatum, new PairInfo(sim, patternsNum));
            	_relationsCount++;         
			}
        }

        /// <summary>
        /// Returns the unique vocabulary sorted alphabetically of the set of relations
        /// </summary>
        public List<string> vocabulary() {
            // Add all targets 
            HashSet<string> voc = new HashSet<string>(this.Keys);

            // Add all relatums to the vocabulary
            foreach (var target in this) {
                // Write all SN of the target
                foreach (var r in target.Value) {
                    if (!voc.Contains(r.Key)) voc.Add(r.Key);
                }
            }

            // Gets distinct words sorted alphabetically
            List<string> vocList = voc.ToList();
            vocList.Sort();
            vocList.Reverse();

            return vocList;
        }
				
		/// "target;relatum;syno;cohypo;hyper_hypo;hyper;hypo;sum;pattern1;pattern2;
		/// pattern3;pattern4;pattern5;pattern6;pattern7;pattern8;pattern9;pattern10;
		/// pattern11;pattern12;pattern13;pattern14;pattern15;pattern16;pattern17"
		public void load(string input, bool addSymmetric) {
			string target;
			string relatum;
			double sim;
			int patterns;
			int patternN;

			using (CsvReader csv = new CsvReader(new StreamReader(input, Encoding.UTF8), true, ';')) {
        		int fieldCount = csv.FieldCount;
		        //string[] headers = csv.GetFieldHeaders();
				// Read file with relations line by line
				while (csv.ReadNextRecord()) {
					// Try to add an entry
					try{
                    	// Read the fields                        
                    	target = csv[TARGET_FIELD].Trim().ToLower();
                    	relatum = csv[RELATUM_FIELD].Trim().ToLower();
                    	sim = (double.TryParse(csv[SUM_FIELD], out sim) ? sim : 0);

						patterns = 0;
						for(int i = PATTERN_FIRST_FIELD; i < fieldCount; i++){
							patternN = (int.TryParse(csv[i], out patternN) ? patternN : 0);
							if (patternN > 0) patterns++;
						}
						// Add the word pair to the dictionary
						this.add(target, relatum, sim, patterns); // We assume that the input file contains symmetric relations                     
						//Console.WriteLine("{0}\t{1}\t{2}\t{3}", target, relatum, sim, patterns);
						if(addSymmetric) this.add(relatum, target, sim, patterns);                    
                	}
					catch(Exception exc){
						Console.WriteLine("{0}\t{1}", csv[0] + ";" + csv[1] + ";" + csv[2], exc.Message);
					}
            	}
			}
		}
				
		public void save(string outputFile) { 
			save(outputFile, false, false);
		}
		
        public void save(string outputFile, bool noSim) { 
			save(outputFile, noSim, false);
		}

        /// <summary>
        /// Saves the structure in CSV file "word1;word2;type;sim"
        /// If noSim is true then format is "word1;word2;type"
        /// </summary>
        public void save(string outputFile, bool noSim, bool sortBySim) {
			// Open the output file
			StreamWriter outputStm = new StreamWriter(outputFile, false, Encoding.UTF8);

			// Write all the relations 
			Dictionary<string, PairInfo> relatums;
			foreach(var target in this) {
				if(sortBySim) {
					var sortedDict = (from entry in target.Value orderby entry.Value.sim descending select entry).ToDictionary(
						pair => pair.Key,
						pair => pair.Value
					);					
					relatums = sortedDict;
				}
				else{
					relatums = target.Value;
				}
				
                // Write all relatums of the target
                foreach (var relatum in relatums) {
                   	if (noSim) {
                       	outputStm.WriteLine("{0};{1}",
							target.Key, relatum.Key);
                   	}
                   	else {
                       	if(relatum.Value.sim > 1){
							outputStm.WriteLine("{0};{1};{2:G10}",
						    	target.Key, relatum.Key, relatum.Value.sim);
						}
						else{
							outputStm.WriteLine("{0};{1};{2:F10}",
						    	target.Key, relatum.Key, relatum.Value.sim);
						}
                   	}
                }
			}

            // Close the output file
            outputStm.Close();
        }

        public void print() {
            foreach (var target in this) {
                foreach (var relatum in target.Value) {
                    if(relatum.Value.sim >= 1){
						Console.WriteLine("{0};{1};{2:G10};{3}",
						    	target.Key, relatum.Key, relatum.Value.sim, relatum.Value.patternsNum);
					}
					else{
						Console.WriteLine("{0};{1};{2:F10};{3}",
						    	target.Key, relatum.Key, relatum.Value.sim, relatum.Value.patternsNum);
					}
                }
            }
        }

        /// <summary>
        /// An interactive command where user inputs a word (target) and a random (unrelated) word is returned.
        /// The constraints of the vocabulary are met.
        /// </summary>
        public void randomTermInteractive() {
            //foreach (var s in _shuffledVocabulary) {
            //    Console.WriteLine("{0}", s);
            //}
			
			
            while (true) {
                // Read a term
                Console.Write("\nEnter term:");
                string term = Console.ReadLine();
                if (!_shuffledVocabulary.Contains(term)) {
                    Console.WriteLine("Warning: term {0} is not in the vocabulary.", term);
                }

                // Generate random word unrelated with the term
                List<string> randomlist = getRandomWords(term);
                for (int i = 0; i < Math.Min(randomlist.Count, 20); i++) {
                    Console.WriteLine(randomlist[i]);
                }
            }
            //RelationsCollection originalRelations = new RelationsCollection(relationsFile, false);
            //List<string> randomWords = originalRelations.getRandomWords(target.Key);
            
        }

        /// <summary>
        /// Loads similarity scores to the set of currently loded relations from an external file. 
        /// simFile is a CSV file in the format 'target;relatum;score'. If bless format then 
        /// input file is "target;relatum;type;score", where type is usually "?".
        /// </summary>
        public void loadSimilarity(string simFile, bool blessFormat) {
            // Initialization
            StreamReader sr = new StreamReader(simFile);
            string[] fields;
            string line, target, relatum;
            int targetField = 0;
            int relatumField = 1 ;            
            int simField = (blessFormat ? 3 : 2);
            int minLenght = (blessFormat ? 4 : 3);
            int inconsistPairsNum = 0;
            bool inconsistency;
            int usedPairsNum = 0;
            
            // Read file with relations line by line
            while ((line = sr.ReadLine()) != null) {
                // Read the fields from the current line                
                fields = line.Split(new char[] { ';' });

                // Add entry if the line is well-formed
                if (fields.Length >= minLenght) {
                    // Read the fields                        
                    target = fields[targetField].Trim().ToLower();
                    relatum = fields[relatumField].Trim().ToLower();
                    if(tryAddSim(target, relatum, fields[simField], out inconsistency)) usedPairsNum++;
                    if(inconsistency) inconsistPairsNum++;
                    if (tryAddSim(relatum, target, fields[simField], out inconsistency)) usedPairsNum++;
                    if(inconsistency) inconsistPairsNum++;
                }
            }

            sr.Close();
            Console.WriteLine("Pairs={0}; Pairs found={1}; Inconsistent pairs={2}",
                _relationsCount, usedPairsNum, inconsistPairsNum);
        }
        
        /// <summary>
        /// Try to add a similarity.
        /// </summary>        
        private bool tryAddSim(string target, string relatum, string sim, out bool inconsistency) {
            double simNum;
            inconsistency = false;
            if (this.ContainsKey(target) && this[target].ContainsKey(relatum)) {
                simNum = (double.TryParse(sim, out simNum) ? simNum : 0);
                //if (this[target][relatum].sim != 0) Console.Write("Duplicate relation: ({0},{1},{2}?={3})\n", target, relatum, this[target][relatum].sim, sim);
                if (this[target][relatum].sim != 0 && this[target][relatum].sim != simNum) {
                    inconsistency = true;
                    Console.Write("Duplicate relation: ({0},{1},{2}!={3})\n", target, relatum, this[target][relatum].sim, simNum);
                }
                this[target][relatum].sim = simNum;
                return true;
            }
            return false;
        }

        /// <summary>
        /// For a given target word generates as many random relations as many 
        /// relations this target has.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public List<string> getRandomWords(string target) {
			// Ensure that vocabulary is shuffled!
			
            // Check if the input parameters are correct
            if (!this.ContainsKey(target)) {
                Console.WriteLine("The target '{0}' was not found!", target);
                return new List<string>();
            }

            // Get vocabulary of random candidates (not targets or relatums)
            List<string> notRandomVoc = this[target].Keys.ToList();
            notRandomVoc.Add(target);
            List<string> randomVoc = _shuffledVocabulary;
            randomVoc = randomVoc.Except(notRandomVoc).ToList();

            // Generate a random index
            Random rnd = new Random(DateTime.Now.Millisecond);
            int randomIndex = rnd.Next(randomVoc.Count - notRandomVoc.Count);

            // Get as much random words as much relatums 
            List<string> randomRelatums = new List<string>();
            for (int relatumNum = 0; relatumNum < this[target].Keys.Count; relatumNum++) {
                string randomWord = randomVoc[randomIndex + relatumNum];
                // Ensure at least surface disimilarity
                bool notEnougthRandom = surfaceMatch(target, randomWord);
                while (notEnougthRandom) {
                    // Try to find better word
                    int newRandomIndex = rnd.Next(randomVoc.Count - notRandomVoc.Count);
                    randomWord = randomVoc[newRandomIndex];
                    notEnougthRandom = surfaceMatch(target, randomWord) || randomRelatums.Contains(randomWord);
                }
                // Add the random relatum 
                randomRelatums.Add(randomWord);
            }

            return randomRelatums;
        }

        private static bool surfaceMatch(string target, string randomWord) {
            return randomWord.Contains(target) || target.Contains(randomWord);
        }

        /// <summary>
        /// Extension method for Fisher-Yates shuffle
        /// </summary>
        private static List<string> shuffle(List<string> list) {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                string value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public bool isTransformed;

        /// <summary>
        /// Normalize the similarity scores to [0;1] range
        /// sim' = (sim - min(sim)) / (max(sim) - min(sim))
        /// </summary>
        public void rerankEfreq () {            
			if (isTransformed) {
				Console.WriteLine ("These raw frequency scores of this relation collection are already normalized");
				return;
			}
			
			// Get max and min sim
            double max = 0;
            double min = 0;
            foreach (var target in this) {
                foreach (var relatum in target.Value.Values) {
                    if (relatum.sim > max) max = relatum.sim;
                    if (relatum.sim < min) min = relatum.sim;
                }
            }

            // Normalize 
            foreach (var target in this)
                foreach (var relatum in target.Value.Values)
                    relatum.sim = (relatum.sim - min) / (max - min);

            isTransformed = true;
        }
		
		/// <summary>
        /// Normalize the raw relation frequencies to the probability
		/// as P(w_i,w_j) = r_ij / sim_ij(r_ij).
        /// </summary>
        public void normProb () {            
			if (isTransformed) {
				Console.WriteLine ("These raw frequency scores of this relation collection are already normalized");
				return;
			}
			
			// Get total number of extractions
			double extractionsNum = 0;
			foreach (var target in this) {
				foreach (var relatum in target.Value.Values) {
					extractionsNum += relatum.sim;
				}
			}

			// Normalize 
			foreach (var target in this) 
				foreach (var relatum in target.Value.Values)
					relatum.sim = relatum.sim / extractionsNum;

			#region DEBUG
			// Check that the distribution sums to one
			//double sum = 0;
			//int num = 0;
			//foreach (var target in this) {
			//	foreach (var relatum in target.Value) {
			//		sum += relatum.Value.sim;
			//		num++;
			//		//Console.WriteLine ("P({0},{1})={2}",
			//		//                   target.Key, relatum.Key, relatum.Value.sim);
			//	}
			//}
			//Console.WriteLine ("sum_ij(P(w_i,w_j)) = {0}, UniqueExtractedPairs={1}, ExtractedPairs={2}",
			//                   sum, num, extractionsNum);
			#endregion
			
            isTransformed = true;
        }

        /// <summary>
        /// Gets the Rfreq counts -- a summary frequency of relations per each word.
        /// </summary>
        private Dictionary<string, double> getRelationsFreq () {
			Dictionary<string, double> relationsFreq = new Dictionary<string, double>();
        	foreach (var target in this) {
				relationsFreq.Add(target.Key, 0);
				foreach (var relatum in target.Value.Values) {
                	relationsFreq[target.Key] += relatum.sim;
				}
        	}
			return relationsFreq;
		}
        

        /// <summary>
        /// Normalize pair frequency according to the branching factor:
        /// sim' = (2*alpha*r_ij)/(r_i# + r_#j), where r_ij -- is the number of 
        /// extractions between words w_i and w_j. 
        /// Default value of alpha = 10 (10 relations per word)
        /// </summary>
        public void rerankEfreqRfreq (double alpha) {
			if (isTransformed) {
				Console.WriteLine ("These raw frequency scores of this relation collection are already normalized");
				return;
			}
			
			// Get number of extractions per term
            Dictionary<string, double> relationsFreq = getRelationsFreq ();
			
		    // Normalize 
			double prevRfreq = 1;
			double targetRfreq;
			double relatumRfreq;
			
            foreach (var target in this) {
                foreach (var relatum in target.Value) {
		            if (relationsFreq.ContainsKey(target.Key) && relationsFreq.ContainsKey(relatum.Key)){
						targetRfreq = relationsFreq[target.Key];
						relatumRfreq =  relationsFreq[relatum.Key];	
					}
					else if(relationsFreq.ContainsKey(target.Key)) {
						Console.WriteLine("Warning: Input file is not symmetric for word '{0}'. Reranking may be wrong.", relatum.Key);
						targetRfreq = relationsFreq[target.Key];
						relatumRfreq = targetRfreq;
					}
					else{
						Console.WriteLine("Error: can not compute relation frequency for the words '{0}' and {1}. Reranking may be wrong.",
						                  target.Key, relatum.Key);
						targetRfreq = prevRfreq;
						relatumRfreq = prevRfreq;
					}
					relatum.Value.sim =
                        	2 * alpha * relatum.Value.sim / (targetRfreq + relatumRfreq);
					prevRfreq = targetRfreq;
                }
            }

            isTransformed = true;
        }

		/// <summary>
		/// Gets the median of an array of integers.
		/// </summary>
		private static double getMedian(int[] sourceNumbers) {
     		if (sourceNumbers == null || sourceNumbers.Length == 0)
            	return 0;

        	// Make sure the list is sorted, but use a new array
        	int[] sortedPNumbers = (int[])sourceNumbers.Clone();
        	sourceNumbers.CopyTo(sortedPNumbers, 0);
        	Array.Sort(sortedPNumbers);

        	// Get the median
        	int size = sortedPNumbers.Length;
        	int mid = size / 2;
        	double median = (size % 2 != 0) ? (double)sortedPNumbers[mid] : ((double)sortedPNumbers[mid] + (double)sortedPNumbers[mid - 1]) / 2;
        	return median;
    	}
		
        /// <summary>
        /// Normalize pair frequency according to the branching factor
        /// sim' = 2*mean(b)*r_ij/(b_i + b_j), where b_i -- is the number of 
        /// extractions with frequency more than minFreq for a w_i. 2*mean(b) 
		/// serves only for readability of the output and does not influence the 
		/// ranking.
        /// </summary>
        public void rerankEfreqRnum (int minFreq) {
			if (isTransformed) {
				Console.WriteLine ("These raw frequency scores of this relation collection are already normalized");
				return;
			}
			
			// Get number of extractions per term with frequency for than minFreq
			Dictionary<string, int> relationsNum = getRelationsNum (minFreq);
			
			// Calculate mean number of relations per word
			int[] nums = relationsNum.Values.ToArray ();	
			double meanNums = (double)nums.Sum () / nums.Length;

			// Normalize 
			double prevRnum = 1;
			double targetRnum;
			double relatumRnum;
		
			foreach (var target in this) {
				foreach (var relatum in target.Value) {
					if (relationsNum.ContainsKey(target.Key) && relationsNum.ContainsKey(relatum.Key)){
						targetRnum = relationsNum[target.Key];
						relatumRnum =  relationsNum[relatum.Key];	
					}
					else if(relationsNum.ContainsKey(target.Key)) {
						Console.WriteLine("Warning: Input file is not symmetric for word '{0}'. Reranking may be wrong.", relatum.Key);
						targetRnum = relationsNum[target.Key];
						relatumRnum = targetRnum;
					}
					else{
						Console.WriteLine("Error: can not compute relation frequency for the words '{0}' and {1}. Reranking may be wrong.",
						                  target.Key, relatum.Key);
						targetRnum = prevRnum;
						relatumRnum = prevRnum;
					}
					
					relatum.Value.sim =
                        2 * meanNums * relatum.Value.sim / (targetRnum + relatumRnum);
                }
            }

            isTransformed = true;
        }
				
		/// <summary>
        /// Normalize pair frequency according to the frequencies of words derived from 
		/// a corpus (used for extraction) or an extraction concordance. 
        /// type == 1: sim = r_ij/(f_i + f_j), where w_i -- is the number of 
        /// times the word w_i appeared in the corpus/concordance.
		/// type == 2: sim = P(r_ij)/(P(w_i)P(w_j)
		/// type == 3: sim = -log P(r_ij)/(P(w_i)P(w_j)
		/// where P(r_ij) = r_ij / sum_ij(r_ij), P(w_i) = f_i / sum_i(f_i)
		/// </summary>
        public void rerankEfreqCfreq (string corpusFreqFile, int type) {
			if (isTransformed) {
				Console.WriteLine ("These raw frequency scores of this relation collection are already normalized");
				return;
			}
			
			// Load the word frequencies
			var corpusFreq = new IVocabulary (corpusFreqFile, false);
			
			// Reranking
			double meanFreq = corpusFreq.getMeanFreq ();
			Console.WriteLine ("mean frequency={0}", meanFreq);
			int missingWordsNum = 0;
							
			if (type == 1) {			
				double targetFreq = 0;
				double relatumFreq = 0;			
				foreach (var target in this) {
					foreach (var relatum in target.Value) {
						targetFreq = 
							(corpusFreq.ContainsKey (target.Key) ? corpusFreq [target.Key] [IVocabulary.FREQ] : meanFreq);
						relatumFreq = 
							(corpusFreq.ContainsKey (relatum.Key) ? corpusFreq [relatum.Key] [IVocabulary.FREQ] : meanFreq);
						if (corpusFreq.ContainsKey (relatum.Key) || corpusFreq.ContainsKey (target.Key))
							missingWordsNum++;
						
						relatum.Value.sim =
							relatum.Value.sim / (targetFreq + relatumFreq);											
					}
				}				
			} else if (type == 2 || type == 3) {
				double targetProb = 0;
				double relatumProb = 0;
				double meanProb = meanFreq / corpusFreq.getTokensNum ();
				var corpusProb = corpusFreq.getProb ();
				this.normProb ();
				
				foreach (var target in this) {
					foreach (var relatum in target.Value) {
						targetProb = 
							(corpusProb.ContainsKey (target.Key) ? corpusProb [target.Key] : meanProb);
						relatumProb = 
							(corpusProb.ContainsKey (relatum.Key) ? corpusProb [relatum.Key] : meanProb);
						if (corpusProb.ContainsKey (relatum.Key) || corpusProb.ContainsKey (target.Key))
							missingWordsNum++;
					
						relatum.Value.sim =
							relatum.Value.sim / (targetProb + relatumProb);											
						if (type == 3)
							relatum.Value.sim = - Math.Log (relatum.Value.sim);
					}
				}	
			} else {
				Console.WriteLine ("Error: wrong type of normalization.");
				return;
			}
			
			Console.WriteLine ("missing words = {0}", missingWordsNum);
			
            isTransformed = true;            
        }

		/// <summary>
        /// Normalize pair frequency according to the frequencies of words derived from 
		/// a corpus and the branching factor of extracted relations.  
        /// sim = (2*mean_i(b_i)/(b_i+b_j)) * P(r_ij)/(P(w_i)*P(w_j))
		/// where P(r_ij) = r_ij / sum_ij(r_ij), P(w_i) = f_i / sum_i(f_i),
		/// b_i is the number of extractions with frequency more than minFreq.
		/// </summary>
        public void rerankEfreqCfreqRnum (string corpusFreqFile, int minFreq) {
			if (isTransformed) {
				Console.WriteLine ("These raw frequency scores of this relation collection are already normalized");
				return;
			}
			
			// Load the word frequencies
			var corpusFreq = new IVocabulary (corpusFreqFile, false);
			
			// Get number of extractions per term with frequency for than minFreq
			Dictionary<string, int> relationsNum = getRelationsNum (minFreq);
			
			// Calculate mean number of relations per word
			int[] branchesArray = relationsNum.Values.ToArray ();	
			double meanBranchesNum = (double)branchesArray.Sum () / branchesArray.Length;
			
			// Probabilities of words 
			double meanProb = corpusFreq.getMeanFreq () / corpusFreq.getTokensNum ();
			var corpusProb = corpusFreq.getProb ();
			
			// Probability of relations
			this.normProb ();			
			
			// Reranking
			int missingWordsNum = 0;			
			double targetProb = 0;
			double relatumProb = 0;				
			double prevRnum = 1;
			double targetRnum;
			double relatumRnum;
		
			foreach (var target in this) {
				foreach (var relatum in target.Value) {
					targetProb = 
						(corpusProb.ContainsKey (target.Key) ? corpusProb [target.Key] : meanProb);
					relatumProb = 
						(corpusProb.ContainsKey (relatum.Key) ? corpusProb [relatum.Key] : meanProb);
					if (corpusProb.ContainsKey (relatum.Key) || corpusProb.ContainsKey (target.Key))
						missingWordsNum++;
		
					if (relationsNum.ContainsKey(target.Key) && relationsNum.ContainsKey(relatum.Key)){
						targetRnum = relationsNum[target.Key];
						relatumRnum = relationsNum[relatum.Key];	
					}
					else if(relationsNum.ContainsKey(target.Key)) {
						Console.WriteLine("Warning: Input file is not symmetric for word '{0}'. Reranking may be wrong.", relatum.Key);
						targetRnum = relationsNum[target.Key];
						relatumRnum = targetRnum;
					}
					else{
						Console.WriteLine("Error: can not compute relation frequency for the words '{0}' and {1}. Reranking may be wrong.",
						                  target.Key, relatum.Key);
						targetRnum = prevRnum;
						relatumRnum = prevRnum;
					}			
					
					relatum.Value.sim =
						(2 * meanBranchesNum / (targetRnum + relatumRnum)) * (relatum.Value.sim / (targetProb + relatumProb)) ;											
				}
			}			
            isTransformed = true;            
        }
		
		/// <summary>
        /// Multiply each sim by patternsNum if it is > 0 or by sqrt(patternsNum) if sqrt is true.
		/// </summary>
        public void multiplyPnum(string patternsNumFile, bool sqrt) {

			this.loadPatternNums(patternsNumFile, false);
			
			double coeff;
			foreach(var target in this) {
				foreach(var relatum in target.Value) {
					if(relatum.Value.patternsNum > 0) {
						if(sqrt)
							coeff = Math.Sqrt(relatum.Value.patternsNum);
						else
							coeff = relatum.Value.patternsNum;
					}
					else {
						coeff = 1;
					}
					relatum.Value.sim *= coeff;											
				}
			}			
			
            isTransformed = true;            
        }
		
		/// <summary>
		/// Returns a dictionary with the number of branches (related words)
		/// with at least minFreq extractons for all target words.
		/// </summary>
		public Dictionary<string, int> getRelationsNum (int minFreq) {
			Dictionary<string, int> extractionsNum = new Dictionary<string, int> ();
			foreach (var target in this) {
				extractionsNum.Add (target.Key, 0);
				foreach (var relatum in target.Value.Values) {
					if (relatum.sim >= minFreq)
						extractionsNum [target.Key]++;
				}
			}
			return extractionsNum;
			
		}		
		
		/// <summary>
		/// Gets the number of different patterns used in the extraction.
		/// </param>
		private int getPatternsNum (string[] fields, bool blessFormat){
			int prefixFieldsNum = (blessFormat ? 4 : 3);
			if(fields.Length >= (prefixFieldsNum + 1)) {
				return fields.Length - prefixFieldsNum;
			}
			else {
				Console.WriteLine("Bad field!");
				return 0;
			}
		}		
		
		/// <summary>
		/// Loads number of extracted patterns from an external source.
		/// Input: an extended bless frame "target;relatum;type;sim;p1;p2;...;pn",
		/// pairs and the number of patterns "p1;p2;...;pn" -- n in this case. 
		/// The information is loaded into the patternsNum field.
		/// If blessFormat is false then the field blessFormat is absent. 
		/// </summary>
		public void loadPatternNums (string relationsFile, bool blessFormat){
			// Initialization
			StreamReader sr = new StreamReader (relationsFile);
			string[] fields;
			string line, target, relatum;
			int patternsNum;
			int targetField = 0;
			int relatumField = 1;
			int errorsNum = 0;
			int relationsNum = 0;
			int relationsUpdatedNum = 0;
			           
			// Read file with relations line by line
			while ((line = sr.ReadLine()) != null) {
				// Read the fields from the current line                
				fields = line.Split (new char[] { ';' });
				
				// Add entry if the line is well-formed
				if (fields.Length >= 4) {
					// Read the fields                        
					target = fields [targetField].Trim ().ToLower ();
					relatum = fields [relatumField].Trim ().ToLower ();
					patternsNum = getPatternsNum (fields, blessFormat);
					
					if (this.ContainsKey(target) && this [target].ContainsKey (relatum) && patternsNum > 0) {
						this [target] [relatum].patternsNum =
							Math.Max (this [target] [relatum].patternsNum, patternsNum);                    
						relationsUpdatedNum++;
					} else if (patternsNum == 0) {
						errorsNum++;						
					} else {
						// A pair does not exist in the loaded dictionary -- skip it                    
					}
					relationsNum++;
				} 
				else {
					errorsNum++;					
				}				
			}
			sr.Close ();
			
			#region DEBUG
			/*
			foreach (var t in this) {
				foreach (var r in t.Value) {
					Console.WriteLine("({0},{1}) patterns_num={2}",
							   t.Key, r.Key, r.Value.patternsNum);
				}
			}
			*/
			#endregion
			Console.WriteLine ("InputRelations={0}, InputRelationsWithoutPatterns={1}, RelationsUpdated={2}",
			                   relationsNum, errorsNum, relationsUpdatedNum);
        }
        
		/// <summary>
		/// Extracts the features for a input frame extract 'target;relatum;type;sim'.
		/// extract -- A file with a set of relations. The features will be extracted from these relations. The file is in the format 'target;relatum;type'
		/// freq -- A word frequency list in the format of a CSV file 'word;frequency'.
		/// patterns -- A CSV file 'target;relatum;sim;p1;p2;...;pn' with information about extraction patterns.
		/// </summary>
		public void extractFeatures(string extractFile, string freqFile, string patternsFile, string outputFile) {
			/*
			int MIN_BRANCH_FREQ = 1;
			
			// Load data
			RelationsCollection extractRelations = new RelationsCollection(extractFile, true);
			// Fill similarity with the counts from this
			extractRelations.loadPatternNums(patternsFile, false); // Add to PairInfo bool[] with patterns
			var branches = getRelationsNum(MIN_BRANCH_FREQ);
			var corpusFreq = new IVocabulary(freqFile, false);
			var corpusProb = corpusFreq.getProb();
			
			// Save the data
			StreamWriter outputStm = new StreamWriter(outputFile, false, Encoding.UTF8);			
			foreach(var target in this) {
				foreach(var relatum in target.Value) {
					#region DEBUG OUTPUT
					
					if (target.Key == "hockey") {
						Console.WriteLine ("({0},{1}):P(r_ij)={2}->{3}, P(w_i)={4}, P(w_j)={5}, b_i={6}, b_j={7}",
						   target.Key, relatum.Key, relatum.Value.sim,
					   	   (2 * meanBranchesNum / (branchesNum [target.Key] + branchesNum [relatum.Key])) * (relatum.Value.sim / (targetProb + relatumProb)),
					   	   targetProb, relatumProb, branchesNum [target.Key], branchesNum [relatum.Key]);
					}
					
					#endregion
					
					// Save relations and their features to a CSV file 
					//corpusFreq[target.Key]
					//corpusFreq[relatum.Key]
					//corpusProb[target.Key]
					//corpusProb[relatum.Key]
					//branchesNum[target.Key]
					//branchesNum[relatum.Key])
					//relatum.Value.sim
					//relatum.Value.patternsNum
					//relatum.Value.patterns[0]
					//relatum.Value.patterns[1]
					//relatum.Value.patterns[..]
					
				}
			}	
			outputStm.Close();
			*/
		}		
    }
}