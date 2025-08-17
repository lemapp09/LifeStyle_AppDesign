using System.Collections.Generic;
using UnityEngine;

namespace AppDesign
{

    [CreateAssetMenu(fileName = "New Quote", menuName = "Tools/Quote Data")]
    public class QuoteData : ScriptableObject
    {
        public string quoteText;
        public string author;
    }
    
    
    [CreateAssetMenu(fileName = "Quote Database", menuName = "Tools/Quote Database")]
    public class QuoteDatabase : ScriptableObject
    {
        public List<QuoteData> quotes;
    }
}