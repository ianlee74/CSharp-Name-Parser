using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharpNameParser
{
    public class NameParser
    {
        /// <summary>
        /// split full names into the following parts:
        /// - prefix / salutation  (Mr., Mrs., etc)
        /// - given name / first name
        /// - middle initials
        /// - surname / last name 
        /// - suffix (II, Phd, Jr, etc)
        /// </summary>
        /// <param name="fullName">Full name</param>
        /// <returns></returns>
        public Name Parse(string fullName)
        {
            fullName = fullName.Trim();

            var lastName = "";
		    var firstName = "";
		    var initials = "";
		    var word = null;
		    var j = 0;
		    var i = 0;

		    // split into words
		    // completely ignore any words in parentheses
            var nameParts = (fullName.Split(' ')).FirstOrDefault(w => w.Substring(0, 1) != "(");
            var numWords = nameParts.Count();

		// is the first word a title? (Mr. Mrs, etc)
		var salutation = this.IsSalutation(nameParts[0]);
		var suffix = this.is_suffix(nameParts[numWords - 1]);
		// set the range for the middle part of the name (trim prefixes & suffixes)
		var start = (salutation) ? 1 : 0;
		var end = (suffix) ? numWords - 1 : numWords;

		word = nameParts[start];
		// if we start off with an initial, we'll call it the first name
		if (this.is_initial(word)) {
			// if so, do a look-ahead to see if they go by their middle name 
			// for ex: "R. Jason Smith" => "Jason Smith" & "R." is stored as an initial
			// but "R. J. Smith" => "R. Smith" and "J." is stored as an initial
			if (this.is_initial(nameParts[start + 1])) {
				firstName += " " + word.toUpperCase();
			} else {
				initials += " " + word.toUpperCase();
			}
		} else {
			firstName += " " + this.fix_case(word);
		}

		// concat the first name
		for (i=start + 1; i<(end - 1); i++) {
			word = nameParts[i];
			// move on to parsing the last name if we find an indicator of a compound last name (Von, Van, etc)
			// we do not check earlier to allow for rare cases where an indicator is actually the first name (like "Von Fabella")
			if (this.is_compound_lastName(word)) {
				break;
			}

			if (this.is_initial(word)) {
				initials += " " + word.toUpperCase(); 
			} else {
				firstName += " " + this.fix_case(word);
			}
		}
		
		// check that we have more than 1 word in our string
		if ((end - start) > 1) {
			// concat the last name
			for (j=i; j<end; j++) {
				lastName += " " + this.fix_case(nameParts[j]);
			}
		}
	
		// return the various parts in an array
		return {
			"salutation": salutation || "",
			"firstName": firstName.trim(),
			"initials": initials.trim(),
			"lastName": lastName.trim(),
			"suffix": suffix || ""
		};
        }

        protected string RemoveIgnoredChars(string word) 
        {
		    //ignore periods
		    return word.Replace(".","");
	    }

        // detect and format standard salutations 
	    // I'm only considering english honorifics for now & not words like 
	    public string IsSalutation(string word)
	    {
	        word = RemoveIgnoredChars(word).ToLower();
		    // returns normalized values
	        switch (word)
	        {
	            case "mister":
	            case "master":
	            case "mr":
	                return "Mr.";
	            case "mrs":
	                return "Mrs.";
	            case "ms":
	            case "miss":
	                return "Ms.";
	            case "dr":
	                return "Dr.";
	            case "rev":
	                return "Rev.";
	            case "fr":
	                return "Fr.";
	            default:
	                return "";
	        }
	    }

        //  detect and format common suffixes 
	    public string is_suffix( string word ) 
        {
		    word = RemoveIgnoredChars(word).ToLower();
	        var suffixArray = new[]
	        {
	            "I", "II", "III", "IV", "V", "Senior", "Junior", "Jr", "Sr", "PhD", "APR", "RPh", "PE", "MD", "MA", "DMD", "CME",
	            "BVM", "CFRE", "CLU", "CPA", "CSC", "CSJ", "DC", "DD", "DDS", "DO", "DVM", "EdD", "Esq",
	            "JD", "LLD", "OD", "OSB", "PC", "Ret", "RGS", "RN", "RNC", "SHCJ", "SJ", "SNJM", "SSMO",
	            "USA", "USAF", "USAFR", "USAR", "USCG", "USMC", "USMCR", "USN", "USNR"
	        };
	        var suffix = suffixArray.FirstOrDefault(w => w.ToLower() == word);
	        return suffix;
	    }

        // detect compound last names like "Von Fange"
	    public bool is_compound_lastName(string word) 
        {
		    word = word.ToLower();
		    // these are some common prefixes that identify a compound last names - what am I missing?
	        var words = new[]{ "vere", "von", "van", "de", "del", "della", "di", "da", "pietro", "vanden", "du", "st.", "st", "la", "lo", "ter" };
	        return !String.IsNullOrEmpty(words.FirstOrDefault(w => w == word));
        }

        // single letter, possibly followed by a period
	    public bool is_initial(string word) 
        {
		    word = RemoveIgnoredChars(word);
		    return (word.Length == 1);
	    }

        // detect mixed case words like "McDonald"
	    // returns false if the string is all one case
	    public bool is_camel_case(string word)
	    {
	        var ucReg = new Regex("[A-Z]+");
		    var lcReg = new Regex("[a-z]+");
		    return (ucReg.IsMatch(word) && lcReg.IsMatch(word));
	    }

        // ucfirst words split by dashes or periods
	    // ucfirst all upper/lower strings, but leave camelcase words alone
	    public string fix_case(string word) 
        {
		    // uppercase words split by dashes, like "Kimura-Fay"
		    word = safe_ucfirst('-',word);
		    // uppercase words split by periods, like "J.P."
		    word = safe_ucfirst('.',word);
		    return word;
	    }

        // helper for this.fix_case
	    // uppercase words split by the seperator (ex. dashes or periods)
	    public string safe_ucfirst(char seperator, string word) 
        {
		    return word.Split(seperator).Where( 
                thisWord => {
			        if(is_camel_case(thisWord)) {
				        return true;
			        } else {
				        return thisWord.substr(0,1).toUpperCase() + thisWord.substr(1).toLowerCase();
			        }
		        }).join(seperator);
	    }
    }
}
