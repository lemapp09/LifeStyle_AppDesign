using UnityEngine;
using UnityEngine.UIElements;

namespace AppDesign
{

    public class QuoteManager : MonoBehaviour
    {
        // Drag and drop your "AllQuotes" database asset here in the Inspector
        public QuoteDatabase quoteDatabase;
        private Label _quoteDropCap, _quoteRestOfText, _quoteAuthor;

        public void QuoteStart()
        {
            if (quoteDatabase != null && quoteDatabase.quotes.Count > 0)
            {
                // Example: Get a random quote from the database
                int randomIndex = Random.Range(0, quoteDatabase.quotes.Count);
                QuoteData randomQuote = quoteDatabase.quotes[randomIndex];

                // Debug.Log($"Today's quote is: \"{randomQuote.quoteText}\" - {randomQuote.author}");
                _quoteDropCap.text = randomQuote.quoteText.Substring(0,1)  ; // The first character of randomQuote.quoteText
                _quoteRestOfText.text = randomQuote.quoteText.Substring(1);
                _quoteAuthor.text = randomQuote.author;
            }
        }

        public void SetUIElements(Label quoteDropCap, Label quoteRestOfText, Label quoteAuthor)
        {
            _quoteDropCap = quoteDropCap;
            _quoteRestOfText = quoteRestOfText;
            _quoteAuthor = quoteAuthor;
        }
    }
}