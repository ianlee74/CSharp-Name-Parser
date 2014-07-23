using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            var i = 0;

		    // split into words
		    // completely ignore any words in parentheses
            var nameParts = fullName.Split(' ').Where(w => !w.StartsWith("(")).ToList();
            var numWords = nameParts.Count();

		    // is the first word a title? (Mr. Mrs, etc)
		    var salutation = IsSalutation(nameParts[0]);
		    var suffix = IsSuffix(nameParts[numWords - 1]);
		    // set the range for the middle part of the name (trim prefixes & suffixes)
		    var start = !String.IsNullOrEmpty(salutation) ? 1 : 0;
		    var end = !String.IsNullOrEmpty(suffix) ? numWords - 1 : numWords;

		    var word = nameParts[start];
		    // if we start off with an initial, we'll call it the first name
		    if (IsInitial(word)) 
            {
			    // if so, do a look-ahead to see if they go by their middle name 
			    // for ex: "R. Jason Smith" => "Jason Smith" & "R." is stored as an initial
			    // but "R. J. Smith" => "R. Smith" and "J." is stored as an initial
			    if (IsInitial(nameParts[start + 1]))
			    {
			        firstName += " " + word.ToUpper();
			    } else
			    {
			        initials += " " + word.ToUpper();
			    }
		    } else {
			    firstName += " " + FixCase(word);
		    }

		    // concat the first name
		    for (i=start + 1; i<(end - 1); i++) {
			    word = nameParts[i];
			    // move on to parsing the last name if we find an indicator of a compound last name (Von, Van, etc)
			    // we do not check earlier to allow for rare cases where an indicator is actually the first name (like "Von Fabella")
			    if (IsCompoundLastName(word)) break;

			    if (IsInitial(word))
			    {
			        initials += " " + word.ToUpper();
			    }
                else
                {
				    firstName += " " + FixCase(word);
			    }
		    }
		
		    // check that we have more than 1 word in our string
		    if ((end - start) > 1)
		    {
		        // concat the last name
		        var j = 0;
		        for (j=i; j<end; j++) {
				    lastName += " " + this.FixCase(nameParts[j]);
			    }
		    }

            // return the various parts in an array
		    return new Name()
                {
			        Salutation = salutation ?? "",
			        FirstName = firstName.Trim(),
			        MiddleInitials = initials.Trim(),
			        LastName = lastName.Trim(),
			        Suffix = suffix ?? ""
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
	    public string IsSuffix( string word ) 
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
	        return suffix ?? "";
	    }

        // detect compound last names like "Von Fange"
	    public bool IsCompoundLastName(string word) 
        {
		    word = word.ToLower();
		    // these are some common prefixes that identify a compound last names - what am I missing?
	        var words = new[]{ "vere", "von", "van", "de", "del", "della", "di", "da", "pietro", "vanden", "du", "st.", "st", "la", "lo", "ter" };
	        return !String.IsNullOrEmpty(words.FirstOrDefault(w => w == word));
        }

        // single letter, possibly followed by a period
	    public bool IsInitial(string word) 
        {
		    word = RemoveIgnoredChars(word);
		    return (word.Length == 1);
	    }

        // detect mixed case words like "McDonald"
	    // returns false if the string is all one case
	    public bool IsCamelCase(string word)
	    {
	        var ucReg = new Regex("[A-Z]+");
		    var lcReg = new Regex("[a-z]+");
		    return (ucReg.IsMatch(word) && lcReg.IsMatch(word));
	    }

        // ucfirst words split by dashes or periods
	    // ucfirst all upper/lower strings, but leave camelcase words alone
	    public string FixCase(string word) 
        {
		    // uppercase words split by dashes, like "Kimura-Fay"
		    word = SafeUcFirst('-',word);
		    // uppercase words split by periods, like "J.P."
		    word = SafeUcFirst('.',word);
		    return word;
	    }

        // helper for this.FixCase
	    // uppercase words split by the seperator (ex. dashes or periods)
	    public string SafeUcFirst(char seperator, string word) 
        {
		    var words = word.Split(seperator);
	        var newWord = new StringBuilder();
	        foreach (var thisWord in words)
	        {
                if (newWord.Length > 0) newWord.Append(seperator);
	            if (IsCamelCase(thisWord))
	            {
	                newWord.Append(thisWord);
	            }
	            else
	            {
	                newWord.Append(thisWord.Substring(0, 1).ToUpper() + thisWord.Substring(1).ToLower());
	            }	            
	        }
	        return newWord.ToString();
        }
    }
}
