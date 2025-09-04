using System.Collections.Generic;
using UnityEngine;

// Use the CreateAssetMenu attribute to allow creating a new instance
// of this ScriptableObject from the Unity Editor's asset menu.
[CreateAssetMenu(fileName = "New Deck Of Cards", menuName = "Deck of Cards", order = 1)]
public class PlayingCards_SO  : ScriptableObject
{
    // A list to hold all the standard 52 playing cards.
    // The SerializeField attribute makes this list visible and editable
    // in the Unity Inspector.
    [SerializeField]
    public List<CardData> StandardCards = new List<CardData>();
    
    // A list to hold the special cards like Jokers and card backs.
    [SerializeField]
    public List<CardData> SpecialCards = new List<CardData>();

    // This method can be used to populate the standard deck data automatically.
    // You would call this from a custom editor script or a context menu.
    // This is optional but can speed up initial setup.
    public void GenerateStandardDeck()
    {
        StandardCards.Clear();
        
        // Populate the list with all 52 standard cards.
        foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
        {
            if (suit == CardSuit.None) continue; // Skip the 'None' value
            
            foreach (CardValue value in System.Enum.GetValues(typeof(CardValue)))
            {
                if (value == CardValue.None) continue; // Skip the 'None' value
                
                CardData newCard = new CardData
                {
                    cardSuit = suit,
                    cardValue = value,
                    cardName = $"{value} of {suit}",
                    cardSprite = null // You'll assign this in the Inspector
                };
                StandardCards.Add(newCard);
            }
        }

        // Add the special cards to their list.
        SpecialCards.Clear();
        SpecialCards.Add(new CardData { cardName = "Joker 1", cardSprite = null });
        SpecialCards.Add(new CardData { cardName = "Joker 2", cardSprite = null });
        SpecialCards.Add(new CardData { cardName = "Card Back 1", cardSprite = null });
        SpecialCards.Add(new CardData { cardName = "Card Back 2", cardSprite = null });
    }
}

// System.Serializable makes this class's fields visible in the Inspector
// even though it doesn't inherit from MonoBehaviour or ScriptableObject.
[System.Serializable]
public class CardData
{
    // The value of the card (Ace, 2, 3, etc.).
    public CardValue cardValue;
    
    // The suit of the card (Hearts, Spades, etc.).
    public CardSuit cardSuit;
    
    // A human-readable name for the card.
    public string cardName;
    
    // A reference to the card's sprite asset.
    public Sprite cardSprite;
}

// Enums provide a clean way to define the suits and values.
public enum CardSuit
{
    None, // A default value for special cards that don't have a suit
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public enum CardValue
{
    None, // A default value for special cards that don't have a value
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13
}