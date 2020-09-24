//	Purpose: Common tools
//	History:
//		- 20200904 JC: Created
//		- 20200924 JC: Refactored into individual class files per standard C# convention.

using System.Collections;
using System.Collections.Generic;
using static X9.Terminal;


namespace X9 {

	//	History:
	//		- 20200907 JC: Created.
	public class DictionaryOfLists : System.Collections.IEnumerable {

		private readonly Dictionary<dynamic, List<dynamic>> _dictionary = new Dictionary<dynamic, List<dynamic>>();

		public long Count { get { return _dictionary?.Count ?? 0; } }

		public bool ContainsKey(dynamic argKey) {
			return _dictionary.ContainsKey(argKey);
		}

		// Add values to a list. List can already exist or not. New values are appended to list.
		public void Add(in dynamic argKey, params dynamic[] argVals) {

			// Either get matching list or create a new one and add it to dictionary.
			if ( ! _dictionary.TryGetValue(argKey, out List<dynamic> list) ) {
				// Key doesn't already exist; Create a new list and add it with the given key
				list = new List<dynamic>();
				_dictionary.Add(argKey, list);
				EchoIfDebug_v1($"{this.GetType()}.Add(): argKey = '{argKey}'");
			}

			// Add the values to the list
			foreach ( var val in argVals ) {
				EchoIfDebug_v1($"{this.GetType()}.Add(): argKey = '{argKey}', val = '{val}'.");
				list.Add(val);
			}

		}

		public List<dynamic> GetValue(in dynamic argKey) {
			if ( _dictionary.TryGetValue(argKey, out List<dynamic> retList) ) {
				return retList;
			} else {
				return null;
			}
		}

		public bool TryGetValue( in dynamic argKey, out List<dynamic> outList ) {
			bool retVal=false;
			if ( _dictionary.TryGetValue(argKey, out List<dynamic> retList) ) {
				outList = retList;
				retVal=true;
			} else {
				outList = null;
			}
			return retVal;
		}

		public long ListCount(in dynamic argKey) {
			long retVal=0;
			if ( _dictionary.TryGetValue(argKey, out List<dynamic> outList) ) {
				retVal = (long)outList.Count;
			}
			return retVal;
		}

		// Required by IEnmerator interface.
		public IEnumerator GetEnumerator() {
			foreach (KeyValuePair<dynamic, List<dynamic>> item in _dictionary) {
				yield return item;
			}
		}
	}

}